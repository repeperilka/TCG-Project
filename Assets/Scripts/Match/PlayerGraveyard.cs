using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGraveyard : MonoBehaviour
{
    public List<CardScript> graveyard;

    public void Start()
    {
        graveyard = new List<CardScript>();
    }

    public IEnumerator AddCard(CardScript _card)
    {
        graveyard.Add(_card);
        yield return StartCoroutine(_card.MoveCard(transform.position, 2f, 25f, "Board", 10));
    }

}
