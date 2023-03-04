using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardScript : MonoBehaviour, IInteractable
{
    //holder data (for interface interaction)
    [HideInInspector] public IHolder holder;
    [HideInInspector] public GameObject holderGameObject;

    //visuals to update
    public List<CardVisuals> visuals;

    //card data for gameplay
    [Header("Card Data")]
    public CardInstance cardInstance;
    public Card cardClass;

    public string cardName;
    public int cardCost;
    public CardType cardType;
    public string cardHability;
    public List<Hability> habilities;

    [Header("Gameplay Data")]
    public int owner;
    public bool interactable = false;
    public bool selected = false;
    public CardTargets targetLines;

    [Header("Card Movement")]
    bool moving;
    public Vector3 positionTarget;
    public IEnumerator moveRoutine;




    void Start()
    {
        targetLines = GetComponent<CardTargets>();
    }
    public virtual CardScript SetCard(CardInstance _card, int _owner)
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
                SpellScript spell = gameObject.AddComponent<SpellScript>();
                spell.SetSpell(_card, _owner, visuals);
                Destroy(this);
                return spell;
        }
        return null;
    }
    public StackCardScript StackCard(CardScript _givenCard, Hability _hability, bool _isHability)
    {
        StackCardScript stackScript = gameObject.AddComponent<StackCardScript>();
        stackScript = stackScript.SetStackCard(_givenCard, _hability, _isHability, visuals);
        Destroy(this);
        return stackScript;
    }
    public void SetInteractable(bool _interactable)
    {
        interactable = _interactable;
        UpdateVisuals();
    }
    public void SetSelected(bool _selected)
    {
        selected = _selected;
        UpdateVisuals();
    }
    public IEnumerator MoveCard(Vector3 _position, float _speed, string _layer, int _priority)
    {
        if(moveRoutine != null)
            StopCoroutine(moveRoutine);
        moveRoutine = MoveCardRoutine(_position, transform.localScale.x, _speed, _layer, _priority);
        yield return StartCoroutine(moveRoutine);
    }
    public IEnumerator MoveCard(Vector3 _position, float _size, float _speed, string _layer, int _priority)
    {
        if(moveRoutine != null)
            StopCoroutine(moveRoutine);
        moveRoutine = MoveCardRoutine(_position, _size, _speed, _layer, _priority);
        yield return StartCoroutine(moveRoutine);
    }
    public IEnumerator MoveCardRoutine(Vector3 _position, float _size, float _speed, string _layer, int _priority)
    {
        moving = true;
        SetLayer("Overlay", 0);
        positionTarget = _position;
        bool[] finish = { false, false };
        while (!finish[0] || !finish[1])
        {
            if (Vector3.Distance(transform.position, _position) > _speed * Time.deltaTime)
            {
                transform.position = Vector3.MoveTowards(transform.position, _position, _speed * Time.deltaTime);
            }
            else
            {
                finish[0] = true;
                transform.position = _position;
            }
            if (Vector3.Distance(transform.localScale, Vector3.one * _size) > _speed * .2f * Time.deltaTime)
            {
                transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * _size, _speed * .2f * Time.deltaTime);
            }
            else
            {
                finish[1] = true;
                transform.localScale = Vector3.one * _size;
            }
            yield return 0;
        }
        transform.position = _position;
        transform.localScale = Vector3.one * _size;
        SetLayer(_layer, _priority);
        moving = false;
    }

    public List<Hability> GetTriggers(string _trigger)
    {
        List<Hability> returnHab = new List<Hability>();
        foreach(Hability hab in habilities)
        {
            if(hab.trigger == _trigger)
            {
                returnHab.Add(hab);
            }
        }
        return returnHab;
    }
    public virtual List<Hability> ActivateTriggers(string _trigger)
    {
        return GetTriggers(_trigger);
    }

    public void ReduceCost(int _amount)
    {
        cardCost -= _amount;
        if(cardCost < 0)
        {
            cardCost = 0;
        }
        UpdateVisuals();
    }
    public void SetLayer(string _layer, int _priority)
    {
        foreach (CardVisuals visual in visuals)
        {
            visual.SetLayer(_layer, _priority);
        }
    }
    public void UpdateVisuals()
    {
        foreach(CardVisuals visuals in visuals)
        {
            visuals.UpdateVisuals();
        }
    }

    public virtual void NextTurn()
    {

    }

    #region Interactable
    public void OnClick()
    {
        if(holder != null && !moving)
        {
            holder.OnClick(this);
        }
        Debug.Log("OnClick");
    }

    public void OnCursorIn()
    {
        if (holder != null && !moving)
        {
            holder.OnCursorIn(this);
        }
    }

    public void OnCursorOut()
    {
        if (holder != null && !moving)
        {
            holder.OnCursorOut(this);
        }
    }

    public void OnDoubleClick()
    {
        Debug.Log("OnDoubleClick");
    }

    public void OnDrag()
    {
        Debug.Log("OnDrag");
    }

    public void OnDrop()
    {
        Debug.Log("OnDrop");
    }

    public bool OnStartDrag()
    {
        if (holder != null)
        {
            return holder.OnDrag(this);
        }
        return false;
    }
    #endregion


}
