using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public string cardName;
    public string cardID;
    public int cost;
    public int price;
    public CardRarity rarity;
    public CardType type;
    public List<Hability> habilities;
    public Sprite sprite;

    public Card(string _name, string _cardID, int _cost, int _price, CardRarity _rarity, CardType _type, Hability[] _habilities, Sprite _sprite)
    {
        cardName = _name;
        cardID = _cardID;
        cost = _cost;
        rarity = _rarity;
        type = _type;
        habilities = new List<Hability>(_habilities);
        sprite = _sprite;
    }
}
[System.Serializable]
public class Creature : Card
{
    public List<Hability> possibleMorphs;
    public int attack;
    public int defense;

    public Creature(string _name, string _cardID, int _cost, int _price, CardRarity _rarity, Hability[] _mainHabilities, Sprite _sprite, Hability[] _possibleHabilities, int _attk, int _def) : base(_name, _cardID, _cost, _price, _rarity, CardType.Creature, _mainHabilities, _sprite)
    {
        possibleMorphs = new List<Hability>(_possibleHabilities);
        attack = _attk;
        defense = _def;
    }
}
[System.Serializable]
public class Spell : Card
{
    public Spell(string _name, string _cardID, int _cost, int _price, CardRarity _rarity, Hability _hability, Sprite _sprite) : base(_name, _cardID, _cost, _price, _rarity, CardType.Spell, new Hability[] { _hability }, _sprite)
    {

    }
}


[System.Serializable]
public class CardInstance
{
    public string cardID;
    public int instanceID;
    public float value;

    public CardInstance()
    {
        cardID = "";
        instanceID = 0;
        value = 0;
    }
    public CardInstance(string _id)
    {
        cardID = _id;
        instanceID = 0;
        value = Random.Range(0, 1f);
    }
    public CardInstance(string _id, int _instanceID)
    {
        cardID = _id;
        instanceID = _instanceID;
        value = Random.Range(0, 1f);
    }
    public CardInstance(string _id, int _instanceID, float _value)
    {
        cardID = _id;
        instanceID = _instanceID;
        value = _value;
    }

}
[System.Serializable]
public enum CardType //order dependant (for card visualization)
{
    Creature,
    Spell,
    Hability
}
[System.Serializable]
public enum CardRarity
{
    Common,
    Unfrecuent,
    Rare,
    Mythic
}
