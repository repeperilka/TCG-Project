using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackCardScript : CardScript
{
    public CardScript givenCard;
    public bool isHability;
    public Hability hability;
    public Effect effect;
    public TrapStatus isTrap;

    public StackCardScript SetStackCard(CardScript _givenCard, Hability _hability, bool _isHability, List<CardVisuals> _visuals)
    {
        //component references
        visuals = _visuals;
        targetLines = GetComponent<CardTargets>();
        givenCard = _givenCard;

        //card class references
        cardInstance = _givenCard.cardInstance;
        cardClass = givenCard.cardClass;

        //gameplay
        owner = _givenCard.owner;
        gameObject.name = "Stack:" + cardClass.cardName;
        isHability = _isHability;
        effect = CardDictionary.GetEffect(_hability.effectID);
        isTrap = TrapStatus.None;

        //generic variables
        cardName = _hability.name;
        cardType = CardType.Hability;

        //sets up habilities and hability string
        hability = _hability;
        cardHability = hability.description;
        habilities = new List<Hability>();

        //setup visuals
        foreach (CardVisuals visual in visuals)
        {
            visual.SetCard(this);
        }
        return this;
    }
}
[System.Serializable]
public enum TrapStatus
{
    None,
    Active,
    Activated
}
