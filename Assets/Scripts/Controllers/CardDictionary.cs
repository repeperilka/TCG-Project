using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class CardDictionary
{
    public static Dictionary<string, Card> allCards;
    public static int lastInstanceID;
    public static List<int> disposedIDs;
    public static Color[] rarityColors = new Color[] {
        new Color(0, 0, 0),
        new Color(.8f, .8f, .8f),
        new Color(1f, 0.74f, 0),
        new Color(.23f, 0, .82f)
    };

    public static void Initialize()
    {
        disposedIDs = new List<int>();
        allCards = new Dictionary<string, Card>();

        List<Creature> toInsert = CreatureInsertor.Instance.creatures;
        for(int i = 0; i < toInsert.Count; i++)
        {
            AddCard(toInsert[i]);
        }
        List<Spell> toInserdt = CreatureInsertor.Instance.spells;
        for (int i = 0; i < toInserdt.Count; i++)
        {
            AddCard(toInserdt[i]);
        }
    }

    public static Card GetCard(string _cardID)
    {
        Card returnCard = allCards[_cardID];
        if (returnCard == null)
            Debug.Log("Card Not Found: " + _cardID);
        return allCards[_cardID];
    }
    public static Effect GetEffect(string _typeAndParameters)
    {
        string[] splitType = _typeAndParameters.Split(' ');
        Type t = null;
        try
        {
            t = Type.GetType(splitType[0]);
        }
        catch
        {
            Debug.Log("GetEffect: TypeName is not good (" + splitType[0] + ")");
            return null;
        }
        Effect newEffect = null;
        if(splitType.Length > 1)
        {
            try
            {
                newEffect = (Effect)Activator.CreateInstance(t, int.Parse(splitType[1]));
            }
            catch
            {
                Debug.Log("GetEffect: Value couldn't be parsed (" + splitType[1] + ")");
                return null;
            }
        }
        else
        {
            newEffect = (Effect)Activator.CreateInstance(t);
        }
        return newEffect;
    }
    public static int GetCardID()
    {
        if(disposedIDs.Count != 0)
        {
            int reusedId = disposedIDs[0];
            disposedIDs.RemoveAt(0);
            return reusedId;
        }
        else
        {
            lastInstanceID++;
            return lastInstanceID;
        }
    }
    public static void DisposeID(int _idToDispose)
    {
        disposedIDs.Add(_idToDispose);
    }
    static void AddCard(Card _card)
    {
        allCards.Add(_card.cardID, _card);
    }
    public static int GetMorphIndex(float _roll, float _morphCount)
    {
        if (_morphCount == 0 || _morphCount == 1)
            return 0;
        float morph_1 = _morphCount - 1f;
        int index = Mathf.RoundToInt(Mathf.Pow(_roll * morph_1, 2f) * (1f / morph_1));
        return index;
    }
    public static Color GetRarityColor(CardRarity _rarity)
    {
        return rarityColors[(int)_rarity];
    }
}
