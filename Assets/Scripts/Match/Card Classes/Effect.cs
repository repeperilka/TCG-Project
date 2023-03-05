using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Hability
{
    public string name;
    public int cost;
    [TextArea]
    public string description;
    public string effectID;
    public string trigger;
    public bool goesToStack;

    public Hability()
    {
        name = "";
        cost = 0;
        description = "";
        effectID = "";
        trigger = "";
        goesToStack = false;
    }
    public Hability(string _name, int _cost, string _description, string _effectID, string _trigger, bool _goesToStack)
    {
        name = _name;
        cost = _cost;
        description = _description;
        effectID = _effectID;
        trigger = _trigger;
        goesToStack = _goesToStack;
    }

}
[System.Serializable]
public class Effect
{
    public string[] targetCommands;
    public List<object> targets;

    public Effect(string[] _targets)
    {
        targetCommands = _targets;
        targets = new List<object>();
    }


    public virtual IEnumerator Execute(CardScript _owner, StackCardScript _this)
    {
        Debug.Log(this.GetType().Name);
        yield return 0;
    }
    public virtual Effect Copy()
    {
        return new Effect(targetCommands);
    }
}
[System.Serializable]
public class effect_draw : Effect
{
    public int amount;
    public effect_draw(int _amount) : base(new string[0])
    {
        amount = _amount;
    }
    public override IEnumerator Execute(CardScript _owner, StackCardScript _this)
    {
        PlayerDeckMatch.Instance.Draw(amount);
        //Debug.Log("Effect(" + _owner.cardClass.cardName + ", " + _owner.owner + "): Draw " + amount + " cards.");
        yield return 0;
    }
    public override Effect Copy()
    {
        return new effect_draw(amount);
    }
}
[System.Serializable]
public class effect_damage : Effect
{
    public int amount;
    public effect_damage(int _amount) : base(new string[] { "1 slot EnemyFrontSlot Any 0" })
    {
        amount = _amount;
    }
    public override IEnumerator Execute(CardScript _owner, StackCardScript _this)
    {
        SlotScript selectedSlot = (SlotScript)targets[0];
        if(selectedSlot.heldCard == null)
        {
            selectedSlot.SetTrap(_this);
        }
        else
        {
            if (((CreatureScript)selectedSlot.heldCard).DealDamage(amount))
            {
                yield return MatchController.Instance.StartCoroutine(MatchController.Instance.DestroyCreature(selectedSlot));
            }
        }
        Debug.Log("Effect(" + _owner.cardClass.cardName + ", " + _owner.owner + "): Deal " + amount + " damage.");
        yield return 0;
    }
    public override Effect Copy()
    {
        return new effect_damage(amount);
    }
}
[System.Serializable]
public class effect_capture_attack : Effect
{
    public int amount;
    public effect_capture_attack(int _maxPower) : base(new string[] { "1 slot EnemyFrontSlot Any 0 " + _maxPower.ToString() + " -1 -1" })
    {
        amount = _maxPower;
    }
    public override IEnumerator Execute(CardScript _owner, StackCardScript _this)
    {
        SlotScript selectedSlot = (SlotScript)targets[0];
        if (selectedSlot.heldCard == null)
        {
            selectedSlot.SetTrap(_this);
        }
        else
        {
            if(selectedSlot.heldCard.attack <= amount)
            {
                CreatureScript creature = selectedSlot.heldCard;
                yield return MatchController.Instance.StartCoroutine(MatchController.Instance.CaptureCreature(selectedSlot));
                Debug.Log("Effect(" + _owner.cardClass.cardName + ", " + _owner.owner + "): " + creature.creature.cardName + " has been captured!");
            }
            else
            {
                Debug.Log("Effect(" + _owner.cardClass.cardName + ", " + _owner.owner + "): The creature broke it's shackles");
            }
        }
        yield return 0;
    }
    public override Effect Copy()
    {
        return new effect_capture_attack(amount);
    }
}
[System.Serializable]
public class effect_capture_defense : Effect
{
    public int amount;
    public effect_capture_defense(int _maxDefense) : base(new string[] { "1 slot EnemyFrontSlot Any 0 -1 " + _maxDefense.ToString() + " -1" })
    {
        amount = _maxDefense;
    }
    public override IEnumerator Execute(CardScript _owner, StackCardScript _this)
    {
        SlotScript selectedSlot = (SlotScript)targets[0];
        if (selectedSlot.heldCard == null)
        {
            selectedSlot.SetTrap(_this);
        }
        else
        {
            if (selectedSlot.heldCard.defense <= amount)
            {
                CreatureScript creature = selectedSlot.heldCard;
                yield return MatchController.Instance.StartCoroutine(MatchController.Instance.CaptureCreature(selectedSlot));
                Debug.Log("Effect(" + _owner.cardClass.cardName + ", " + _owner.owner + "): " + creature.creature.cardName + " has been captured!");
            }
            else
            {
                Debug.Log("Effect(" + _owner.cardClass.cardName + ", " + _owner.owner + "): The creature broke it's shackles");
            }
        }
        yield return 0;
    }
    public override Effect Copy()
    {
        return new effect_capture_defense(amount);
    }
}
[System.Serializable]
public class effect_move_empty : Effect
{
    public effect_move_empty() : base(new string[] { "1 slot PlayerSlot Empty Any"}) { }
    public override IEnumerator Execute(CardScript _owner, StackCardScript _this)
    {
        SlotScript selectedSlot = (SlotScript)targets[0];
        if (selectedSlot.heldCard == null)
        {
            Debug.Log("Effect(" + _owner.cardClass.cardName + ", " + _owner.owner + "): Moving to empty slot");
            _owner.holderGameObject.GetComponent<SlotScript>().RemoveCard();
            GameController.Instance.StartCoroutine(selectedSlot.SetHeldCard((CreatureScript)_owner));
        }
        else
        {
            Debug.Log("Effect(" + _owner.cardClass.cardName + ", " + _owner.owner + "): Slot is already full");
        }
        yield return 0;
    }
    public override Effect Copy()
    {
        return new effect_move_empty();
    }
}
[System.Serializable]
public class effect_defense_add : Effect
{
    int amount;
    public effect_defense_add(int _amount) : base(new string[] { "1 slot PlayerSlot Any Any" }) { amount = _amount; }
    public override IEnumerator Execute(CardScript _owner, StackCardScript _this)
    {
        SlotScript selectedSlot = (SlotScript)targets[0];
        if (selectedSlot.heldCard == null)
        {
            selectedSlot.SetTrap(_this);
        }
        else
        {
            CreatureScript creature = selectedSlot.heldCard;
            creature.AddDefense(amount);
            Debug.Log("Effect(" + _owner.cardClass.cardName + ", " + _owner.owner + "): " + creature.creature.cardName + " recieved +1 Defense!");
        }
        yield return 0;
    }
    public override Effect Copy()
    {
        return new effect_defense_add(amount);
    }
}
[System.Serializable]
public class effect_attack_add : Effect
{
    int amount;
    public effect_attack_add(int _amount) : base(new string[] { "1 slot PlayerSlot Any Any" }) { amount = _amount; }
    public override IEnumerator Execute(CardScript _owner, StackCardScript _this)
    {
        SlotScript selectedSlot = (SlotScript)targets[0];
        if (selectedSlot.heldCard == null)
        {
            selectedSlot.SetTrap(_this);
        }
        else
        {
            CreatureScript creature = selectedSlot.heldCard;
            creature.AddAttack(amount);
            Debug.Log("Effect(" + _owner.cardClass.cardName + ", " + _owner.owner + "): " + creature.creature.cardName + " recieved +1 Attack!");
        }
        yield return 0;
    }
    public override Effect Copy()
    {
        return new effect_defense_add(amount);
    }
}
[System.Serializable]
public class effect_dragon_morph_3 : Effect
{
    public effect_dragon_morph_3() : base(new string[] {  }) { }
    public override IEnumerator Execute(CardScript _owner, StackCardScript _this)
    {
        int attackToAdd = Mathf.RoundToInt(SaveGame.currentSave.gold / 10000f);
        ((CreatureScript)_owner).AddAttack(attackToAdd);
        yield return 0;
    }
    public override Effect Copy()
    {
        return new effect_dragon_morph_3();
    }
}
[System.Serializable]
public class effect_boss_1 : Effect
{
    public effect_boss_1() : base(new string[] {  }) { }
    public override IEnumerator Execute(CardScript _owner, StackCardScript _this)
    {
        for(int i = 0; i < 3; i++)
        {
            int randomSlot = Random.Range(0, 5);
            _owner.holderGameObject.GetComponent<SlotScript>().RemoveCard();
            yield return BoardController.Instance.StartCoroutine(BoardController.Instance.enemyFrontSlots[randomSlot].SetHeldCard((CreatureScript)_owner));
            yield return BoardController.Instance.StartCoroutine(MatchController.Instance.AttackAnimation(BoardController.Instance.enemyFrontSlots[randomSlot], BoardController.Instance.playerSlots[randomSlot]));
        }
        yield return 0;
    }
    public override Effect Copy()
    {
        return new effect_boss_1();
    }
}




