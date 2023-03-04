using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;


public static class SaveGame
{
    public static GameSave currentSave;

    public static int saveIndex;

    public static GameSave[] savesData;

    public static void Initialize()
    {
        if(!Directory.Exists(Application.persistentDataPath + "/Saves"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Saves");
        }
        Debug.Log(Application.persistentDataPath);
        currentSave = new GameSave();
        UpdateSaveData();
    }
    public static string[] GetSaves()
    {
        return Directory.GetDirectories(Application.persistentDataPath + "/Saves/");
    }
    public static void UpdateSaveData()
    {
        savesData = new GameSave[4];
        string[] saves = Directory.GetFiles(Application.persistentDataPath + "/Saves/");
        for (int i = 0; i < saves.Length; i++)
        {
            if(saves[i].Substring(saves[i].Length - 4, 4) != ".sav")
            {
                Debug.Log("This is not a save file");
                continue;
            }

            string[] saveSplit = saves[i].Split('/');
            int currentSave = 0;
            try
            {
                currentSave = int.Parse(saveSplit[saveSplit.Length - 1].Replace(".sav", ""));
            }
            catch
            {
                Debug.Log("Wrong name save");
                continue;
            }
            savesData[currentSave] = JsonConvert.DeserializeObject<GameSave>(File.ReadAllText(Application.persistentDataPath + "/Saves/" + currentSave + ".sav"));
        }
        for (int i = 0; i < savesData.Length; i++)
        {
            if (savesData[i] == null)
            {
                savesData[i] = new GameSave();
                Debug.Log("[" + i + "].saveName = Empty");
            }
            else
            {
                Debug.Log("[" + i + "].saveName = " + savesData[i].saveName);
            }
        }

        int lastPlayed = PlayerPrefs.GetInt("lastPlayed");
        if (lastPlayed != -1)
        {
            if (savesData[lastPlayed].saveName == "")
            {
                PlayerPrefs.SetInt("lastPlayed", -1);
            }
        }
    }


    public static void DeleteGame(int _saveIndex)
    {
        File.Delete(Application.persistentDataPath + "/Saves/" + _saveIndex + ".sav");
        UpdateSaveData();
    }
    public static void NewGame(int _saveIndex, string _saveName)
    {
        currentSave = new GameSave();
        currentSave.newTerrain = true;
        currentSave.saveName = _saveName;
        currentSave.playerHP = new int[] { 20, 20 };
        currentSave.timePlayed = 0;
        currentSave.gold = 100;
        currentSave.worldSeed = Random.Range(0, 100000);
        currentSave.playerDeck = new List<CardInstance>();
        currentSave.playerInventory = new List<CardInstance>();
        currentSave.playerPosition = new Vector2Int(50, 50);

        AddCardToDeck(new CardInstance("dog"));
        AddCardToDeck(new CardInstance("dog"));
        AddCardToDeck(new CardInstance("dog"));
        AddCardToDeck(new CardInstance("dog"));
        AddCardToDeck(new CardInstance("cat"));
        AddCardToDeck(new CardInstance("cat"));
        AddCardToDeck(new CardInstance("cat"));
        AddCardToDeck(new CardInstance("cat"));
        AddCardToDeck(new CardInstance("battle_bear"));
        AddCardToDeck(new CardInstance("battle_bear"));
        AddCardToDeck(new CardInstance("battle_bear"));
        AddCardToDeck(new CardInstance("dragon"));
        AddCardToDeck(new CardInstance("knoledge"));
        AddCardToDeck(new CardInstance("knoledge"));
        AddCardToDeck(new CardInstance("knoledge"));
        AddCardToDeck(new CardInstance("fireball"));
        AddCardToDeck(new CardInstance("fireball"));
        AddCardToDeck(new CardInstance("fireball"));
        AddCardToDeck(new CardInstance("fireball"));
        AddCardToDeck(new CardInstance("trap"));
        AddCardToDeck(new CardInstance("trap"));
        AddCardToDeck(new CardInstance("trap"));
        AddCardToDeck(new CardInstance("trap"));


        saveIndex = _saveIndex;
        Save(_saveIndex);
        PlayerPrefs.SetInt("lastLoaded", _saveIndex);
        GameController.Instance.sceneController.LoadScene(1);
    }
    public static void Save(int _saveIndex)
    {
        string jsonData = JsonConvert.SerializeObject(currentSave);
        File.WriteAllText(Application.persistentDataPath + "/Saves/" + _saveIndex + ".sav", jsonData);
        UpdateSaveData();
    }
    public static void Save()
    {
        Save(saveIndex);
    }
    public static bool Load(int _saveIndex, bool _sceneChange)
    {
        saveIndex = _saveIndex;

        currentSave = JsonConvert.DeserializeObject<GameSave>(File.ReadAllText(Application.persistentDataPath + "/Saves/" + _saveIndex + ".sav"));

        PlayerPrefs.SetInt("lastLoaded", _saveIndex);
        if(_sceneChange)
            GameController.Instance.sceneController.LoadScene(1);
        return true;
    }

    public static void AddCardToInventory(CardInstance _card)
    {
        currentSave.playerInventory.Add(_card);
        SortInventory();
    }
    public static void RemoveCardFromInventory(CardInstance _card)
    {
        currentSave.playerInventory.Remove(_card);
        SortInventory();
    }
    public static void SortInventory()
    {
        currentSave.playerInventory.Sort(SortByID);
    }

    public static void AddCardToDeck(CardInstance _card)
    {
        currentSave.playerDeck.Add(_card);
        SortDeck();
    }
    public static void RemoveCardFromDeck(CardInstance _card)
    {
        currentSave.playerDeck.Remove(_card);
        SortDeck();
    }
    public static void SortDeck()
    {
        currentSave.playerDeck.Sort(SortByID);
    }


    static int SortByID(CardInstance p1, CardInstance p2)
    {
        return p1.cardID.CompareTo(p2.cardID);
    }
}
[System.Serializable]
public class GameSave
{
    public string saveName;
    public int saveIndex;
    public string timeConverted;
    float floatTimePlayed;
    public float timePlayed { get { return floatTimePlayed; } set { floatTimePlayed = value; timeConverted = GetTimeString(floatTimePlayed); } }

