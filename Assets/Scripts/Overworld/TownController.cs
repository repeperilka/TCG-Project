using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TownController : MonoBehaviour
{
    public int townIndex;

    public AudioSource enviromentalAudio;
    public AudioClip audioTown;
    public AudioClip audioCreature;
    public AudioClip audioSpells;

    public GameObject creatureShopPanel;
    public TextMeshProUGUI creatureGold;
    public StoreCard[] creatureShopCards;

    public GameObject spellShopPanel;
    public TextMeshProUGUI spellGold;
    public StoreCard[] spellShopCards;

    public GameObject deckPanel;


    private void OnEnable()
    {
        enviromentalAudio.Play();
    }

    private void Start()
    {
        creatureShopPanel.SetActive(false);
        spellShopPanel.SetActive(false);
        deckPanel.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SaveGame.currentSave.gold += 1000;
        }
    }
    public void SetTown(int _town)
    {
        townIndex = _town;
    }

    public void OpenCreatureShop()
    {
        creatureGold.text = SaveGame.currentSave.gold.ToString();
        creatureShopPanel.gameObject.SetActive(true);
        for(int i = 0; i < SaveGame.currentSave.towns[townIndex].creatureShop.Length; i++)
        {
            creatureShopCards[i].SetCard(SaveGame.currentSave.towns[townIndex].creatureShop[i]);
        }
        enviromentalAudio.clip = audioCreature;
        enviromentalAudio.Play();
    }
    public void CloseCreatureShop()
    {
        creatureShopPanel.SetActive(false);
        SaveGame.Save();
        enviromentalAudio.clip = audioTown;
        enviromentalAudio.Play();
    }

    public void BuyCreature(int _index)
    {
        if (SaveGame.currentSave.gold < creatureShopCards[_index].price)
            return;
        SaveGame.currentSave.gold -= creatureShopCards[_index].price;
        creatureGold.text = SaveGame.currentSave.gold.ToString();

        CardInstance boughtCard = creatureShopCards[_index].Buy();
        SaveGame.currentSave.towns[townIndex].creatureShop[_index].instanceID = 1;

        SaveGame.AddCardToInventory(boughtCard);
    }



    public void OpenSpellShop()
    {
        spellGold.text = SaveGame.currentSave.gold.ToString();
        spellShopPanel.gameObject.SetActive(true);
        for (int i = 0; i < SaveGame.currentSave.towns[townIndex].creatureShop.Length; i++)
        {
            spellShopCards[i].SetCard(SaveGame.currentSave.towns[townIndex].spellShop[i]);
        }
        enviromentalAudio.clip = audioSpells;
        enviromentalAudio.Play();
    }
    public void CloseSpellShop()
    {
        spellShopPanel.SetActive(false);
        SaveGame.Save();
        enviromentalAudio.clip = audioTown;
        enviromentalAudio.Play();
    }
    public void BuySpell(int _index)
    {
        if (SaveGame.currentSave.gold < spellShopCards[_index].price)
            return;
        SaveGame.currentSave.gold -= spellShopCards[_index].price;
        spellGold.text = SaveGame.currentSave.gold.ToString();

        CardInstance boughtCard = spellShopCards[_index].Buy();
        SaveGame.currentSave.towns[townIndex].spellShop[_index].instanceID = 1;

        SaveGame.currentSave.playerInventory.Add(boughtCard);
    }



    public void OpenDeckBuilderPanel()
    {
        deckPanel.gameObject.SetActive(true);
        DeckPanelController.Instance.UpdatePanels();
        if (!SaveGame.currentSave.tutorials[3])
        {
            OverworldController.Instance.deckBuildingTutorial.gameObject.SetActive(true);
        }
    }
    public void CloseDeckBuilderPanel()
    {
        deckPanel.SetActive(false);
    }
}
