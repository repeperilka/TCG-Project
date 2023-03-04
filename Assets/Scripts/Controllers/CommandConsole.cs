using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Reflection;
using System;

public class CommandConsole : MonoBehaviour
{
    public GameObject textObject;
    public Transform textParent;
    public bool active;
    public Vector2 openConsolePosition;
    public RectTransform consolePanel;
    public TMP_InputField inputField;

    #region Singleton
    public static CommandConsole Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetActiveConsole(!active);
        }
    }

    public void SetActiveConsole(bool _active)
    {
        active = _active;
        if (active)
        {
            consolePanel.anchoredPosition = openConsolePosition;
        }
        else
        {
            consolePanel.anchoredPosition = Vector2.zero;
        }
    }
    public void SendCommand()
    {
        List<string> splitCommand = new List<string>(inputField.text.Split(' '));
        string command = splitCommand[0];
        splitCommand.RemoveAt(0);

        MethodInfo[] methods = typeof(CommandConsole).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
        Debug.Log("MethodCount: " + methods.Length);

        bool found = false;
        foreach(MethodInfo method in methods)
        {
            if(method.Name == command)
            {
                try
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if(parameters.Length == splitCommand.Count)
                    {
                        List<object> parameterToGive = new List<object>();
                        foreach(string param in splitCommand)
                        {
                            parameterToGive.Add(param);
                        }
                        method.Invoke(this, parameterToGive.ToArray());
                        found = true;
                    }
                }
                catch(Exception e)
                {
                    AddPrompt(e.Message);
                }
            }
        }
        if (!found)
        {
            AddPrompt("Command not found");
        }
        inputField.text = "";
    }
    public void AddPrompt(string _prompt)
    {
        TextMeshProUGUI newText = Instantiate(textObject, textParent).GetComponent<TextMeshProUGUI>();
        newText.text = _prompt;
        newText.gameObject.GetComponent<RectTransform>().sizeDelta = newText.GetPreferredValues();
    }
    public void PrintDeck()
    {
        try
        {
            AddPrompt("Deck[" + SaveGame.currentSave.playerDeck.Count.ToString() + "]");
            foreach (CardInstance card in SaveGame.currentSave.playerDeck)
            {
                AddPrompt(card.cardID);
            }
        }
        catch
        {
            AddPrompt("No save has been loaded");
        }
    }
    public void AddCardToDeck(string _cardId)
    {
        if(SaveGame.currentSave.playerDeck != null)
        {
            SaveGame.AddCardToDeck(new CardInstance(_cardId));
        }
        else
        {
            AddPrompt("No save has been loaded");
        }
    }
    public void AddCardToDeck(string _cardId, string _value)
    {
        if (SaveGame.currentSave.playerDeck != null)
        {
            SaveGame.AddCardToDeck(new CardInstance(_cardId, 0, float.Parse(_value)));
        }
        else
        {
            AddPrompt("No save has been loaded");
        }
    }

}
