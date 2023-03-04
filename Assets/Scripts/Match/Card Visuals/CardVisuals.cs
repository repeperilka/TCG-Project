using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class CardVisuals : MonoBehaviour
{
    public CardScript baseCard;
    public virtual void UpdateVisuals()
    {

    }
    public virtual void SetCard(CardScript _card)
    {
        baseCard = _card;
        UpdateVisuals();
    }
    public virtual void SetLayer(string _layer, int _priority)
    {

    }
}
