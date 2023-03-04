using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTileScript : MonoBehaviour
{
    public Vector2Int index;
    public SpriteRenderer rend;
    public BiomeType biome;
    public PolygonCollider2D thisCollider;

    private void Start()
    {
        thisCollider = GetComponent<PolygonCollider2D>();
        thisCollider.enabled = false;
    }

    public void SetTile(Vector2Int _index, int _sortingOrder)
    {
        rend.sortingOrder = _sortingOrder;
        index = _index;
        gameObject.name = _index.x + ":" + _index.y;
    }
    public void SetSprite(TileBiomeData _data)
    {
        if(_data.biomeType != BiomeType.Ocean)
        {
            thisCollider.enabled = true;
        }
        rend.sprite = _data.sprite;
        biome = _data.biomeType;
    }
}
