using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    const float tileWidth = 0.94f;
    const float tileHeigt = 0.52f;
    const float tileSize = .39f;

    public List<Biome> biomes;
    public Sprite[] townSprites;

    int[,] voronoi;
    const float voronoiPerlinSize = 0.04f;
    const float voronoiPerlinCut = .4f;
    const float voronoiEdgeCut = 0.6f;
    public bool updateVoronoi;


    public TileMap map;
    const float horizontalOffset = -0.44f;
    public int mapSize = 99;

    [Header("Perlin")]
    const float perlinSize = 0.26f;
    public int seed;

    private void Start()
    {

    }

    private void Update()
    {
        if (updateVoronoi)
        {
            if(seed != 0)
            {
                StartCoroutine(GenerateTerrain(seed));
            }
            else
            {
                GenerateTerrain();
            }
            updateVoronoi = false;
        }
    }


    public TileBiomeData GetHeight(int _x, int _y)
    {
        if (voronoi[_x, _y] == 0)
            return new TileBiomeData(BiomeType.Ocean, null);

        float maxBiome = 0;
        int currentBiome = 0;
        for(int i = 0; i < biomes.Count; i++)
        {
            float perlinBiome = Mathf.PerlinNoise((biomes[i].apperanceOffset.x + _x) * biomes[i].apperanceSize, (biomes[i].apperanceOffset.y + _y) * biomes[i].apperanceSize);
            if(perlinBiome > maxBiome)
            {
                maxBiome = perlinBiome;
                currentBiome = i;
            }
        }

        float fHeigth = Mathf.Pow(Mathf.PerlinNoise(_x * perlinSize, _y * perlinSize), 2f);
        int heigth = biomes[currentBiome].spriteCut.Length - 1;

        for(int i = 0; i < biomes[currentBiome].spriteCut.Length; i++)
        {
            if(fHeigth < biomes[currentBiome].spriteCut[i])
            {
                heigth = i;
                break;
            }
        }
        switch (heigth)
        {
            case 0:
                return new TileBiomeData(BiomeType.Ocean, null);
            case 1:
                return new TileBiomeData((BiomeType)(((int)biomes[currentBiome].biomeType) + (heigth - 1)), biomes[currentBiome].plains[Random.Range(0, biomes[currentBiome].plains.Length)]);
            case 2:
                return new TileBiomeData((BiomeType)(((int)biomes[currentBiome].biomeType) + (heigth - 1)), biomes[currentBiome].forest[Random.Range(0, biomes[currentBiome].forest.Length)]);
            case 3:
                return new TileBiomeData((BiomeType)(((int)biomes[currentBiome].biomeType) + (heigth - 1)), biomes[currentBiome].hill[Random.Range(0, biomes[currentBiome].hill.Length)]);
            case 4:
                return new TileBiomeData((BiomeType)(((int)biomes[currentBiome].biomeType) + (heigth - 1)), biomes[currentBiome].mountain[Random.Range(0, biomes[currentBiome].mountain.Length)]);
            default:
                return new TileBiomeData((BiomeType)(((int)biomes[currentBiome].biomeType) + 3), biomes[currentBiome].mountain[Random.Range(0, biomes[currentBiome].mountain.Length)]);
        }

    }




    //generate random terrain
    public IEnumerator GenerateTerrain()
    {
        if (SaveGame.currentSave.newTerrain)
        {
            SaveGame.currentSave.worldSeed = Random.Range(0, 1000000);
        }
        yield return StartCoroutine(GenerateTerrain(SaveGame.currentSave.worldSeed));
    }
    //generate seeded terrain
    IEnumerator GenerateTerrain(int _seed)
    {
        seed = _seed;
        Random.InitState(_seed);
        yield return StartCoroutine(CreateVoronoi());
        yield return StartCoroutine(UpdateMap());

        if (SaveGame.currentSave.newTerrain)
        {
            GenerateTowns(50);
            SaveGame.currentSave.newTerrain = false;
            SaveGame.Save();
            Debug.Log("New Game Terrain");
        }
        else
        {
            Debug.Log("Load Game Terrain");
            UpdateTowns();
        }

    }

    //creates voronoi noise for outlines of the island (needed before creating the biome map)
    public IEnumerator CreateVoronoi()
    {
        voronoi = new int[mapSize, mapSize];
        List<Vector3Int> points = new List<Vector3Int>();

        //offset for the perlin noise
        Vector2Int perlinOffset = new Vector2Int(Random.Range(0, 1000), Random.Range(0, 1000));

        //tile based distribution of the points in the voronoi (this is how much they can derive from the x, y interger coordinates: mapSize(99) * voronoiSampleRatio(0,08))
        int diference = 8;

        //distributing semi-random points for voronoi
        for (int x = 0; x < mapSize; x += diference)
        {
            for (int y = 0; y < mapSize; y += diference)
            {
                int nx = Random.Range(x, x + diference);
                int ny = Random.Range(y, y + diference);
                //gets the value of the point (either 0 or 1) based on a combination of outlineDistance to the centre texture and a perlin noise
                points.Add(new Vector3Int(nx, ny, GetVoronoiPointValue(nx, ny, perlinOffset)));
            }
            yield return 0;
        }
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                float dist = 1000f;
                int value = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    float thisDist = Vector2.Distance(new Vector2(x, y), new Vector2(points[i].x, points[i].y));
                    if (thisDist < dist)
                    {
                        dist = thisDist;
                        value = points[i].z;
                    }
                }
                voronoi[x, y] = value;
            }
            yield return 0;
        }
    }
    //returns the value of a voronoi point based on an outlineDistance texture and a perlin noise with the given offset
    public int GetVoronoiPointValue(int _x, int _y, Vector2Int _perlinOffset)
    {
        //dividing the distance between [_x, _y] and the middle of the voronoi by the longest distance to normalize it (longest distance being from [0, 0] to the centre)
        float dist = Vector2.Distance(Vector2.one * 49.5f, new Vector2(_x, _y)) / 70f;

        if (dist > voronoiEdgeCut)
            return 0;

        //perlin for emptiness in the middle
        float perlin = Mathf.PerlinNoise((_perlinOffset.x + _x) * voronoiPerlinSize, (_perlinOffset.y + _y) * voronoiPerlinSize);
        if(perlin < voronoiPerlinCut)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }
    //updates already spawned tiles
    public IEnumerator UpdateMap()
    {
        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                map[x, y].SetSprite(GetHeight(x, y));
            }
            yield return 0;
        }
    }

    //generates towns
    public void GenerateTowns(int _townAmount)
    {
        SaveGame.currentSave.towns = new List<MapTown>();
        int spawnedTowns = 0;
        while(spawnedTowns < _townAmount)
        {
            Vector2Int index = new Vector2Int(Random.Range(1, mapSize - 2), Random.Range(1, mapSize - 2));
            if(map[index.x, index.y].biome != BiomeType.Ocean && map[index.x, index.y].biome != BiomeType.Town)
            {
                MapTileScript tile = map[index.x, index.y];
                SubBiomeType biomePools = SubBiomeType.Tundra;
                Sprite townSprite = null;
                string townName = "";
                if (tile.biome.ToString().Contains("Tundra"))
                {
                    townName = "Tundra";
                    townSprite = townSprites[0];
                    biomePools = SubBiomeType.Tundra;
                }
                else if (tile.biome.ToString().Contains("Forest"))
                {
                    townName = "Forest";
                    townSprite = townSprites[1];
                    biomePools = SubBiomeType.Forest;
                }
                else if (tile.biome.ToString().Contains("Desert"))
                {
                    townName = "Desert";
                    townSprite = townSprites[2];
                    biomePools = SubBiomeType.Desert;
                }

                tile.SetSprite(new TileBiomeData(BiomeType.Town, townSprite));
                SaveGame.currentSave.towns.Add(new MapTown(townName + "_Town", tile.index, biomePools));
                spawnedTowns++;
            }
        }
        SaveGame.Save();
        UpdateTowns();
    }
    //takes towns from saveGame
    public void UpdateTowns()
    {
        for(int i = 0; i < SaveGame.currentSave.towns.Count; i++)
        {
            MapTileScript townTile = map[SaveGame.currentSave.towns[i].index.x, SaveGame.currentSave.towns[i].index.y];
            Sprite townSprite = null;
            switch (SaveGame.currentSave.towns[i].storesBiomePool)
            {
                case SubBiomeType.Tundra:
                    townSprite = townSprites[0];
                    break;
                case SubBiomeType.Forest:
                    townSprite = townSprites[1];
                    break;
                case SubBiomeType.Desert:
                    townSprite = townSprites[2];
                    break;
            }
            townTile.SetSprite(new TileBiomeData(BiomeType.Town, townSprite));
        }
    }


    //gets town on index
    public int GetTown(Vector2Int _index)
    {
        foreach(MapTown town in SaveGame.currentSave.towns)
        {
            if(town.index.x == _index.x && town.index.y == _index.y)
            {
                return SaveGame.currentSave.towns.IndexOf(town);
            }
        }
        return -1;
    }
}
[System.Serializable]
public class Biome
{
    public string biomeName;
    public string biomeID;
    public BiomeType biomeType;
    public SubBiomeType mainBiome;
    public float perlinSize;
    public Sprite[] plains;
    public Sprite[] forest;
    public Sprite[] hill;
    public Sprite[] mountain;
    public float[] spriteCut;
    public Vector2 apperanceOffset;
    public float apperanceSize;
}
public struct TileBiomeData
{
    public BiomeType biomeType;
    public Sprite sprite;

