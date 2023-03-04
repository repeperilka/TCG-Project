using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuAudio : MonoBehaviour
{
    public AudioSource onHover;
    public AudioSource onClick;
    AudioClip clipOnButtonHover;
    AudioClip clipButtonCancel;
    AudioClip clipButtonClick;
    AudioClip clipDeleteSave;
    AudioClip clipStartingGame;

    private void Start()
    {
        clipOnButtonHover = Resources.Load<AudioClip>("Audio/button_hover");
        clipButtonCancel = Resources.Load<AudioClip>("Audio/button_cancel");
        clipButtonClick = Resources.Load<AudioClip>("Audio/button_click");
        clipDeleteSave = Resources.Load<AudioClip>("Audio/delete_save");
        clipStartingGame = Resources.Load<AudioClip>("Audio/starting_game");
    }
    public void OnButtonHover(EventTrigger _button)
    {
        if (_button.GetComponent<Button>().interactable && _button.gameObject.activeInHierarchy)
        {
            onHover.clip = clipOnButtonHover;
            onHover.Play();
        }
    }
    public void OnButtonCancel()
    {
        onClick.clip = clipButtonCancel;
        onClick.Play();
    }
    public void OnButtonClick()
    {
        onClick.clip = clipButtonClick;
        onClick.Play();
    }
    public void OnDeleteSave()
    {
        onClick.clip = clipDeleteSave;
        onClick.Play();
    }
    public void OnStartingGame()
    {
        onClick.clip = clipStartingGame;
        onClick.Play();
    }
}
