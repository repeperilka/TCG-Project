using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OverworldController : MonoBehaviour
{
    public MapGenerator map;
    public TownController town;
    public GameObject townUI;

    public OverworldPlayer player;
    public float speed;
    public LayerMask tileMask;

    public List<CreaturePool> creaturePools;
    public List<CreaturePool> spellPools;

    public int currentTown;

    public Image loadScreen;

    public Camera renderCam;
    public AudioSource musicSource;
    public CameraController playerCamera;
    public ConversationTextScript druidConver;

    public TutorialScreen overworldTutorial;
    public TutorialScreen deckBuildingTutorial;

    [HideInInspector]
    public bool active = false;

    #region Singleton
    public static OverworldController Instance { get; private set; }
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
    }
    #endregion

    private void Start()
    {
        town.gameObject.SetActive(false);
    }


    private void Update()
    {
        if (!active)
            return;

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, tileMask);
            if(hit.collider != null)
            {
                player.path = GetPath(hit.collider.GetComponent<MapTileScript>());
            }
        }
        if (Input.GetKey(KeyCode.Space))
        {
            playerCamera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10f);
        }
    }

    public IEnumerator FadeInMusic()
    {
        while(musicSource.volume < 1f)
        {
            musicSource.volume += Time.deltaTime * 0.5f;
            yield return 0;
        }
        musicSource.volume = 1f;
    }
    public IEnumerator FadeOutMusic()
    {
        while (musicSource.volume > 0)
        {
            musicSource.volume -= Time.deltaTime * 1.2f;
            yield return 0;
        }
        musicSource.volume = 0f;
    }
    public IEnumerator StartScene()
    {
        loadScreen.color = Color.black;
        StartCoroutine(FadeInMusic());
        yield return StartCoroutine(map.GenerateTerrain());
        SetPlayerPosition(SaveGame.currentSave.playerPosition);
        loadScreen.CrossFadeAlpha(0, 1f, false);
        yield return new WaitForSeconds(1f);
        loadScreen.raycastTarget = false;
        if (!SaveGame.currentSave.tutorials[0])
        {
            druidConver.gameObject.SetActive(true);
            yield return StartCoroutine(Tutorial());
        }
        if (!SaveGame.currentSave.tutorials[1])
        {
            overworldTutorial.gameObject.SetActive(true);
            while (overworldTutorial.gameObject.activeInHierarchy)
            {
                yield return 0;
            }
        }
        active = true;
    }

    public IEnumerator Tutorial()
    {
        yield return StartCoroutine(druidConver.ConversationRoutine(new string[] { 
        "Oh, hola amigo!",
        "Bienvenido al mundo de los vivos",
        "Tranquilízate, sé que hay mucho que procesar sobre todo esto",
        "Por eso, permiteme que te explique",
        "Tú, amigo mío, eres un Centinela, al igual que yo",
        "Fuimos creados por nuestra diosa Gaïa, con el propósito de mantener el equilibrio en estas tierras",
        "...",
        "Ahora que lo pienso",
        "Gaïa no ha necesitado dar a luz a ningún otro centinela en siglos",
        "...",
        "Oh oh, quizas tengamos un problema serio entre manos",
        "Cierto es que he estado escuchando rumores que venían de tierras lejanas",
        "Rumores que hablaban de cultivos muriendo sin razón aparente y animales migrando antes de lo esperado",
        "Hmm, de ser así, no tenemos tiempo que perder",
        }));
        MapTileScript verminTile = map.map[map.verminTile.x, map.verminTile.y];
        while(Vector2.Distance(verminTile.transform.position, playerCamera.transform.position) > .1f)
        {
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, new Vector3(verminTile.transform.position.x, verminTile.transform.position.y, -10f), 
                Time.deltaTime * 10f);
            yield return 0;
        }
        yield return StartCoroutine(druidConver.ConversationRoutine(new string[] {
        "Este es el lugar",
        "No sé qué es lo que encontrarás ahí, pero estoy seguro que es la razón por la que Gaïa te dio vida",
        }));
        while (Vector2.Distance(player.transform.position, playerCamera.transform.position) > .1f)
        {
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, new Vector3(player.transform.position.x, player.transform.position.y, -10f),
                Time.deltaTime * 10f);
            yield return 0;
        }

        yield return StartCoroutine(druidConver.ConversationRoutine(new string[] {
        "Vas a necesitar la ayuda de algunos espíritus",
        "Ellos te proporcionarán la fuerza que necesitas para recorrer los distintos biomas de esta nuestra isla",
        "Para ello, solo tienes que conjurar el hechizo \"Capturar Espíritu\" sobre cualquier criatura de las manadas salvajes que encuentres",
        "Después de capturarlos, puedes ir al pueblo más cercano y agregarlo a tu arsenal de espíritus",
        "Eso es todo lo que puedo decirte por ahora",
        "Embarca en tu viaje, permanece siempre cerca de un pueblo, y ten mucho cuidado"
        }));
        SaveGame.currentSave.tutorials[0] = true;
        SaveGame.Save();
    }
    public NavPath GetPath(MapTileScript _destination)
    {
        List<MapTileScript> pathTiles = new List<MapTileScript>();
        pathTiles.Add(_destination);

        int iterations = 0;
        MapTileScript currentTile = _destination;
        while(Vector2.Distance(currentTile.transform.position, player.transform.position) > .5f && iterations < 1000)
        {
            MapTileScript closest = null;
            float distance = 1000f;
            if(currentTile.index.y % 2 == 1)
            {
                MapTileScript newTile = map.map[currentTile.index.x - 1, currentTile.index.y];
                float newDist = Vector2.Distance(newTile.transform.position, player.transform.position);
                if (newDist < distance)
                {
                    distance = newDist;
                    closest = newTile;
                }
                newTile = map.map[currentTile.index.x, currentTile.index.y + 1];
                newDist = Vector2.Distance(newTile.transform.position, player.transform.position);
                if (newDist < distance)
                {
                    distance = newDist;
                    closest = newTile;
                }
                newTile = map.map[currentTile.index.x, currentTile.index.y - 1];
                newDist = Vector2.Distance(newTile.transform.position, player.transform.position);
                if (newDist < distance)
                {
                    distance = newDist;
                    closest = newTile;
                }

                newTile = map.map[currentTile.index.x + 1, currentTile.index.y + 1];
                newDist = Vector2.Distance(newTile.transform.position, player.transform.position);
                if (newDist < distance)
                {
                    distance = newDist;
                    closest = newTile;
                }
                newTile = map.map[currentTile.index.x + 1, currentTile.index.y];
                newDist = Vector2.Distance(newTile.transform.position, player.transform.position);
                if (newDist < distance)
                {
                    distance = newDist;
                    closest = newTile;
                }
                newTile = map.map[currentTile.index.x + 1, currentTile.index.y - 1];
                newDist = Vector2.Distance(newTile.transform.position, player.transform.position);
                if (newDist < distance)
                {
                    distance = newDist;
                    closest = newTile;
                }
            }
            else
            {
                MapTileScript newTile = map.map[currentTile.index.x - 1, currentTile.index.y];
                float newDist = Vector2.Distance(newTile.transform.position, player.transform.position);
                if (newDist < distance)
                {
                    distance = newDist;
                    closest = newTile;
                }
                newTile = map.map[currentTile.index.x - 1, currentTile.index.y];
                newDist = Vector2.Distance(newTile.transform.position, player.transform.position);
                if (newDist < distance)
                {
                    distance = newDist;
                    closest = newTile;
                }
                newTile = map.map[currentTile.index.x - 1, currentTile.index.y - 1];
                newDist = Vector2.Distance(newTile.transform.position, player.transform.position);
                if (newDist < distance)
                {
                    distance = newDist;
                    closest = newTile;
                }

                newTile = map.map[currentTile.index.x, currentTile.index.y + 1];
                newDist = Vector2.Distance(newTile.transform.position, player.transform.position);
                if (newDist < distance)
                {
                    distance = newDist;
                    closest = newTile;
                }
                newTile = map.map[currentTile.index.x + 1, currentTile.index.y];
                newDist = Vector2.Distance(newTile.transform.position, player.transform.position);
                if (newDist < distance)
                {
                    distance = newDist;
                    closest = newTile;
                }
                newTile = map.map[currentTile.index.x, currentTile.index.y - 1];
                newDist = Vector2.Distance(newTile.transform.position, player.transform.position);
                if (newDist < distance)
                {
                    distance = newDist;
                    closest = newTile;
                }
            }
            pathTiles.Insert(0, closest);
            currentTile = closest;
            iterations++;
        }
        return new NavPath(pathTiles.ToArray());
    }

    public void SetPlayerPosition(Vector2Int _position)
    {
        Debug.Log(_position);
        Vector2 tilePosition = map.map[_position.x, _position.y].transform.position;
        player.transform.position = tilePosition;
        Camera.main.transform.position = new Vector3(tilePosition.x, tilePosition.y, Camera.main.transform.position.z);

    }
    public void RandomEncounter(BiomeType _type, int _maxTurn)
    {
        CreaturePool pool = null;
        for(int i = 0; i < creaturePools.Count; i++)
        {
            if(creaturePools[i].biome == _type)
            {
                pool = creaturePools[i];
                break;
            }
        }

        if (pool == null)
            return;

        GameController.Instance.turnCount = _maxTurn;
        GameController.Instance.randomEncounterPool = pool;
        GameController.Instance.bossFight = null;
        StartCoroutine(RandomEncounterAnimation());
    }
    public void BossFight(string _bossID)
    {
        GameController.Instance.bossFight = new CardInstance(_bossID);
        StartCoroutine(RandomEncounterAnimation());
    }
    public IEnumerator RandomEncounterAnimation()
    {
        yield return 0;
        Debug.Log("ScreenShot");
        RenderTexture renderTex = new RenderTexture(Screen.width, Screen.height, 16);
        renderCam.targetTexture = renderTex;
        RenderTexture.active = renderTex;
        renderCam.Render();
        yield return new WaitForEndOfFrame();
        RenderTexture.active = null;
        Texture2D tex = new Texture2D(renderTex.width, renderTex.height);
        tex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        tex.Apply();
        Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, renderTex.width, renderTex.height), Vector2.one * 0.5f);
        GameController.Instance.randomEncounterCapture = newSprite;
        GameController.Instance.sceneController.LoadScene(2);
    }

    public string GetCreatureFromPool(BiomeType _type)
    {
        foreach(CreaturePool pool in creaturePools)
        {
            if(pool.biome == _type)
            {
                return pool.GetCard();
            }
        }
        return "";
    }
    public string GetSpellFromPool(BiomeType _type)
    {
        foreach (CreaturePool pool in spellPools)
        {
            if (pool.biome == _type)
            {
                return pool.GetCard();
            }
        }
        return "";
    }

    public string GetCreatureFromPool(SubBiomeType _type)
    {
        List<CreaturePool> avaliablePools = new List<CreaturePool>();
        foreach (CreaturePool pool in creaturePools)
        {
            if (pool.mainBiome == _type)
            {
                avaliablePools.Add(pool);
            }
        }
        if (avaliablePools.Count == 0)
        {
            return "";
        }
        else
        {
            return avaliablePools[Random.Range(0, avaliablePools.Count - 1)].GetCard();
        }
    }
    public string GetSpellFromPool(SubBiomeType _type)
    {
        List<CreaturePool> avaliablePools = new List<CreaturePool>();
        foreach (CreaturePool pool in spellPools)
        {
            if (pool.mainBiome == _type)
            {
                avaliablePools.Add(pool);
            }
        }
        if (avaliablePools.Count == 0)
        {
            return "";
        }
        else
        {
            return avaliablePools[Random.Range(0, avaliablePools.Count - 1)].GetCard();
        }
    }

    public int GetTown(Vector2Int _index)
    {
        return map.GetTown(_index);
    }
    public void EnterTown(int _town)
    {
        currentTown = _town;
        map.gameObject.SetActive(false);
        town.gameObject.SetActive(true);
        SaveGame.currentSave.playerHP = new int[] { 20, 20 };
        SaveGame.currentSave.lastTown = SaveGame.currentSave.towns[_town].index;
        town.SetTown(_town);
        playerCamera.enabled = false;
    }
    public void ExitTown()
    {
        currentTown = -1;
        map.gameObject.SetActive(true);
        town.gameObject.SetActive(false);
        SaveGame.Save();
        playerCamera.enabled = true;
    }
    public void RestockTowns()
    {
        foreach(MapTown town in SaveGame.currentSave.towns)
        {
            town.Restock();
        }
    }
    public void QuitToDesktop()
    {
        SaveGame.Save();
        Application.Quit();
    }
    public void QuitToMainMenu()
    {
        SaveGame.Save();
        GameController.Instance.LoadMainMenu();
    }
}