    public bool newTerrain;
    public bool tutorial;

    public int[] playerHP;
    public int worldSeed;
    public int gold;
    public float worldTime;
    public List<CardInstance> playerDeck;
    public List<CardInstance> playerInventory;
    public Vector2Int playerPosition;
    public List<MapTown> towns;

    public GameSave()
    {
        saveName = "";
        saveIndex = 0;
        timePlayed = 0;
        timeConverted = "";
        playerHP = new int[2];
        worldSeed = 0;
        gold = 0;
        worldTime = 0;
        newTerrain = true;
        tutorial = true;
        playerDeck = new List<CardInstance>();
        playerInventory = new List<CardInstance>();
        playerPosition = Vector2Int.one;
        towns = new List<MapTown>();
    }
    public GameSave(string _saveName, int _saveIndex, int _timePlayed, int[] _playerHP, int _worldSeed, int _gold, float _worldTime, bool _newTerrain, bool _tutorial, List<CardInstance> _playerDeck, List<CardInstance> _playerInventory,
        Vector2Int _playerPosition, List<MapTown> _towns)
    {
        saveName = _saveName;
        saveIndex = _saveIndex;
        timePlayed = _timePlayed;
        playerHP = _playerHP;
        worldSeed = _worldSeed;
        gold = _gold;
        worldTime = _worldTime;
        playerDeck = _playerDeck;
        newTerrain = _newTerrain;
        tutorial = _tutorial;
        playerInventory = _playerInventory;
        playerPosition = _playerPosition;
        towns = _towns;
    }
    public static string GetTimeString(float _seconds)
    {
        int seconds = Mathf.RoundToInt(_seconds);
        int minutes = 0;
        int hours = 0;
        while (seconds > 60)
        {
            seconds -= 60;
            minutes++;
        }
        while (minutes > 60)
        {
            minutes -= 60;
            hours++;
        }
        string returnString = hours.ToString() + ":";
        if (minutes.ToString().Length < 2)
        {
            returnString += "0" + minutes + ":";
        }
        else
        {
            returnString += minutes.ToString();
        }
        if (seconds.ToString().Length < 2)
        {
            returnString += "0" + seconds;
        }
        else
        {
            returnString += seconds;
        }
        return returnString;
    }
}
