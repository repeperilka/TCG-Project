using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

[System.Serializable]
public class CreatureScript : CardScript
{
    [Header("Card Data")]
    public Creature creature;
    public int morphIndex;
    public string morph;
    public int attack;
    public int defense;
    public List<Buff> buffs;

    public override CardScript SetCard(CardInstance _card, int _owner)
    {
        Card card = CardDictionary.GetCard(_card.cardID);
        switch (card.type)
        {
            case CardType.Creature:
                return SetCreature(_card, _owner, visuals);
            case CardType.Spell:
                SpellScript spell = gameObject.AddComponent<SpellScript>();
                spell.SetSpell(_card, _owner, visuals);
                Destroy(this);
                return spell;
        }
        return null;
    }
    public CreatureScript SetCreature(CardInstance _creature, int _owner, List<CardVisuals> _visuals)
    {
        //component references
        visuals = _visuals;
        targetLines = GetComponent<CardTargets>();
        buffs = new List<Buff>();

        //card class references
        cardInstance = _creature;
        cardClass = CardDictionary.GetCard(_creature.cardID);
        creature = (Creature)cardClass;

        //gameplay
        owner = _owner;
        gameObject.name = creature.cardName;

        //generic variables
        cardName = cardClass.cardName;
        cardType = CardType.Creature;
        cardCost = creature.cost;

        //creature based variables
        attack = creature.attack;
        defense = creature.defense;

        //sets up habilities and hability string
        habilities = new List<Hability>();
        cardHability = "";
        foreach(Hability hab in cardClass.habilities)
        {
            cardHability += hab.description + "\r\n";
            habilities.Add(hab);
        }

        //sets up the morph
        if (creature.possibleMorphs.Count != 0)
        {
            int morphIndexPref = CardDictionary.GetMorphIndex(cardInstance.value, creature.possibleMorphs.Count);
            if(morphIndexPref < creature.possibleMorphs.Count)
            {
                morphIndex = morphIndexPref;
                habilities.Add(creature.possibleMorphs[morphIndex]);
                morph = creature.possibleMorphs[morphIndex].description;
            }
            else
            {
                Debug.LogException(new System.Exception("MorphIndex failed: givenIndex = " + morphIndexPref + " : morph count = " + creature.possibleMorphs.Count + " : roll value = " + cardInstance.value));
            }
        }
        else
        {
            morph = "";
        }

        //setup visuals
        foreach (CardVisuals visual in visuals)
        {
            visual.SetCard(this);
        }

        //performs OnSetup triggers
        List<Hability> triggers = GetTriggers("OnSetup");
        foreach(Hability hab in triggers)
        {
            Effect effect = CardDictionary.GetEffect(hab.effectID);
            StartCoroutine(effect.Execute(this, null));
        }
        return this;
    }
    public void AddAttack(int _amount)
    {
        attack += _amount;
        UpdateVisuals();
    }
    public void AddDefense(int _amount)
    {
        defense += _amount;
        UpdateVisuals();
    }
    public bool DealDamage(int _amount)
    {
        defense -= _amount;
        UpdateVisuals();
        if (defense <= 0)
            return true;
        return false;
    }

    public override List<Hability> ActivateTriggers(string _trigger)
    {
        if(_trigger == "OnNextTurn")
        {
            NextTurn();
        }
        for(int i = 0; i < buffs.Count; i++)
        {
            if(buffs[i].removeTrigger == _trigger)
            {
                RemoveBuff(buffs[i]);
                i--;
            }
        }
        return base.ActivateTriggers(_trigger);
    }
    public void AddBuff(Buff _buff)
    {
        AddAttack(_buff.attackAdd);
        AddDefense(_buff.defenseAdd);
        habilities.Add(_buff.habilityAdd);
        buffs.Add(_buff);
    }
    public void RemoveBuff(Buff _buff)
    {
        AddAttack(-_buff.attackAdd);
        AddDefense(-_buff.defenseAdd);
        habilities.Remove(_buff.habilityAdd);
        buffs.Remove(_buff);
    }
    public override void NextTurn()
    {
        for(int i = 0; i < buffs.Count; i++)
        {
            buffs[i].turnRemove -= 1;
            if(buffs[i].turnRemove == 0)
            {
                RemoveBuff(buffs[i]);
                i--;
            }
        }
        base.NextTurn();
    }
}
[System.Serializable]
public class Buff
{
    public int costAdd;
    public int attackAdd;
    public int defenseAdd;
    public Hability habilityAdd;
    public string removeTrigger;
    public int turnRemove;
    public bool stackable;

    public Buff(int _costAdd, int _attack, int _defense, Hability _hability, string _removeTrigger, int _turnRemove, bool _stackable)
    {
        costAdd = _costAdd;
        attackAdd = _attack;
        defenseAdd = _defense;
        habilityAdd = _hability;
        removeTrigger = _removeTrigger;
        turnRemove = _turnRemove;
        stackable = _stackable;
    }
}