    public TileBiomeData(BiomeType _type, Sprite _sprite)
    {
        biomeType = _type;
        sprite = _sprite;
    }
}

[System.Serializable]
public enum BiomeType
{
    Ocean,

    TundraPlains,
    TundraTress,
    TundraHill,
    TundraMountain,

    ForestPlains,
    ForestTrees,
    ForestHill,
    ForestMountain,

    DesertPlains,
    DesertTrees,
    DesertHill,
    DesertMountain,

    Town
}
[System.Serializable]
public enum SubBiomeType
{
    Tundra,
    Forest,
    Desert,
}
[System.Serializable]
public class MapTown
{
    public string townName;
    public Vector2Int index;
    public SubBiomeType storesBiomePool;

    //instance id in cardinstance means if the card has been bought: 0 = avaliable, 1 = bought
    public CardInstance[] creatureShop;
    public CardInstance[] spellShop;

    public MapTown()
    {
        townName = "";
        index = new Vector2Int(0, 0);
        storesBiomePool = SubBiomeType.Tundra;
        creatureShop = new CardInstance[6];
        spellShop = new CardInstance[6];
    }

    public MapTown(string _townName, Vector2Int _index, SubBiomeType _biomePool)
    {
        townName = _townName;
        index = _index;
        storesBiomePool = _biomePool;
        creatureShop = new CardInstance[6];
        spellShop = new CardInstance[6];
        Restock();
    }
    public MapTown(string _townName, Vector2Int _index, SubBiomeType _biomePool, CardInstance[] _creatureShop, CardInstance[] _spellShop, CardInstance[] _itemsShop)
    {
        townName = _townName;
        index = _index;
        storesBiomePool = _biomePool;
        creatureShop = _creatureShop;
        spellShop = _spellShop;
    }
    public void Restock()
    {
        for (int i = 0; i < creatureShop.Length; i++)
        {
            string creatureID = "";

            int iterations = 0;
            while (creatureID == "" && iterations < 100)
            {
                creatureID = OverworldController.Instance.GetCreatureFromPool(storesBiomePool);
                iterations++;
            }

            Creature creature = (Creature)CardDictionary.GetCard(creatureID);
            creatureShop[i] = new CardInstance(creatureID, 0);
        }
        for(int i = 0; i < spellShop.Length; i++)
        {
            string spellID = "";

            int iterations = 0;
            while (spellID == "" && iterations < 100)
            {
                spellID = OverworldController.Instance.GetSpellFromPool(storesBiomePool);
                iterations++;
            }

            spellShop[i] = new CardInstance(spellID, 0);
        }
    }
}
[System.Serializable]
public class TileMap
{
    public MapTileScript[] tiles;
    public int lengthX;
    public int lengthY;

    public TileMap(int _sizeX, int _sizeY)
    {
        lengthX = _sizeX;
        lengthY = _sizeY;
        tiles = new MapTileScript[_sizeX * _sizeY];
    }
    public MapTileScript this[int _x, int _y]
    {
        get => tiles[_x * lengthY + _y];
        set => tiles[_x * lengthY + _y] = value;
    }
}