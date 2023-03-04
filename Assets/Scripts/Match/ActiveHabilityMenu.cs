using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActiveHabilityMenu : MonoBehaviour
{
    public GameObject parent;
    public CardScript habilityOwner;
    public List<Hability> habilities;
    public List<TextMeshProUGUI> buttonTexts;
    public bool active;

    #region Singleton
    public static ActiveHabilityMenu Instance { get; private set; }
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
        if (Input.GetMouseButtonDown(0) && active)
        {
            Deactivate();
        }
    }

    public void SetMenu(CardScript _owner)
    {
        if (_owner == habilityOwner)
            return;

        List<Hability> habs = _owner.GetTriggers("Active");
        if (habs.Count == 0)
            return;


        for(int i = 0; i < habs.Count; i++)
        {
            if (!MatchController.Instance.CanPlayHability(habs[i], _owner, false))
            {
                habs.RemoveAt(i);
                i--;
            }
        }

        if (habs.Count == 0)
            return;

        parent.gameObject.SetActive(true);
        active = true;
        if(CardPreview.Instance.previewedCard != null)
            CardPreview.Instance.RemovePreview(CardPreview.Instance.previewedCard);
        parent.transform.position = Camera.main.WorldToScreenPoint(_owner.transform.position);
        habilityOwner = _owner;
        habilities = habs;

        for(int i = 0; i < buttonTexts.Count; i++)
        {
            if (i < habilities.Count)
            {
                buttonTexts[i].text = habilities[i].name;
                buttonTexts[i].transform.parent.gameObject.SetActive(true);
            }
            else
            {
                buttonTexts[i].transform.parent.gameObject.SetActive(false);
            }
        }
    }
    public void PlayHability(int _index)
    {
        MatchController.Instance.SetAction(new PhaseAction(TurnAction.PlayHability, habilityOwner, habilities[_index]), 0);
        Deactivate();
    }

    public void Deactivate()
    {
        Debug.Log("Deactivate");
        Invoke("DeactivateParent", 0.2f);
    }

    public void DeactivateParent()
    {
        active = false;
        habilityOwner = null;
        parent.gameObject.SetActive(false);
    }


}
