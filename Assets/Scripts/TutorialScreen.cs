using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialScreen : MonoBehaviour
{
    public Sprite[] tutorialImages;
    public Image tutorialImage;
    public int currentImage = 0;
    public Button nextButton;
    public Button prevButton;
    public TextMeshProUGUI imagesLeft;
    public int tutorialIndex;

    private void Start()
    {
        UpdateImage();
    }
    public void NextImage()
    {
        currentImage++;
        if(currentImage == tutorialImages.Length - 1)
        {
            nextButton.interactable = false;
        }
        else
        {
            nextButton.interactable = true;
        }
        if(tutorialImages.Length != 1)
            prevButton.interactable = true;
        UpdateImage();
    }
    public void PrevImage()
    {
        currentImage--;
        if (currentImage == 0)
        {
            prevButton.interactable = false;
        }
        else
        {
            prevButton.interactable = true;
        }
        if (tutorialImages.Length != 1)
            nextButton.interactable = true;
        UpdateImage();
    }
    void UpdateImage()
    {
        tutorialImage.sprite = tutorialImages[currentImage];
        imagesLeft.text = (currentImage + 1).ToString() + " / " + (tutorialImages.Length).ToString();
    }
    public void Continue()
    {
        SaveGame.currentSave.tutorials[tutorialIndex] = true;
        SaveGame.Save();
        gameObject.SetActive(false);
    }
}
