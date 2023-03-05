using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    GameObject cardUIPrefab;
    GameObject cardSpritePrefab;


    public CreaturePool randomEncounterPool;
    public CardInstance bossFight;
    public int turnCount;

    public SceneController sceneController;
    public Sprite randomEncounterCapture;

    #region Singleton
    public static GameController Instance { get; private set; }
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    private void Start()
    {
        CardDictionary.Initialize();
        SaveGame.Initialize();

        if(SceneManager.GetActiveScene().buildIndex == 2)
        {
            SaveGame.Load(0, false);
            randomEncounterPool = new CreaturePool(new string[] { "cat", "dog", "battle_bear", "dragon" });
            turnCount = 10;
        }
        else if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            SaveGame.Load(0, false);
        }

        sceneController = GetComponent<SceneController>();

        cardUIPrefab = Resources.Load<GameObject>("Prefabs/ui_card");
        cardSpritePrefab = Resources.Load<GameObject>("Prefabs/sprite_card");
    }

    private void Update()
    {
        SaveGame.currentSave.timePlayed += Time.deltaTime;
    }
    public string GetCardFromPool()
    {
        return randomEncounterPool.GetCard();
    }
    public string GetCardFromPool(float[] _ratios)
    {
        return randomEncounterPool.GetCard(_ratios);
    }
    public CardScript InstantiateCard(string _cardID, Vector3 _position, int _owner)
    {
        CardScript newCard = Instantiate(cardSpritePrefab, _position, Quaternion.identity).GetComponent<CardScript>();
        newCard = newCard.SetCard(new CardInstance(_cardID), _owner);
        newCard.SetLayer("Overlay", 100);
        return newCard;
    }
    public CardScript InstantiateCard(CardInstance _card, Vector3 _position, int _owner)
    {
        CardScript newCard = Instantiate(cardSpritePrefab, _position, Quaternion.identity).GetComponent<CardScript>();
        newCard = newCard.SetCard(_card, _owner);
        newCard.SetLayer("Overlay", 100);
        return newCard;
    }
    public StackCardScript InstantiateStackCard(CardScript _card, Vector3 _position, Hability _hability, bool _isHability)
    {
        CardScript card = Instantiate(cardSpritePrefab, _position, Quaternion.identity).GetComponent<CardScript>();
        return card.StackCard(_card, _hability, _isHability);
    }
    public CardScript InstantiateCardUI(CardInstance _card, Transform _canvasParent)
    {
        CardScript card = Instantiate(cardUIPrefab, _canvasParent).GetComponent<CardScript>();
        card = card.SetCard(_card, 0);
        return card;
    }

    public void LoadMainMenu()
    {
        sceneController.LoadScene(0);
    }
    public void Save()
    {
        SaveGame.Save();
    }
    public void QuitToDesktop()
    {
        Application.Quit();
    }
}
[System.Serializable]
public class CreaturePool
{
    public string biomeName;
    public SubBiomeType mainBiome;
    public BiomeType biome;
    public List<string> commonIDs;
    public List<string> uncommonIDs;
    public List<string> rareIDs;
    public List<string> mythicIDs;
    public static readonly float[] baseRatio = { 1f, .5f, .1f, .01f };

    public CreaturePool(Creature[] _creatures)
    {
        commonIDs = new List<string>();
        uncommonIDs = new List<string>();
        rareIDs = new List<string>();
        mythicIDs = new List<string>();
        foreach (Creature creature in _creatures)
        {
            switch (creature.rarity)
            {
                case CardRarity.Common:
                    commonIDs.Add(creature.cardID);
                    break;
                case CardRarity.Unfrecuent:
                    uncommonIDs.Add(creature.cardID);
                    break;
                case CardRarity.Rare:
                    rareIDs.Add(creature.cardID);
                    break;
                case CardRarity.Mythic:
                    mythicIDs.Add(creature.cardID);
                    break;
            }
        }
    }
    public CreaturePool(string[] _creatureIDs)
    {
        commonIDs = new List<string>();
        uncommonIDs = new List<string>();
        rareIDs = new List<string>();
        mythicIDs = new List<string>();

        foreach (string creatureId in _creatureIDs)
        {
            Creature creature = (Creature)CardDictionary.GetCard(creatureId);
            switch (creature.rarity)
            {
                case CardRarity.Common:
                    commonIDs.Add(creature.cardID);
                    break;
                case CardRarity.Unfrecuent:
                    uncommonIDs.Add(creature.cardID);
                    break;
                case CardRarity.Rare:
                    rareIDs.Add(creature.cardID);
                    break;
                case CardRarity.Mythic:
                    mythicIDs.Add(creature.cardID);
                    break;
            }
        }
    }
    public string GetCard()
    {
        return GetCard(baseRatio);
    }
    public string GetCard(float[] _ratios)
    {
        float[] ratios = _ratios;
        if(ratios.Length != 4)
        {
            Debug.Log("Pool creature wrong ratios");
            ratios = baseRatio;
        }

        float random = Random.Range(0, ratios[0] + ratios[1] + ratios[2] + ratios[3]);
        for (int i = 0; i < ratios.Length; i++)
        {
            random -= baseRatio[i];
            if (random <= 0f)
            {
                int currentPool = i;
                while(currentPool != -1)
                {
                    switch (currentPool)
                    {
                        case 0:
                            if (commonIDs.Count == 0)
                            {
                                currentPool--;
                            }
                            else
                            {
                                return commonIDs[Random.Range(0, commonIDs.Count)];
                            }
                            break;
                        case 1:
                            if(uncommonIDs.Count == 0)
                            {
                                currentPool--;
                            }
                            else
                            {
                                return uncommonIDs[Random.Range(0, uncommonIDs.Count)];
                            }
                            break;
                        case 2:
                            if (rareIDs.Count == 0)
                            {
                                currentPool--;
                            }
                            else
                            {
                                return rareIDs[Random.Range(0, rareIDs.Count)];
                            }
                            break;
                        case 3:
                            if (mythicIDs.Count == 0)
                            {
                                currentPool--;
                            }
                            else
                            {
                                return mythicIDs[Random.Range(0, mythicIDs.Count)];
                            }
                            break;
                    }
                }
                Debug.Log("Pools are not sufficiently populated");
                return null;
            }
        }
        Debug.Log("Pool creature not found");
        return null;
    }
}
