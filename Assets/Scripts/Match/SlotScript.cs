using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotScript : MonoBehaviour, IHolder, IInteractable
{
    public CreatureScript heldCard;
    public SlotType owner; //0 player, 1 frontEnemy, 2 backEnemy
    public SlotStatus status = SlotStatus.Empty;
    [SerializeField] bool Interactable = false;
    public float cardSize;
    public string layerName;
    public int layerPriority;

    public StackCardScript trap;
    public SpriteRenderer rend;
    public bool interactable { get { return Interactable; } set { SetInteractable(value); } }

    void Awake()
    {
        rend = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        cardSize = transform.GetChild(0).localScale.x;
    }
    public IEnumerator SetHeldCard(CreatureScript _card)
    {
        _card.transform.SetParent(transform);
        _card.holder = this;
        yield return StartCoroutine(_card.MoveCard(new Vector3(transform.position.x, transform.position.y + .01f, transform.position.z - .2f), cardSize, 30f, layerName, layerPriority));
        status = SlotStatus.Full;
        heldCard = _card;
        heldCard.holderGameObject = gameObject;
        if(trap != null)
        {
            trap.isTrap = TrapStatus.Activated;
            MatchController.Instance.AddStackCard(trap);
            RemoveTrap();
        }
    }

    public CardScript RemoveCard()
    {
        if(heldCard != null)
        {
            CardScript held = heldCard;
            heldCard.holder = null;
            heldCard.transform.SetParent(null);
            heldCard = null;
            status = SlotStatus.Empty;
            return held;
        }
        return null;
    }
    public StackCardScript RemoveTrap()
    {
        if(trap != null)
        {
            trap.GetComponent<BoxCollider2D>().enabled = false;
            trap = null;
        }
        return trap;
    }
    public void SetInteractable(bool _isInteractable)
    {
        Interactable = _isInteractable;
        if (_isInteractable)
        {
            rend.color = Color.blue;
        }
        else
        {
            rend.color = Color.white;
        }
    }
    public void SetTrap(StackCardScript _card)
    {
        if(trap != null)
        {
            Destroy(trap.gameObject);
        }
        trap = _card;
        trap.holder = this;

        StartCoroutine(_card.MoveCard(new Vector3(transform.position.x, transform.position.y, -0.1f), cardSize, 25f, "Board", -1));
        trap.GetComponent<BoxCollider2D>().enabled = true;
        _card.isTrap = TrapStatus.Active;
    }



    #region IHolder
    public bool OnClick(CardScript _card)
    {
        if(_card.owner == 0)
            ActiveHabilityMenu.Instance.SetMenu(_card);
        return false;
    }
    public bool OnCursorIn(CardScript _card)
    {
        CardPreview.Instance.SetPreviewedCard(_card);
        return false;
    }

    public void OnCursorIn()
    {

    }

    public bool OnCursorOut(CardScript _card)
    {
        CardPreview.Instance.RemovePreview(_card);
        return false;
    }

    public void OnCursorOut()
    {

    }

    public void OnDoubleClick()
    {

    }

    public bool OnDrag(CardScript _card)
    {
        return false;
    }

    public void OnDrag()
    {

    }

    public void OnDrop()
    {

    }

    public bool OnStartDrag()
    {
        return false;
    }

    void IInteractable.OnClick()
    {

    }

    void IInteractable.OnCursorIn()
    {

    }

    void IInteractable.OnCursorOut()
    {

    }

    void IInteractable.OnDoubleClick()
    {

    }

    void IInteractable.OnDrag()
    {

    }

    void IInteractable.OnDrop()
    {

    }

    bool IInteractable.OnStartDrag()
    {
        return false;
    }
    #endregion
}
