using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour, IHolder
{
    public List<CardScript> heldCards = new List<CardScript>();
    public CardScript highLighted;
    [SerializeField] float maxSeparation;
    [SerializeField] float width;
    [SerializeField] float cardSize;
    [SerializeField] bool autoSort;
    [SerializeField] float highlightedHeight;
    [SerializeField] bool Interactable = true;
    public bool interactable
    {
        get { return Interactable; }
        set { SetInteractable(value); }
    }

    #region Singleton
    public static PlayerHand Instance { get; private set; }
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
    }
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            AddCard(GameController.Instance.InstantiateCard("dog", new Vector3(-10, transform.position.y, 0), 0));
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            AddCard(GameController.Instance.InstantiateCard("knoledge", new Vector3(-10, transform.position.y, 0), 0));
        }
        if (autoSort)
        {
            Sort();
        }
    }
    public void AddCard(CardScript _card)
    {
        float cardX = _card.transform.position.x;

        Vector3 rayPoint = new Vector3(cardX, -4f, -10f);
        RaycastHit2D hit = Physics2D.Raycast(rayPoint, Vector2.zero);
        bool found = false;

        if (hit.collider != null)
        {
            if (heldCards.Contains(hit.collider.GetComponent<CardScript>()))
            {
                heldCards.Insert(heldCards.IndexOf(hit.collider.GetComponent<CardScript>()), _card);
                found = true;
            }
        }
        if (!found)
        {
            if(cardX > 0)
            {
                heldCards.Add(_card);
            }
            else
            {
                heldCards.Insert(0, _card);
            }
        }
        _card.transform.SetParent(transform);
        _card.holder = this;
        Sort();
    }
    public void Sort()
    {
        float separation = width / (heldCards.Count - 1f);
        float offset = 0;
        if(separation > maxSeparation)
        {
            separation = maxSeparation;
            offset = (width - (separation * (heldCards.Count - 1f))) / 2f;
        }

        float currentX = (-width / 2f) + offset;
        for(int i = 0; i < heldCards.Count; i++)
        {
            StartCoroutine(heldCards[i].MoveCard(new Vector3(currentX, transform.position.y, transform.position.z + i * .01f), cardSize, 25f, "Hand", -i * 10));
            currentX += separation;
        }
    }
    public void SetInteractable(bool _value)
    {
        Interactable = _value;
        if(_value)
        {
            SetInteractables();
        }
        else
        {
            RemoveInteractables();
        }
    }

    public void RemoveCard(CardScript _card)
    {
        heldCards.Remove(_card);
        _card.transform.SetParent(null);
        _card.holder = null;
        Sort();
    }

    public bool OnDrag(CardScript _card)
    {
        if (interactable && heldCards.Contains(_card))
        {
            RemoveCard(_card);
            return true;
        }
        return false;
    }
    public bool OnCursorIn(CardScript _card)
    {
        CardPreview.Instance.SetPreviewedCard(_card);
        return false;
    }
    public bool OnCursorOut(CardScript _card)
    {
        CardPreview.Instance.RemovePreview(_card);
        return false;
    }
    public bool OnClick(CardScript _card)
    {
        return false;
    }
    public void SetInteractables()
    {
        for(int i = 0; i < heldCards.Count; i++)
        {
            if (MatchController.Instance.CanPlay(heldCards[i], false))
            {
                heldCards[i].SetInteractable(true);
            }
        }
    }
    public void RemoveInteractables()
    {
        for (int i = 0; i < heldCards.Count; i++)
        {
            heldCards[i].SetInteractable(false);
        }
    }

}
public interface IHolder
{
    bool OnDrag(CardScript _card);
    bool OnCursorIn(CardScript _card);
    bool OnCursorOut(CardScript _card);
    bool OnClick(CardScript _card);
}
