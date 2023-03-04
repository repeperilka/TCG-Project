using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldPlayer : MonoBehaviour
{
    public NavPath path;
    public float speed;
    MapTileScript currentTile;
    public bool removeEncounters;
    public bool walking;
    AudioSource audioSource;
    public Animator anim;
    public SpriteRenderer charSprite;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        if(path.currentPoint != path.points.Count)
        {
            SetWalking(true);
            transform.position = Vector3.MoveTowards(transform.position, path.points[path.currentPoint].transform.position, speed * Time.deltaTime);

            Vector2 direction = path.points[path.currentPoint].transform.position - transform.position;
            charSprite.flipX = direction.x < 0;

            float deadZone = 0.1f;
            anim.SetBool("right", direction.x > deadZone || direction.x < -deadZone);
            anim.SetBool("up", direction.y > deadZone);
            anim.SetBool("down", direction.y < -deadZone);

            if (Vector3.Distance(transform.position, path.points[path.currentPoint].transform.position) < .05f)
            {
                currentTile = path.points[path.currentPoint];
                SaveGame.currentSave.playerPosition = currentTile.index;
                path.currentPoint++;
                DayNightCycle.Instance.AddStep();
                if(Random.value < .1f && (currentTile.biome != BiomeType.Town && currentTile.biome != BiomeType.Ocean) && !removeEncounters)
                {
                    OverworldController.Instance.RandomEncounter(currentTile.biome, 5);
                }
                if(path.currentPoint == path.points.Count && currentTile.biome == BiomeType.Town)
                {
                    int town = OverworldController.Instance.GetTown(currentTile.index);
                    if(town != -1)
                    {
                        OverworldController.Instance.EnterTown(town);
                    }
                }
            }
        }
        else
        {
            SetWalking(false);
        }
    }
    public void SetWalking(bool _value)
    {
        if(walking != _value)
        {
            walking = _value;
            if (_value)
            {
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }
        anim.SetBool("idle", !_value);
    }
}
[System.Serializable]
public class NavPath
{
    public List<MapTileScript> points;
    public int currentPoint;

    public NavPath(MapTileScript[] _points)
    {
        points = new List<MapTileScript>(_points);
        currentPoint = 0;
    }
}
