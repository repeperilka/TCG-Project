using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardUIDeck : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public CardScript card;
    public string cardOwner; //"deck", "inventory"

    public void SetCard(CardInstance _card, string _owner)
    {
        card = card.SetCard(_card, 0);
        cardOwner = _owner;
    }
    public void SetCardOwner(string _owner)
    {
        cardOwner = _owner;
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        DeckPanelController.Instance.SetHeldCard(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DeckPanelController.Instance.DropHeldCard();
    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }
}
