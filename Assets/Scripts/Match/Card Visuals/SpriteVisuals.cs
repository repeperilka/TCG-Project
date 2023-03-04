using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class SpriteVisuals : CardVisuals
{
    public SpriteCardLayout[] layouts;
    int layoutIndex;
    SpriteCardLayout thisLayout;

    private void Start()
    {

    }

    public override void SetCard(CardScript _card)
    {
        baseCard = _card;

        //sets up the correct layout
        layoutIndex = (int)_card.cardType;
        thisLayout = layouts[layoutIndex];
        for (int i = 0; i < layouts.Length; i++)
        {
            layouts[i].parent.gameObject.SetActive(i == layoutIndex);
        }
        UpdateVisuals();
    }

    public override void UpdateVisuals()
    {
        if (baseCard.selected)
        {
            thisLayout.sprites[0].gameObject.SetActive(true);
            thisLayout.sprites[0].color = new Color(0.98f, 0.85f, 0);
        }
        else if (baseCard.interactable)
        {
            thisLayout.sprites[0].gameObject.SetActive(true);
            thisLayout.sprites[0].color = new Color(0, 0.53f, 1f);
        }
        else
        {
            thisLayout.sprites[0].gameObject.SetActive(false);
        }
        //populates visualization with class based variables
        switch (baseCard.cardType)
        {
            case CardType.Creature:
                CreatureScript creature = (CreatureScript)baseCard;
                thisLayout.sprites[1].sprite = creature.creature.sprite;
                thisLayout.texts[0].text = creature.cardName;
                thisLayout.texts[1].text = creature.cardType.ToString();
                thisLayout.texts[2].text = creature.cardHability;
                thisLayout.texts[3].text = creature.cardCost.ToString();
                thisLayout.texts[4].color = CardDictionary.GetRarityColor(creature.cardClass.rarity);
                thisLayout.texts[5].text = creature.morph;
                thisLayout.texts[6].text = creature.attack + "/" + creature.defense;
                break;
            case CardType.Spell:
                SpellScript spell = (SpellScript)baseCard;
                thisLayout.sprites[1].sprite = baseCard.cardClass.sprite;
                thisLayout.texts[0].text = baseCard.cardName;
                thisLayout.texts[1].text = baseCard.cardType.ToString();
                thisLayout.texts[2].text = baseCard.cardHability;
                thisLayout.texts[3].text = baseCard.cardCost.ToString();
                thisLayout.texts[4].color = CardDictionary.GetRarityColor(spell.cardClass.rarity);
                break;
            case CardType.Hability:
                StackCardScript stackCard = (StackCardScript)baseCard;
                thisLayout.sprites[1].sprite = stackCard.givenCard.cardClass.sprite;
                thisLayout.texts[0].text = baseCard.cardName;
                thisLayout.texts[1].text = stackCard.isTrap != TrapStatus.None ? "Trap" : "Hability";
                thisLayout.texts[2].text = baseCard.cardHability;
                break;
        }
    }

    public override void SetLayer(string _layer, int _priority)
    {
        for(int i = 0; i < thisLayout.sprites.Length; i++)
        {
            thisLayout.sprites[i].sortingLayerName = _layer;
            thisLayout.sprites[i].sortingOrder = _priority + i;
        }
        for (int i = 0; i < thisLayout.texts.Length; i++)
        {
            thisLayout.texts[i].sortingLayerID = SortingLayer.NameToID(_layer);
            thisLayout.texts[i].sortingOrder = _priority + thisLayout.sprites.Length;
        }
    }
}
[System.Serializable]
public class SpriteCardLayout
{
    public Transform parent;
    public SpriteRenderer[] sprites; //cardSelection, image, frame, (creature)attk/def
    public TextMeshPro[] texts; //name, type, hability, cost, rarity, morph, attk/def
}
