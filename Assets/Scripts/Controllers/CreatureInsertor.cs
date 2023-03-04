using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureInsertor : MonoBehaviour
{
    public List<Creature> creatures;
    public List<Spell> spells;


    #region Singleton
    public static CreatureInsertor Instance { get; private set; }
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
}
