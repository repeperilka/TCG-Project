using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardPreview : MonoBehaviour
{
    public UIVisuals previewCard;
    public CardScript previewedCard;
    public Vector4 bounds;
    public float previewSpeed = 20f;

    IEnumerator routine;

    bool Visualizable = true;
    public bool visualizable { get { return Visualizable; } set { Visualizable = value; RemovePreview(previewedCard); } }

    #region Singleton
    public static CardPreview Instance { get; private set; }
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
        bounds = new Vector4(194.5f, 265f, Screen.width - 194.5f, Screen.height - 265);
        routine = PreviewAnimation();
    }

    public void SetPreviewedCard(CardScript _card)
    {
        if (!visualizable || ActiveHabilityMenu.Instance.active)
            return;
        previewedCard = _card;
        previewCard.SetCard(_card);
        Vector3 point = Camera.main.WorldToScreenPoint(_card.transform.position);
        point = new Vector3(Mathf.Clamp(point.x, bounds.x, bounds.z), Mathf.Clamp(point.y, bounds.y, bounds.w));
        previewCard.transform.position = point;
        if(routine != null)
        {
            StopCoroutine(routine);
        }
        routine = PreviewAnimation();
        StartCoroutine(routine);
    }
    public void RemovePreview(CardScript _card)
    {
        if(previewedCard == _card)
        {
            previewedCard = null;
        }
        if (routine != null)
            StopCoroutine(routine);
        previewCard.transform.localScale = Vector3.zero;
    }


    public IEnumerator PreviewAnimation()
    {
        previewCard.transform.localScale = Vector3.zero;
        float scale = 0;
        while(scale < 1f)
        {
            scale += previewSpeed * Time.deltaTime;
            previewCard.transform.localScale = Vector3.one * scale;
            yield return 0;
        }
        previewCard.transform.localScale = Vector3.one;
        yield return 0;
    }
}
