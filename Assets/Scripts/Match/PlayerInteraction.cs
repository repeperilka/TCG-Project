using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public GameObject targetObject;
    [SerializeField]
    public IInteractable target;
    public InteractionAction lastAction;

    public CardScript heldCard;

    Vector2 mouseDownPosition;


    void Update()
    {
        IInteractable newInteractable = null;
        GameObject newObject = null;

        Vector2 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(cursorWorldPosition, Vector2.zero);

        if (hit.collider != null)
        {
            newInteractable = hit.collider.GetComponent<IInteractable>();
            newObject = hit.collider.gameObject;
        }


        switch (lastAction)
        {
            case InteractionAction.None:
                if(newInteractable != null)
                {
                    target = newInteractable;
                    targetObject = newObject;
                    lastAction = InteractionAction.OnCursorIn;
                    target.OnCursorIn();
                }
                break;
            case InteractionAction.OnCursorIn:
                if(newInteractable != target)
                {
                    target.OnCursorOut();
                    target = null;
                    targetObject = null;
                    lastAction = InteractionAction.None;
                }
                if(newInteractable != null && target == null)
                {
                    target = newInteractable;
                    targetObject = newObject;
                    lastAction = InteractionAction.OnCursorIn;
                    target.OnCursorIn();
                }
                if (Input.GetMouseButtonDown(0))
                {
                    target.OnCursorOut();
                    lastAction = InteractionAction.OnMouseDown;
                    mouseDownPosition = Input.mousePosition;
                }
                break;
            case InteractionAction.OnMouseDown:
                if (Input.GetMouseButtonUp(0))
                {
                    lastAction = InteractionAction.None;
                    target.OnClick();

                }
                if(Vector2.Distance(Input.mousePosition, mouseDownPosition) > 10f)
                {
                    if (target.OnStartDrag())
                    {
                        MatchController.Instance.PlayAudio(MatchAudio.PickCard);
                        heldCard = targetObject.GetComponent<CardScript>();
                        heldCard.GetComponent<BoxCollider2D>().enabled = false;
                        heldCard.SetLayer("Overlay", 0);
                        heldCard.transform.localScale = Vector3.one * 3f;
                        lastAction = InteractionAction.OnDrag;
                    }
                    else
                    {
                        lastAction = InteractionAction.None;
                    }
                }
                break;
            case InteractionAction.OnDrag:
                heldCard.transform.position = new Vector3(cursorWorldPosition.x, cursorWorldPosition.y, -1f);
                if (Input.GetMouseButtonUp(0))
                {
                    if (heldCard.transform.position.y > -2 && MatchController.Instance.CanPlay(heldCard, false))
                    {
                        MatchController.Instance.SetAction(new PhaseAction(heldCard.cardType == CardType.Creature ? TurnAction.PlayCreature : TurnAction.PlaySpell, heldCard, null), 0);
                    }
                    else
                    {
                        PlayerHand.Instance.AddCard(heldCard);
                    }
                    heldCard.GetComponent<BoxCollider2D>().enabled = true;
                    heldCard = null;
                    target = null;
                    targetObject = null;
                    lastAction = InteractionAction.None;
                }
                break;
        }
    }

    public void Continue()
    {
        MatchController.Instance.SetAction(new PhaseAction(TurnAction.Continue, null, null), 0);
    }
}
[System.Serializable]
public enum InteractionAction
{
    None,
    OnCursorIn,
    OnCursorOut,
    OnMouseDown,
    OnClick,
    OnDoubleClick,
    OnStartDrag,
    OnDrag,
    OnDrop
}
public interface IInteractable
{
    void OnCursorIn();
    void OnCursorOut();
    void OnClick();
    void OnDoubleClick();
    bool OnStartDrag();
    void OnDrag();
    void OnDrop();
}
