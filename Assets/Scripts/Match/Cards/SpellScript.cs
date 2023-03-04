using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class SpellScript : CardScript
{
    public Spell spell;

    public override CardScript SetCard(CardInstance _card, int _owner)
    {
        Card card = CardDictionary.GetCard(_card.cardID);
        switch (card.type)
        {
            case CardType.Creature:
                CreatureScript creature = gameObject.AddComponent<CreatureScript>();
                creature.SetCreature(_card, _owner, visuals);
                Destroy(this);
                return creature;
            case CardType.Spell:
                return SetSpell(_card, _owner, visuals);
        }
        return null;
    }
    public SpellScript SetSpell(CardInstance _spell, int _owner, List<CardVisuals> _visuals)
    {

        //component references
        visuals = _visuals;
        targetLines = GetComponent<CardTargets>();

        //card class references
        cardInstance = _spell;
        cardClass = CardDictionary.GetCard(_spell.cardID);
        spell = (Spell)cardClass;

        //gameplay
        owner = _owner;
        gameObject.name = spell.cardName;

        //generic variables
        cardName = cardClass.cardName;
        cardType = CardType.Spell;
        cardCost = spell.cost;

        //sets up habilities and hability string
        habilities = new List<Hability>();
        cardHability = "";
        foreach (Hability hab in cardClass.habilities)
        {
            cardHability += hab.description + "\r\n";
            habilities.Add(hab);
        }

        //setup visuals
        foreach (CardVisuals visual in visuals)
        {
            visual.SetCard(this);
        }
        return this;
    }
}
