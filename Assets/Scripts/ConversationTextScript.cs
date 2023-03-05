using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ConversationTextScript : MonoBehaviour
{
    public Transform parent;
    public TextMeshProUGUI target;
    public string[] lines;

    float speed = .1f;
    public Image nextButton;

    public void StartConversation(string[] _lines)
    {
        lines = _lines;
        StartCoroutine(ConversationRoutine());
    }
    public IEnumerator ConversationRoutine()
    {
        parent.gameObject.SetActive(true);
        float currentSpeed = speed;
        int currentLine = 0;
        while(currentLine < lines.Length)
        {
            nextButton.gameObject.SetActive(false);
            target.text = "";
            currentSpeed = speed;
            for(int i = 0; i < lines[currentLine].Length; i++)
            {
                target.text += lines[currentLine][i];
                float currentTime = 0;
                while(currentTime < currentSpeed)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        currentSpeed = .01f;
                    }
                    currentTime += Time.deltaTime;
                    yield return 0;
                }
            }
            nextButton.gameObject.SetActive(true);
            while (!Input.GetMouseButtonDown(0))
            {
                nextButton.color = new Color(1f, 1f, 1f, Mathf.Abs(Mathf.Sin(Time.time)));
                yield return 0;
            }
            currentLine++;
        }
        nextButton.gameObject.SetActive(false);
        parent.gameObject.SetActive(false);
    }
    public IEnumerator ConversationRoutine(string[] _lines)
    {
        lines = _lines;
        parent.gameObject.SetActive(true);
        float currentSpeed = speed;
        int currentLine = 0;
        while (currentLine < lines.Length)
        {
            nextButton.gameObject.SetActive(false);
            target.text = "";
            currentSpeed = speed;
            for (int i = 0; i < lines[currentLine].Length; i++)
            {
                target.text += lines[currentLine][i];
                float currentTime = 0;
                while (currentTime < currentSpeed)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        currentSpeed = .01f;
                    }
                    currentTime += Time.deltaTime;
                    yield return 0;
                }
            }
            nextButton.gameObject.SetActive(true);
            while (!Input.GetMouseButtonDown(0))
            {
                nextButton.color = new Color(1f, 1f, 1f, Mathf.Abs(Mathf.Sin(Time.time)));
                yield return 0;
            }
            currentLine++;
        }
        nextButton.gameObject.SetActive(false);
        parent.gameObject.SetActive(false);
    }
}
