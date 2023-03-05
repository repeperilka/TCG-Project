using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{

    public AudioSource musicSource;

    RectTransform titleBackground;
    RectTransform titleParent;
    Image titleName;
    Image titleShadow;

    GameObject mainMenuPanel;
    Button[] mainMenuButtons; //0 = continue, 1 = new game, 2 = load, 3 = settings, 4 = exits

    GameObject newGamePanel;
    Button[] newGameButtons;
    Button[] loadButtons;

    GameObject newGameNamePanel;
    TMP_InputField newGameNameField;
    public Toggle skipLoreToggle;
    public Toggle skipTutorialToggle;
    GameObject noNameWarning;

    GameObject deleteGamePanel;

    public Image transitionPanel;



    public SavePanelMode mode;
    public int newGameIndex;

    // Start is called before the first frame update
    void Start()
    {

        //finding title background
        titleBackground = transform.Find("title_screen").GetComponent<RectTransform>();

        //finding the holder transform of the title name and its shadow
        titleParent = transform.Find("game_title").GetComponent<RectTransform>();
        titleName = titleParent.transform.Find("title_image").GetComponent<Image>();
        titleShadow = titleParent.transform.Find("title_shadow").GetComponent<Image>();

        //finding main menu buttons
        mainMenuPanel = transform.Find("main_menu").gameObject;
        mainMenuButtons = new Button[5];
        mainMenuButtons[0] = mainMenuPanel.transform.Find("Continue").GetComponent<Button>();
        mainMenuButtons[1] = mainMenuPanel.transform.Find("NewGame").GetComponent<Button>();
        mainMenuButtons[2] = mainMenuPanel.transform.Find("Load").GetComponent<Button>();
        mainMenuButtons[3] = mainMenuPanel.transform.Find("Settings").GetComponent<Button>();
        mainMenuButtons[4] = mainMenuPanel.transform.Find("Exit").GetComponent<Button>();
        mainMenuPanel.SetActive(false);

        //finding load/new game buttons
        newGamePanel = transform.Find("new_load_game").gameObject;
        Transform buttonHolder = newGamePanel.transform.Find("buttons");
        newGameButtons = new Button[4];
        loadButtons = new Button[4];
        for(int i = 0; i < 4; i++)
        {
            newGameButtons[i] = buttonHolder.Find("new_game_" + i).GetComponent<Button>();
            loadButtons[i] = buttonHolder.Find("load_game_" + i).GetComponent<Button>();
        }
        newGamePanel.SetActive(false);

        //finding NewGameName panel
        newGameNamePanel = transform.Find("new_game_name").gameObject;
        newGameNameField = newGameNamePanel.transform.Find("field_name").GetComponent<TMP_InputField>();
        noNameWarning = newGameNamePanel.transform.Find("warning_noName").gameObject;
        newGameNamePanel.SetActive(false);

        //find DeleteGame panel
        deleteGamePanel = transform.Find("delete_game").gameObject;
        deleteGamePanel.SetActive(false);

        //start intro animation
        StartCoroutine(TitleScreen());
    }
    private void Update()
    {

    }
    public IEnumerator TitleScreen()
    {
        titleBackground.localScale = Vector3.one * 1.2f;
        titleParent.anchoredPosition = Vector3.zero;
        titleName.color = new Color(1f, 1f, 1f, 0);
        titleShadow.color = new Color(0, 0, 0, 0);
        
        transitionPanel.color = Color.black;
        
        float targetY = Screen.height / 4.32f * -1f;
        float speed = Time.deltaTime * 1.4f;
        
        transitionPanel.CrossFadeAlpha(0, .3f, true);
        
        while(Mathf.Abs(titleBackground.localScale.x - 1f) > .01)
        {
            titleBackground.localScale = Vector3.Lerp(titleBackground.localScale, Vector3.one, Time.deltaTime * 1.4f);
            yield return 0;
        }
        
        yield return new WaitForSeconds(.4f);
        
        while(Mathf.Abs(titleParent.anchoredPosition.y - targetY) > .01f && Mathf.Abs(titleName.color.a - 1f) > .01f)
        {
            titleParent.anchoredPosition = Vector3.Lerp(titleParent.anchoredPosition, new Vector3(0, targetY, 0), Time.deltaTime * 2f);
            titleName.color = Color.Lerp(titleName.color, new Color(1f, 1f, 1f, 1f), Time.deltaTime * 2f);
            titleShadow.color = Color.Lerp(titleShadow.color, new Color(0, 0, 0, 1f), Time.deltaTime * 2f);
            yield return 0;
        }
        titleName.color = Color.white;
        titleShadow.color = Color.black;
        
        yield return new WaitForSeconds(1f);
        
        
        
        mainMenuPanel.SetActive(true);
        int lastLoaded = PlayerPrefs.GetInt("lastPlayed");
        if(lastLoaded != -1)
        {
            mainMenuButtons[0].gameObject.SetActive(true);
        }
        else
        {
            mainMenuButtons[0].gameObject.SetActive(false);
        }
        yield return 0;
    }
    public void Continue()
    {
        int lastPlayed = PlayerPrefs.GetInt("lastPlayed");
        SaveGame.Load(lastPlayed, true);
    }


    public void SetMode(SavePanelMode _mode)
    {
        newGamePanel.gameObject.SetActive(true);
        mode = _mode;
        switch (mode)
        {
            case SavePanelMode.Load:
                for (int i = 0; i < 4; i++)
                {
                    if (SaveGame.savesData[i].saveName == "")
                    {
                        newGameButtons[i].gameObject.SetActive(true);
                        newGameButtons[i].interactable = false;
                        loadButtons[i].gameObject.SetActive(false);
                    }
                    else
                    {
                        newGameButtons[i].gameObject.SetActive(false);
                        loadButtons[i].gameObject.SetActive(true);
                        loadButtons[i].transform.Find("name").GetComponent<TextMeshProUGUI>().text = SaveGame.savesData[i].saveName;
                        loadButtons[i].transform.Find("time").GetComponent<TextMeshProUGUI>().text = SaveGame.savesData[i].timeConverted;
                        loadButtons[i].interactable = true;
                    }
                }
                break;
            case SavePanelMode.NewGame:
                for (int i = 0; i < 4; i++)
                {
                    if (SaveGame.savesData[i].saveName == "")
                    {
                        newGameButtons[i].gameObject.SetActive(true);
                        newGameButtons[i].interactable = true;
                        loadButtons[i].gameObject.SetActive(false);
                    }
                    else
                    {
                        newGameButtons[i].gameObject.SetActive(false);
                        loadButtons[i].gameObject.SetActive(true);
                        loadButtons[i].transform.Find("name").GetComponent<TextMeshProUGUI>().text = SaveGame.savesData[i].saveName;
                        loadButtons[i].transform.Find("time").GetComponent<TextMeshProUGUI>().text = SaveGame.savesData[i].timeConverted;
                        loadButtons[i].interactable = false;
                    }
                }
                break;
        }
    }

    public void OpenMainMenuPanel()
    {
        mainMenuPanel.SetActive(true);
        int lastLoaded = PlayerPrefs.GetInt("lastPlayed");
        if(lastLoaded != -1)
        {
            mainMenuButtons[0].gameObject.SetActive(true);
        }
        else
        {
            mainMenuButtons[0].gameObject.SetActive(false);
        }
    }

    public void OpenNewGamePanel(int _mode)
    {
        SetMode((SavePanelMode)_mode);
        mainMenuPanel.gameObject.SetActive(false);
    }
    public void SetNewGameIndex(int _index)
    {
        newGameIndex = _index;
    }
    public void CloseNewGamePanel()
    {
        newGamePanel.gameObject.SetActive(false);
        mainMenuPanel.gameObject.SetActive(true);
    }

    public void OpenGameNamePanel()
    {
        newGameNamePanel.gameObject.SetActive(true);
        newGameNameField.text = "";
        newGameNameField.Select();
        newGameNameField.ActivateInputField();
    }
    public void CloseGameNamePanel()
    {
        noNameWarning.gameObject.SetActive(false);
        newGameNamePanel.gameObject.SetActive(false);
    }

    public void OpenDeleteGamePanel()
    {
        deleteGamePanel.gameObject.SetActive(true);
    }
    public void CloseDeleteGamePanel()
    {
        deleteGamePanel.SetActive(false);
    }


    public void NewGame()
    {
        if(newGameNameField.text == "")
        {
            noNameWarning.gameObject.SetActive(true);
            OpenGameNamePanel();
        }
        else
        {
            StartCoroutine(LoadGameTransition(newGameIndex, newGameNameField.text, skipLoreToggle.isOn, skipTutorialToggle.isOn));
        }
    }
    public void LoadGame()
    {
        StartCoroutine(LoadGameTransition(newGameIndex, "", false, false));
    }
    public void DeleteGame()
    {
        SaveGame.DeleteGame(newGameIndex);
        SetMode(mode);
    }
    public IEnumerator LoadGameTransition(int _gameIndex, string _gameName, bool _skipLore, bool _skipTutorial)
    {
        transitionPanel.gameObject.SetActive(true);
        deleteGamePanel.gameObject.SetActive(false);
        mainMenuPanel.gameObject.SetActive(false);
        newGameNamePanel.gameObject.SetActive(false);
        newGamePanel.gameObject.SetActive(false);


        StartCoroutine(MusicFadeOut());
        transitionPanel.CrossFadeAlpha(1f, .7f, false);
        yield return new WaitForSeconds(.7f);

        if(_gameName != "")
        {
            SaveGame.NewGame(_gameIndex, _gameName, _skipLore, _skipTutorial);
        }
        else
        {
            SaveGame.Load(_gameIndex, true);
        }
        yield return 0;
    }
    public IEnumerator MusicFadeOut()
    {
        while (musicSource.volume > 0)
        {
            musicSource.volume -= Time.deltaTime * 1.2f;
            yield return 0;
        }
        musicSource.volume = 0;
    }

    public void QuitToDesktop()
    {
        GameController.Instance.QuitToDesktop();
    }

}
[System.Serializable]
public enum SavePanelMode
{
    Load,
    NewGame
}