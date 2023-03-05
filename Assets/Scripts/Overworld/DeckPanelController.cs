using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DeckPanelController : MonoBehaviour
{
    GameObject inventoryCardPrefab;
    public Transform inventoryParent;
    public List<CardUIDeck> inventoryCards;
    public Transform deckParent;
    public List<CardUIDeck> deckCards;

    public LayerMask hitMask;
    public GraphicRaycaster raycaster;

    public CardUIDeck heldcard;
    public TextMeshProUGUI goldText;



    #region Singleton
    public static DeckPanelController Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        inventoryCardPrefab = Resources.Load<GameObject>("Prefabs/inventory_card");
    }
    #endregion
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (heldcard != null)
        {
            heldcard.transform.position = Input.mousePosition;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            UpdatePanels();
        }
    }

    public void SetHeldCard(CardUIDeck _heldcard)
    {
        heldcard = _heldcard;
        RectTransform rTrans = heldcard.GetComponent<RectTransform>();
        rTrans.sizeDelta = new Vector2(rTrans.sizeDelta.x * 2f, rTrans.sizeDelta.y * 2f);
        heldcard.transform.SetParent(transform);
    }
    public void DropHeldCard()
    {
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(data, results);

        string owner = "";
        foreach(RaycastResult result in results)
        {
            if (result.gameObject.layer == 8)
                owner = "deck";
            if (result.gameObject.layer == 9)
                owner = "inventory";
            if (result.gameObject.layer == 10)
                owner = "sell";
        }

        heldcard.transform.SetParent(heldcard.cardOwner == "deck" ? deckParent : inventoryParent);

        switch (owner)
        {
            case "":
                heldcard.transform.SetParent(heldcard.cardOwner == "deck" ? deckParent : inventoryParent);
                break;
            case "inventory":
                if(heldcard.cardOwner != "inventory")
                {
                    SaveGame.RemoveCardFromDeck(heldcard.card.cardInstance);
                    SaveGame.AddCardToInventory(heldcard.card.cardInstance);
                }
                break;
            case "deck":
                if(heldcard.cardOwner != "deck")
                {
                    SaveGame.RemoveCardFromInventory(heldcard.card.cardInstance);
                    SaveGame.AddCardToDeck(heldcard.card.cardInstance);
                }
                break;
            case "sell":
                switch (heldcard.cardOwner)
                {
                    case "deck":
                        SaveGame.RemoveCardFromDeck(heldcard.card.cardInstance);
                        break;
                    case "inventory":
                        SaveGame.RemoveCardFromInventory(heldcard.card.cardInstance);
                        break;
                }
                SaveGame.currentSave.gold += Mathf.RoundToInt(heldcard.card.cardClass.price * .4f);
                break;
        }
        UpdatePanels();

        heldcard = null;
    }
    public void UpdatePanels()
    {
        if (inventoryCards == null)
            inventoryCards = new List<CardUIDeck>();

        goldText.text = SaveGame.currentSave.gold.ToString();
        for (int i = 0; i < inventoryCards.Count; i++)
        {
            if (i < SaveGame.currentSave.playerInventory.Count)
            {
                inventoryCards[i].gameObject.SetActive(true);
                inventoryCards[i].transform.SetSiblingIndex(i);
                inventoryCards[i].SetCard(SaveGame.currentSave.playerInventory[i], "inventory");
            }
            else
            {
                inventoryCards[i].gameObject.SetActive(false);
            }
        }
        for (int i = inventoryCards.Count; i < SaveGame.currentSave.playerInventory.Count; i++)
        {
            CardUIDeck newCard = Instantiate(inventoryCardPrefab, inventoryParent).GetComponent<CardUIDeck>();
            newCard.SetCard(SaveGame.currentSave.playerInventory[i], "inventory");
            inventoryCards.Add(newCard);
            newCard.gameObject.name = i.ToString();
            inventoryCards[i].transform.SetSiblingIndex(i);
        }

        if (deckCards == null)
            deckCards = new List<CardUIDeck>();
        for (int i = 0; i < deckCards.Count; i++)
        {
            if (i < SaveGame.currentSave.playerDeck.Count)
            {
                deckCards[i].gameObject.SetActive(true);
                deckCards[i].transform.SetSiblingIndex(i);
                deckCards[i].SetCard(SaveGame.currentSave.playerDeck[i], "deck");
            }
            else
            {
                deckCards[i].gameObject.SetActive(false);
            }
        }
        for (int i = deckCards.Count; i < SaveGame.currentSave.playerDeck.Count; i++)
        {
            CardUIDeck newCard = Instantiate(inventoryCardPrefab, deckParent).GetComponent<CardUIDeck>();
            newCard.SetCard(SaveGame.currentSave.playerDeck[i], "deck");
            deckCards.Add(newCard);
            newCard.gameObject.name = i.ToString();
            deckCards[i].transform.SetSiblingIndex(i);
        }
    }
}
