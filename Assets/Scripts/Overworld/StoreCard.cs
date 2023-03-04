using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class StoreCard : MonoBehaviour
{
    public CardScript card;
    public TextMeshProUGUI priceText;
    public int price;
    public Button buyButton;
    public GameObject soldOut;

    public void Restock()
    {
        buyButton.interactable = true;
        card.cardInstance.instanceID = 0;
        soldOut.gameObject.SetActive(false);
    }
    public CardInstance Buy()
    {
        buyButton.interactable = false;
        card.cardInstance.instanceID = 1;
        soldOut.gameObject.SetActive(true);
        return new CardInstance(card.cardClass.cardID, GetInstanceID(), card.cardInstance.value);
    }
    public void SetCard(CardInstance _card)
    {
        card = card.SetCard(_card, 0);
        price = card.cardClass.price;
        if (card.cardType == CardType.Creature)
            price += (((CreatureScript)card).morphIndex * 50);
        priceText.text = price.ToString();
        if (_card.instanceID == 0)
        {
            Restock();
        }
        else
        {
            Buy();
        }
    }
}
