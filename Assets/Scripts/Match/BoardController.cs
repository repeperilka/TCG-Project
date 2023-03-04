using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BoardController : MonoBehaviour
{
    public List<SlotScript> playerSlots;
    public List<SlotScript> enemyFrontSlots;
    public List<SlotScript> enemyBackSlots;

    public LayerMask slotMask;

    public int[] playerCrystals = new int[2]; //current / maximum
    public TextMeshProUGUI playerHpText;
    public TextMeshProUGUI playerGoldText;

    public SlotScript selectedSlot;
    public bool cancelSelection = false;
    public GameObject cancelSelectionButton;
    public TextMeshProUGUI continueButton;



    public Image[] crystalImages;
    public Color[] emptyFullCrystalColors;

    public PlayerInteraction playerInteraction;



    #region Singleton
    public static BoardController Instance { get; private set; }
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
    private void Start()
    {
        playerHpText.text = SaveGame.currentSave.playerHP[0].ToString();
        playerCrystals = new int[] { 1, 1 };
        UpdateCrystals();
        UpdateGold();
    }


    public IEnumerator SelectSlot(SlotType _owner, SlotStatus _status, SlotStatus _trap, CreatureFilter[] _filters) //empty = 0 only empty, 1 only full, 2 any: hastrap = 0 no, 1 yes, 2 doesn't matter
    {
        SetSlotsInteractable(false, SlotType.Any);
        SlotScript[] selectedSlots = GetSlots(_owner, _status, _trap, _filters);
        foreach(SlotScript thisSlot in selectedSlots)
        {
            thisSlot.interactable = true;
        }

        //onCursor variables
        IInteractable interactable = null;
        SlotScript slot = null;
        selectedSlot = null;
        cancelSelection = false;
        cancelSelectionButton.SetActive(true);

        //slot selection cycle
        while(selectedSlot == null)
        {
            //raycast and interactable selection (for IInteractable functions)
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, slotMask);

            if (hit.collider != null)
            {
                //gets the hit slot and checks if it is interactable
                SlotScript thisSlot = hit.collider.GetComponent<SlotScript>();
                if(thisSlot.interactable)
                {
                    //gets the interactable to check for OnCursorOut / OnCursorIn
                    IInteractable newInteractable = hit.collider.GetComponent<IInteractable>();
                    if (newInteractable != interactable)
                    {
                        if (interactable != null)
                        {
                            interactable.OnCursorOut();
                            interactable = null;
                            slot = null;
                        }
                        interactable = newInteractable;
                        interactable.OnCursorOut();
                        slot = hit.collider.GetComponent<SlotScript>();

                    }
                }
                else
                {
                    if (interactable != null)
                    {
                        interactable.OnCursorOut();
                        interactable = null;
                        slot = null;
                    }
                }
            }
            else
            {
                if (interactable != null)
                {
                    interactable.OnCursorOut();
                    interactable = null;
                    slot = null;
                }
            }

            //selects the slot hovered and sets the selectedSlot variable for external scripts to access
            if (Input.GetMouseButtonDown(0) && slot != null)
            {
                interactable.OnCursorOut();
                selectedSlot = slot;
            }
            if (cancelSelection)
            {
                break;
            }
            yield return 0;
        }

        //sets all slots interactable
        SetSlotsInteractable(false, SlotType.Any);
        cancelSelectionButton.SetActive(false);
    }
    public void CancelSlotSelection()
    {
        cancelSelection = true;
    }
    public int EmptySlotsCount(SlotType[] _owners)
    {
        int finalSlots = 0;
        foreach(SlotType type in _owners)
        {
            finalSlots += EmptySlotsCount(type);
        }
        return finalSlots;
    }
    public int EmptySlotsCount(SlotType _owner)
    {
        int empty = 0;
        if (_owner == SlotType.Any || _owner == SlotType.PlayerSlot)
        {
            foreach (SlotScript slot in playerSlots)
            {
                if (slot.heldCard == null)
                    empty++;
            }
        }
        if (_owner == SlotType.Any || _owner == SlotType.EnemyFrontSlot)
        {
            foreach (SlotScript slot in enemyFrontSlots)
            {
                if (slot.heldCard == null)
                    empty++;
            }
        }
        if (_owner == SlotType.Any || _owner == SlotType.EnemyBackSlot)
        {
            foreach (SlotScript slot in enemyBackSlots)
            {
                if (slot.heldCard == null)
                    empty++;
            }
        }
        return empty;
    }

    public SlotScript[] GetSlots(SlotType _owner, SlotStatus _status)
    {
        return GetSlots(_owner, _status, SlotStatus.Any, new CreatureFilter[0]);
    }
    public SlotScript[] GetSlots(SlotType _owner, SlotStatus _status, SlotStatus _hasTrap, CreatureFilter[] _filters)
    {
        List<SlotScript> selectedSlots = new List<SlotScript>();
        //sets the possible slots interactable
        switch (_owner)
        {
            case SlotType.PlayerSlot:
                selectedSlots = new List<SlotScript>(playerSlots);
                break;
            case SlotType.EnemyFrontSlot:
                selectedSlots = new List<SlotScript>(enemyFrontSlots);
                break;
            case SlotType.EnemyBackSlot:
                selectedSlots = new List<SlotScript>(enemyBackSlots);
                break;
            default:
                selectedSlots.AddRange(playerSlots);
                selectedSlots.AddRange(enemyFrontSlots);
                selectedSlots.AddRange(enemyBackSlots);
                break;
        }
        for(int i = 0; i < selectedSlots.Count; i++)
        {
            if (_status != SlotStatus.Any && _status != selectedSlots[i].status)
            {
                selectedSlots.RemoveAt(i);
                i--;
                continue;
            }
            switch (_hasTrap)
            {
                case SlotStatus.Empty:
                    if (selectedSlots[i].trap != null)
                    {
                        selectedSlots.RemoveAt(i);
                        i--;
                        continue;
                    }
                    break;
                case SlotStatus.Full:
                    if (selectedSlots[i].trap == null)
                    {
                        selectedSlots.RemoveAt(i);
                        i--;
                        continue;
                    }
                    break;

            }
            if (_filters.Length != 0)
            {
                for(int e = 0; e < _filters.Length; e++)
                {
                    if (!_filters[e].CheckCreature((CreatureScript)selectedSlots[i].heldCard))
                    {
                        selectedSlots.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }
        }
        return selectedSlots.ToArray();
    }

    public int SlotsCount(SlotType _owner, SlotStatus _status, SlotStatus _trap, CreatureFilter[] _filters)
    {
        SlotScript[] selectedSlots = GetSlots(_owner, _status, _trap, _filters);
        return selectedSlots.Length;
    }


    public void SetSlotsInteractable(bool _value, SlotType _slotType) //sets slots interactable variables
    {
        if(_slotType == SlotType.Any ||_slotType == SlotType.PlayerSlot)
        {
            foreach (SlotScript slot in playerSlots)
            {
                slot.interactable = _value;
            }
        }
        if (_slotType == SlotType.Any || _slotType == SlotType.EnemyFrontSlot)
        {
            foreach (SlotScript slot in enemyFrontSlots)
            {
                slot.interactable = _value;
            }
        }
        if (_slotType == SlotType.Any || _slotType == SlotType.EnemyBackSlot)
        {
            foreach (SlotScript slot in enemyBackSlots)
            {
                slot.interactable = _value;
            }
        }
    }
    public void SetSlotsInteractableException(bool _value, SlotType _slotType)
    {
        if (_slotType != SlotType.PlayerSlot)
        {
            foreach (SlotScript slot in playerSlots)
            {
                slot.interactable = _value;
            }
        }
        if (_slotType != SlotType.EnemyFrontSlot)
        {
            foreach (SlotScript slot in enemyFrontSlots)
            {
                slot.interactable = _value;
            }
        }
        if (_slotType != SlotType.EnemyBackSlot)
        {
            foreach (SlotScript slot in enemyBackSlots)
            {
                slot.interactable = _value;
            }
        }
    }
    public void SetPlayerActiveSlots()
    {
        foreach(SlotScript slot in playerSlots)
        {
            if(slot.heldCard != null)
            {
                List<Hability> triggers = slot.heldCard.GetTriggers("Active");
                if(triggers.Count != 0)
                {
                    bool canPlay = false;
                    for(int i = 0; i < triggers.Count; i++)
                    {
                        if (MatchController.Instance.CanPlayHability(triggers[i], slot.heldCard, false))
                        {
                            canPlay = true;
                            break;
                        }
                    }
                    slot.interactable = canPlay;
                    slot.heldCard.SetInteractable(canPlay);
                }
                else
                {
                    slot.interactable = false;
                }
            }
            else
            {
                slot.interactable = false;
            }

        }
    }
    public void ResetPlayerActiveSlots()
    {
        foreach (SlotScript slot in playerSlots)
        {
            if (slot.heldCard != null)
            {
                slot.heldCard.SetInteractable(false);
            }
            slot.interactable = false;
        }
    }

    public void AddCrystal(int _amount)
    {
        playerCrystals[1] += _amount;
        if (playerCrystals[1] > 10)
            playerCrystals[1] = 10;
        UpdateCrystals();
    }
    public void ReplenishCrystals(int _amount)
    {
        playerCrystals[0] += _amount;
        if (playerCrystals[0] > playerCrystals[1])
            playerCrystals[0] = playerCrystals[1];
        UpdateCrystals();
    }
    public void ReplenishCrystals()
    {
        playerCrystals[0] = playerCrystals[1];
        UpdateCrystals();
    }
    public void RemoveCrystals(int _amount)
    {
        playerCrystals[0]-= _amount;
        if (playerCrystals[0] < 0)
            playerCrystals[0] = 0;
        UpdateCrystals();
    }
    public void RemoveCrystals()
    {
        playerCrystals[0] = 0;
        UpdateCrystals();
    }
    public void UpdateCrystals()
    {
        for(int i = 0; i < crystalImages.Length; i++)
        {
            if(i < playerCrystals[1])
            {
                crystalImages[i].gameObject.SetActive(true);
                if(i < playerCrystals[0])
                {
                    StartCoroutine(ReplenishCrystal(crystalImages[i]));
                }
                else
                {
                    StartCoroutine(EmptyCrystal(crystalImages[i]));
                }
            }
            else
            {
                crystalImages[i].gameObject.SetActive(false);
            }
        }
    }
    public IEnumerator ReplenishCrystal(Image _crystal)
    {
        _crystal.color = emptyFullCrystalColors[1];
        yield break;
    }
    public IEnumerator EmptyCrystal(Image _crystal)
    {
        _crystal.color = emptyFullCrystalColors[0];
        yield break;
    }


    public void DealDamageToPlayer(int _amount)
    {
        SaveGame.currentSave.playerHP[0] -= _amount;
        playerHpText.text = SaveGame.currentSave.playerHP[0].ToString();
    }

    public void UpdateGold()
    {
        playerGoldText.text = (SaveGame.currentSave.gold + MatchController.Instance.earnedGold).ToString();
    }

    public Vector3 ColorToVector(Color _color)
    {
        float h, v, s;
        Color.RGBToHSV(_color, out h, out s, out v);
        return new Vector3(h, s, v);
    }
    public Color VectorToColor(Vector3 _color)
    {
        Color newColor = Color.HSVToRGB(_color.x, _color.y, _color.z);
        return newColor;
    }

}
[System.Serializable]
public enum SlotType //index related to owner of the slot
{
    PlayerSlot,
    EnemyFrontSlot,
    EnemyBackSlot,
    Any
}
[System.Serializable]
public enum SlotStatus
{
    Any,
    Empty,
    Full
}
