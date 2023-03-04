using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeckMatch : MonoBehaviour
{
    public bool initialized = false;
    public List<CardInstance> playerDeck;

    #region Singleton
    public static PlayerDeckMatch Instance { get; private set; }
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

    }
    IEnumerator GetDeck()
    {
        while(SaveGame.currentSave.playerDeck == null)
        {
            yield return 0;
        }
        playerDeck = new List<CardInstance>(SaveGame.currentSave.playerDeck);
        initialized = true;
        Shuffle();
    }
    public void Shuffle()
    {
        for (int i = 0; i < playerDeck.Count; i++)
        {
            CardInstance temp = playerDeck[i];
            int randomIndex = Random.Range(i, playerDeck.Count);
            playerDeck[i] = playerDeck[randomIndex];
            playerDeck[randomIndex] = temp;
        }
    }
    public bool Draw(int _amount)
    {
        for(int i = 0; i < _amount; i++)
        {
            Draw();
        }
        MatchController.Instance.PlayAudio(MatchAudio.Draw);
        return true;
    }
    bool Draw()
    {
        if (playerDeck.Count == 0)
            return false;
        PlayerHand.Instance.AddCard(GameController.Instance.InstantiateCard(playerDeck[0], transform.position, 0));
        playerDeck.RemoveAt(0);
        return true;
    }
}