[System.Serializable]
public class self_reduce_cost : Effect
{
    int amount;
    public self_reduce_cost(int _amount) : base(new string[0]) { amount = _amount; }
    public override IEnumerator Execute(CardScript _owner, StackCardScript _this)
    {
        Debug.Log("Effect(" + _owner.cardClass.cardName + ", " + _owner.owner + "): Reducing " + _owner.cardClass.cardName + "'s cost by " + amount.ToString());
        _owner.ReduceCost(amount);
        yield return 0;
    }
    public override Effect Copy()
    {
        return new self_reduce_cost(amount);
    }
}
[System.Serializable]
public class self_attack_add : Effect
{
    int amount;
    public self_attack_add(int _amount) : base(new string[0]) { amount = _amount; }
    public override IEnumerator Execute(CardScript _owner, StackCardScript _this)
    {
        Debug.Log("Effect(" + _owner.cardClass.cardName + ", " + _owner.owner + "): Raising " + _owner.cardClass.cardName + "'s attack by " + amount.ToString());
        CreatureScript creature = (CreatureScript)_owner;
        creature.AddAttack(amount);
        yield return 0;
    }
    public override Effect Copy()
    {
        return new self_attack_add(amount);
    }
}
[System.Serializable]
public class self_defense_add : Effect
{
    int amount;
    public self_defense_add(int _amount) : base(new string[0]) { amount = _amount; }
    public override IEnumerator Execute(CardScript _owner, StackCardScript _this)
    {
        Debug.Log("Effect(" + _owner.cardClass.cardName + ", " + _owner.owner + "): Raising " + _owner.cardClass.cardName + "'s defense by " + amount.ToString());
        CreatureScript creature = (CreatureScript)_owner;
        creature.AddDefense(amount);
        yield return 0;
    }
    public override Effect Copy()
    {
        return new self_defense_add(amount);
    }
}
[System.Serializable]
public class effect_desert_critters_buff : Effect
{
    public effect_desert_critters_buff() : base(new string[0]) {  }
    public override IEnumerator Execute(CardScript _owner, StackCardScript _this)
    {
        SlotScript[] slots = BoardController.Instance.GetSlots((SlotType)_owner.owner, SlotStatus.Full);
        foreach(SlotScript slot in slots)
        {
            if(slot.heldCard.cardClass.cardID == _owner.cardClass.cardID)
            {
                slot.heldCard.AddBuff(new Buff(0, 1, 1, new Hability(), "OnThisExit", 2, true));
            }
        }
        yield return 0;
    }
    public override Effect Copy()
    {
        return new effect_desert_critters_buff();
    }
}

//[System.Serializable]
//public enum EffectTrigger
//{
//    Active,
//    OnCardInHand,
//    OnCardPlayed,
//
//    OnOwnedCreatureEnter,
//    OnOwnedCreatureExit,
//    OnEnemyCreatureEnter,
//    OnEnemyCreatureExit,
//    OnCreatureEnter,
//    OnCreatureExit,
//
//    OnThisEnter,
//    OnThisExit
//}