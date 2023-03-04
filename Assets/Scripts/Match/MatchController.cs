using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class MatchController : MonoBehaviour
{
    public PlayerGraveyard graveyard;
    public List<StackCardScript> stack = new List<StackCardScript>();

    PhaseAction[] actions = new PhaseAction[] { null, null };
    public List<CardInstance> capturedCards;
    public int earnedGold;

    public AudioClip musicClip;
    public AudioClip encounterClip;
    public AudioSource source1;
    public AudioSource source2;
    public AudioSource source3;
    public AudioSource musicSource;

    public AudioClip[] effectClips;

    public int turnOwner;
    public int turnCount;

    public Transform turnIndicator;

    public Light2D transitionLight;
    public Image transitionImage;
    public Image transitionMove;
    public Image transitionGraphic;
    public float frameTime;
    public Sprite[] transitionAnim;

    IEnumerator gameCycleRoutine;

    [Header("End Match Panels")]
    public GameObject victoryPanel;
    public Transform victoryCardHolder;

    public GameObject defeatPanel;
    public Transform defeatCardHolder;

    public Image transitionOverworld;

    public bool continueToOverworld;

    #region Singleton
    public static MatchController Instance { get; private set; }
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
        capturedCards = new List<CardInstance>();
        PlayerDeckMatch.Instance.playerDeck = new List<CardInstance>(SaveGame.currentSave.playerDeck);
        PlayerDeckMatch.Instance.Shuffle();
        StartCoroutine(StartMatchAnimation());
    }
    public IEnumerator StartMatchAnimation()
    {
        encounterClip = Resources.Load<AudioClip>("Audio/encounter_thumb");
        source1.clip = encounterClip;
        source1.Play();

        transitionImage.sprite = GameController.Instance.randomEncounterCapture;
        transitionMove.gameObject.SetActive(false);
        transitionMove.sprite = GameController.Instance.randomEncounterCapture;
        yield return new WaitForSeconds(0.1f);
        transitionImage.sprite = null;
        yield return new WaitForSeconds(0.1f);
        transitionImage.sprite = GameController.Instance.randomEncounterCapture;

        transitionMove.gameObject.SetActive(true);
        float transitionMoveScale = 1f;
        float transitionMoveSpeed = .2f;
        while(transitionMoveScale < 1.2f)
        {
            transitionMoveScale = transitionMoveScale + transitionMoveSpeed * Time.deltaTime;
            transitionMove.transform.localScale = Vector3.one * transitionMoveScale;
            yield return 0;
        }


        for(int i = 0; i < transitionAnim.Length; i++)
        {
            if (i == 5)
            {
                transitionImage.gameObject.SetActive(false);
                transitionMove.gameObject.SetActive(false);
            }
            transitionGraphic.sprite = transitionAnim[i];
            yield return new WaitForSeconds(frameTime);
        }

        float lightSpeed = 2f;
        while(transitionLight.intensity < .8f - Time.deltaTime * lightSpeed)
        {
            transitionLight.intensity = Mathf.Lerp(transitionLight.intensity, 0.8f, Time.deltaTime * lightSpeed);
            yield return 0;
        }
        transitionLight.intensity = .8f;


        gameCycleRoutine = GameCycle();
        StartCoroutine(gameCycleRoutine);
    }
    public IEnumerator GameCycle()
    {
        PlayerDeckMatch.Instance.Draw(7);
        while (SaveGame.currentSave.playerHP[0] > 0 && 
            !(BoardController.Instance.EmptySlotsCount(new SlotType[]{ SlotType.EnemyBackSlot, SlotType.EnemyFrontSlot }) == 10 && turnCount >= GameController.Instance.turnCount))
        {
            while (stack.Count != 0)
            {
                //wait for cards to be positioned in the stack
                float allDistances = 0;
                do
                {
                    allDistances = 0;
                    foreach(StackCardScript stackCard in stack)
                    {
                        allDistances += Vector3.Distance(stackCard.positionTarget, stackCard.transform.position);
                    }
                    yield return 0;
                }
                while (allDistances > .4f);


                //targets
                stack[0].SetSelected(true);
                PlayAudio(MatchAudio.SelectStack);
                if (stack[0].isTrap != TrapStatus.Activated && stack[0].effect.targets.Count == 0)
                {
                    string[] targets = stack[0].effect.targetCommands;
                    stack[0].targetLines.SetTargetAmount(targets.Length);
                    bool cancel = false;
                    for (int i = 0; i < targets.Length; i++)
                    {
                        if (cancel)
                            break;
                        string[] splitTarget = targets[i].Split(" ");
                        int amount = int.Parse(splitTarget[0]);
                        switch (splitTarget[1])
                        {
                            case "slot": //0 = amount, 1 = slot, 2 = slottype, 3 = slotstatus, 4 = doesSlotHaveAtrapAlready, 
                                SlotType slotType = (SlotType)System.Enum.Parse(typeof(SlotType), splitTarget[2]);
                                SlotStatus slotstatus = (SlotStatus)System.Enum.Parse(typeof(SlotStatus), splitTarget[3]);
                                SlotStatus hasTrap = (SlotStatus)System.Enum.Parse(typeof(SlotStatus), splitTarget[4]);
                                List<CreatureFilter> filters = new List<CreatureFilter>();
                                if (splitTarget.Length > 5)
                                {
                                    for (int r = 5; r < splitTarget.Length; r += 3)
                                    {
                                        filters.Add(new CreatureFilter(splitTarget[r], splitTarget[r + 1], splitTarget[r + 2]));
                                    }
                                }
                                for (int e = 0; e < amount; e++)
                                {
                                    yield return StartCoroutine(BoardController.Instance.SelectSlot(slotType, slotstatus, hasTrap, filters.ToArray()));
                                    if (BoardController.Instance.selectedSlot != null)
                                    {
                                        stack[0].effect.targets.Add(BoardController.Instance.selectedSlot);
                                        stack[0].targetLines.SetTarget(i, BoardController.Instance.selectedSlot.transform.position);
                                    }
                                    else
                                    {
                                        cancel = true;
                                        break;
                                    }
                                }
                                break;
                        }
                    }

                    //if target selection cancelled
                    if (cancel)
                    {
                        stack[0].targetLines.ResetTargets();
                        if (!stack[0].isHability)
                        {
                            PlayerHand.Instance.AddCard(stack[0].givenCard);
                            BoardController.Instance.ReplenishCrystals(stack[0].givenCard.cardCost);
                        }
                        actions = new PhaseAction[] { null, null };
                        RemoveStackCard(0, true);
                        continue;
                    }
                }
                else
                {
                    List<Vector3> targets = new List<Vector3>();
                    for(int i = 0; i < stack[0].effect.targets.Count; i++)
                    {
                        try
                        {
                            SlotScript target = (SlotScript)stack[0].effect.targets[i];
                            targets.Add(target.transform.position);
                        }
                        catch
                        {
                            try
                            {
                                CardScript card = (CardScript)stack[0].effect.targets[i];
                                targets.Add(card.transform.position);
                            }
                            catch
                            {

                            }
                        }
                    }
                    stack[0].targetLines.SetTargets(targets.ToArray());
                }



                //response
                int priority = 1 - stack[0].givenCard.owner;
                yield return StartCoroutine(AskForAction(priority, TurnAction.ResponseAction));
                switch (actions[priority].action)
                {
                    case TurnAction.Continue:
                        actions = new PhaseAction[2];
                        break;
                    case TurnAction.PlayHability:
                        yield return StartCoroutine(PlayHability(actions[priority].cardToPlay, actions[priority].habilityToPlay));
                        actions = new PhaseAction[2];
                        continue;
                }
                priority = 1 - priority;
                yield return StartCoroutine(AskForAction(priority, TurnAction.ResponseAction));
                switch (actions[priority].action)
                {
                    case TurnAction.Continue:
                        actions = new PhaseAction[2];
                        break;
                    case TurnAction.PlayHability:
                        yield return StartCoroutine(PlayHability(actions[priority].cardToPlay, actions[priority].habilityToPlay));
                        actions = new PhaseAction[2];
                        continue;
                }


                //execute
                stack[0].SetSelected(false);
                stack[0].targetLines.ResetTargets();
                yield return StartCoroutine(stack[0].effect.Execute(stack[0].givenCard, stack[0]));

                //cleanUp
                actions = new PhaseAction[] { null, null };
                RemoveStackCard(0, stack[0].isTrap == TrapStatus.None || stack[0].isTrap == TrapStatus.Activated);
                yield return 0;
            }

            actions = new PhaseAction[] { null, null };
            yield return StartCoroutine(AskForAction(turnOwner, TurnAction.TurnAction));
            if (stack.Count != 0)
                continue;
            switch (actions[turnOwner].action)
            {
                case TurnAction.Continue:
                    yield return StartCoroutine(Combat());
                    yield return StartCoroutine(NextTurn());
                    break;
                case TurnAction.PlayCreature:
                    yield return StartCoroutine(PlayCreature((CreatureScript)actions[turnOwner].cardToPlay));
                    break;
                case TurnAction.PlaySpell:
                    yield return StartCoroutine(PlaySpell((SpellScript)actions[turnOwner].cardToPlay));
                    break;
                case TurnAction.PlayHability:
                    yield return StartCoroutine(PlayHability(actions[turnOwner].cardToPlay, actions[turnOwner].habilityToPlay));
                    break;
            }
            actions = new PhaseAction[] { null, null };
            yield return 0;
        }
        if(SaveGame.currentSave.playerHP[0] <= 0)
        {
            StartCoroutine(Defeat());
        }
        else
        {
            StartCoroutine(Victory());
        }
    }
    
    public void ContinueToOverworld()
    {
        continueToOverworld = true;
    }
    public IEnumerator Victory()
    {
        Debug.Log("Victory");
        for (int i = 0; i < capturedCards.Count; i++)
        {
            SaveGame.AddCardToInventory(capturedCards[i]);
        }
        SaveGame.currentSave.gold += earnedGold;
        yield return StartCoroutine(EndMatch(victoryPanel, victoryCardHolder));
    }
    public IEnumerator Defeat()
    {
        Debug.Log("Defeat");
        if(SaveGame.currentSave.gold < 100)
        {
            SaveGame.currentSave.gold = 0;
        }
        else
        {
            SaveGame.currentSave.gold -= 100;
        }
        yield return StartCoroutine(EndMatch(defeatPanel, defeatCardHolder));
    }
    public IEnumerator EndMatch(GameObject _panel, Transform _cardsParent)
    {
        _panel.gameObject.SetActive(true);
        BoardController.Instance.playerInteraction.enabled = false;
        CardPreview.Instance.RemovePreview(CardPreview.Instance.previewedCard);

        for(int i = 0; i < capturedCards.Count; i++)
        {
            GameController.Instance.InstantiateCardUI(capturedCards[i], _cardsParent);
            yield return new WaitForSeconds(0.2f);
        }
        SaveGame.Save();
        yield return 0;

        continueToOverworld = false;
        while (!continueToOverworld)
        {
            yield return 0;
        }

        while(transitionOverworld.color.a < 1f)
        {
            transitionOverworld.color = new Color(0f, 0f, 0f, transitionOverworld.color.a + Time.deltaTime);
            yield return 0;
        }

        GameController.Instance.sceneController.LoadScene(1);
    }
    public void Surrend()
    {
        if (gameCycleRoutine != null)
            StopCoroutine(gameCycleRoutine);
        StartCoroutine(Defeat());
    }
    public IEnumerator Combat()
    {
        List<SlotScript> attackerSlots = BoardController.Instance.playerSlots;
        List<SlotScript> defender = BoardController.Instance.enemyFrontSlots;
        if (turnOwner == 1)
        {
            attackerSlots = BoardController.Instance.enemyFrontSlots;
            defender = BoardController.Instance.playerSlots;
        }

        for(int i = 0; i < attackerSlots.Count; i++)
        {
            if (attackerSlots[i].heldCard == null)
                continue;
            yield return StartCoroutine(AttackAnimation(attackerSlots[i], defender[i]));
        }


        yield return 0;
    }
    public IEnumerator DestroyCreature(SlotScript _slot)
    {
        if(_slot.owner == SlotType.EnemyFrontSlot || _slot.owner == SlotType.EnemyBackSlot)
            StartCoroutine(AddMoney(Mathf.RoundToInt(_slot.heldCard.cardClass.price * .4f)));
        PlayAudio(MatchAudio.DieCreature);
        yield return StartCoroutine(_slot.heldCard.MoveCard(new Vector3(_slot.transform.position.x + .5f, _slot.transform.position.y, _slot.transform.position.z), 20f, "Board", 1));
        yield return StartCoroutine(_slot.heldCard.MoveCard(new Vector3(_slot.transform.position.x - .5f, _slot.transform.position.y, _slot.transform.position.z), 20f, "Board", 1));
        yield return StartCoroutine(_slot.heldCard.MoveCard(new Vector3(_slot.transform.position.x + .5f, _slot.transform.position.y, _slot.transform.position.z), 20f, "Board", 1));
        yield return StartCoroutine(_slot.heldCard.MoveCard(new Vector3(_slot.transform.position.x - .5f, _slot.transform.position.y, _slot.transform.position.z), 20f, "Board", 1));
        yield return StartCoroutine(_slot.heldCard.MoveCard(new Vector3(_slot.transform.position.x, _slot.transform.position.y, _slot.transform.position.z), 20f, "Board", 1));
        CardScript creatureDied = _slot.RemoveCard();
        if (_slot.owner == 0)
        {
            StartCoroutine(graveyard.AddCard(creatureDied));
        }
        else
        {
            StartCoroutine(DisposeCard(creatureDied));
        }

        //get triggers
        GetTriggers("OnThisExit", creatureDied);
        SlotScript[] thisSlots = BoardController.Instance.GetSlots((SlotType)_slot.owner, SlotStatus.Full);
        foreach (SlotScript slot in thisSlots)
        {
            GetTriggers("OnPlayerCreatureExit", slot.heldCard);
        }
        SlotScript[] enemySlots = BoardController.Instance.GetSlots((SlotType)(1f - (int)_slot.owner), SlotStatus.Full);
        foreach (SlotScript slot in enemySlots)
        {
            GetTriggers("OnEnemyCreatureExit", slot.heldCard);
        }
        yield return 0;
    }
    public IEnumerator AddMoney(int _amount)
    {
        earnedGold += _amount;
        BoardController.Instance.UpdateGold();

        yield return 0;
    }
    public IEnumerator AttackAnimation(SlotScript _attackerSlot, SlotScript _defenderSlot)
    {
        if (_attackerSlot.heldCard.owner == 0 && _defenderSlot.heldCard == null)
            yield break;

        float current = _attackerSlot.transform.position.y;
        float back = -1f;
        float front = 2f;
        if(_attackerSlot.heldCard.owner == 1)
        {
            back *= -1;
            front *= -1;
        }

        bool killDefender = false;
        PlayAudio(MatchAudio.Attack);
        yield return StartCoroutine(_attackerSlot.heldCard.MoveCard(new Vector3(_attackerSlot.transform.position.x, current + back, _attackerSlot.transform.position.z), 6f, "Board", 10));
        yield return StartCoroutine(_attackerSlot.heldCard.MoveCard(new Vector3(_attackerSlot.transform.position.x, current + front, _attackerSlot.transform.position.z), 20f, "Board", 10));
        if (_defenderSlot.heldCard != null)
        {
            killDefender = ((CreatureScript)_defenderSlot.heldCard).DealDamage(((CreatureScript)_attackerSlot.heldCard).attack);
        }
        else
        {
            BoardController.Instance.DealDamageToPlayer(((CreatureScript)_attackerSlot.heldCard).attack);
        }
        yield return StartCoroutine(_attackerSlot.heldCard.MoveCard(new Vector3(_attackerSlot.heldCard.transform.position.x, current, _attackerSlot.transform.position.z - 0.2f), 8f, "Board", 0));

        if (killDefender)
        {
            yield return StartCoroutine(DestroyCreature(_defenderSlot));
        }
    }
    public bool CanPlay(CardScript _card, bool _debug = true)
    {
        if (actions[_card.owner] == null)
        {
            if(_debug)
                Debug.Log("CanPlay(" + _card.cardClass.cardName + "): Not your priority");
            return false;
        }
        else if(actions[_card.owner].possibleActions == TurnAction.ResponseAction)
        {
            if (_debug)
                Debug.Log("CanPlay(" + _card.cardClass.cardName + "): Can't play cards on response");
            return false;
        }
        if (BoardController.Instance.playerCrystals[0] < _card.cardCost)
        {
            if (_debug)
                Debug.Log("CanPlay(" + _card.cardClass.cardName + "): Not enough crystals");
            return false;
        }
        switch (_card.cardType)
        {
            case CardType.Spell:
                Hability hab = _card.cardClass.habilities[0];
                if (!CanPlayHability(hab, _card, _debug))
                {
                    return false;
                }
                break;
            case CardType.Creature:
                if (BoardController.Instance.EmptySlotsCount((SlotType)_card.owner) == 0)
                {
                    if (_debug)
                        Debug.Log("CanPlay(" + _card.cardClass.cardName + "): No empty slots");
                    return false;
                }
                break;
            default:
                if (_debug)
                    Debug.Log("CanPlay(" + _card.cardClass.cardName + "): Unknown card type");
                return false;
        }
        if (_debug)
            Debug.Log("CanPlay(" + _card.cardClass.cardName + "): Card can be played");
        return true;
    }
    public bool CanPlayHability(Hability _hability, CardScript _owner, bool _debug = true)
    {

        if(actions[_owner.owner] == null)
        {
            if (_debug)
                Debug.Log("CanPlayHability(" + _owner.cardClass.cardName + "): Not your priority");
            return false;
        }

        if (BoardController.Instance.playerCrystals[0] < _hability.cost)
        {
            if (_debug)
                Debug.Log("CanPlayHability(" + _owner.cardClass.cardName + "): Not enough crystals");
            return false;
        }

        Effect newEffect = CardDictionary.GetEffect(_hability.effectID);
        string[] _targets = newEffect.targetCommands;
        for(int i = 0; i < _targets.Length; i++)
        {
            string[] splitTarget = _targets[i].Split();
            int amount = int.Parse(splitTarget[0]);
            switch (splitTarget[1])
            {
                case "slot": //0 = amount, 1 = slot, 2 = slottype, 3 = slotstatus, 4 = doesslothavetrapalready, (5, 6, 7...) filters
                    SlotType slotType = (SlotType)System.Enum.Parse(typeof(SlotType), splitTarget[2]);
                    SlotStatus slotstatus = (SlotStatus)System.Enum.Parse(typeof(SlotStatus), splitTarget[3]);
                    SlotStatus hasTrap = (SlotStatus)System.Enum.Parse(typeof(SlotStatus), splitTarget[4]);
                    List<CreatureFilter> filters = new List<CreatureFilter>();

                    if(splitTarget.Length > 5)
                    {
                        for(int r = 5; r < splitTarget.Length; r+=3)
                        {
                            filters.Add(new CreatureFilter(splitTarget[r], splitTarget[r + 1], splitTarget[r + 2]));
                        }
                    }

                    int slotsAmount = BoardController.Instance.SlotsCount(slotType, slotstatus, hasTrap, filters.ToArray());
                    if (slotsAmount < amount)
                    {
                        if (_debug)
                            Debug.Log("CanPlayHability(" + _owner.cardClass.cardName + "): Not enough required slots: " + amount.ToString() + ", " + slotType.ToString() + ", " + slotstatus.ToString() + ", " + hasTrap.ToString() + " = " + slotsAmount.ToString());

                        return false;
                    }
                    break;
            }
        }
        if (_debug)
            Debug.Log("CanPlayHability(" + _owner.cardClass.cardName + "): Hability can be played");
        return true;
    }
    public IEnumerator PlayCreature(CreatureScript _card)
    {
        StartCoroutine(_card.MoveCard(new Vector3(0, 3f, 0), 3f, 30f, "Overlay", 0));

        _card.SetSelected(true);
        _card.SetInteractable(false);
        yield return StartCoroutine(BoardController.Instance.SelectSlot((SlotType)_card.owner, SlotStatus.Empty, SlotStatus.Any, new CreatureFilter[0]));
        if(BoardController.Instance.selectedSlot == null)
        {
            _card.SetSelected(false);
            _card.SetInteractable(true);
            PlayerHand.Instance.AddCard(_card);
            yield break;
        }

        _card.SetSelected(false);
        _card.SetInteractable(false);

        BoardController.Instance.RemoveCrystals(_card.cardCost);
        PlayAudio(MatchAudio.PlaceCreature);
        yield return StartCoroutine(BoardController.Instance.selectedSlot.SetHeldCard(_card));


        //call the onThisEnter habilities
        GetTriggers("OnThisEnter", _card);
        if(_card.owner == 0)
        {
            GetTriggers("OnPlayerCreatureEnter", SlotType.PlayerSlot);
            GetTriggers("OnEnemyCreatureEnter", SlotType.EnemyFrontSlot);
        }
        else
        {
            GetTriggers("OnPlayerCreatureEnter", SlotType.EnemyFrontSlot);
            GetTriggers("OnEnemyCreatureEnter", SlotType.PlayerSlot);
        }

        yield return 0;
    }
    public IEnumerator PlaySpell(SpellScript _card)
    {
        _card.SetSelected(false);
        _card.SetInteractable(false);
        Debug.Log("Playing spell: " + _card.cardClass.cardName);
        StackCardScript stackCard = GameController.Instance.InstantiateStackCard(_card, _card.transform.position, _card.cardClass.habilities[0], false);
        stackCard.transform.localScale = _card.transform.localScale;
        AddStackCard(stackCard);
        StartCoroutine(graveyard.AddCard(_card));

        BoardController.Instance.RemoveCrystals(_card.cardCost);

        //call the onThisEnter habilities
        GetTriggers("OnThisEnter", _card);
        GetTriggers("OnSpellPlayed", SlotType.PlayerSlot);
        GetTriggers("OnSpellPlayed", SlotType.EnemyFrontSlot);

        yield return 0;
    }
    public IEnumerator PlayHability(CardScript _owner, Hability _hability)
    {
        Debug.Log("Playing hability: " + _hability.name);
        StackCardScript stackCard = GameController.Instance.InstantiateStackCard(_owner, _owner.transform.position, _hability, false);
        stackCard.transform.localScale = _owner.transform.localScale;
        AddStackCard(stackCard);

        BoardController.Instance.RemoveCrystals(_hability.cost);


        yield return 0;
    }
    public IEnumerator NextTurn()
    {
        yield return StartCoroutine(SetTurnOwner(1 - turnOwner));
        if(turnOwner == 0)
        {
            BoardController.Instance.AddCrystal(1);
            BoardController.Instance.ReplenishCrystals();
            PlayerDeckMatch.Instance.Draw(1);
        }
        else
        {
            //moves cards from back to front
            for(int i = 0; i < BoardController.Instance.enemyBackSlots.Count; i++)
            {
                if(BoardController.Instance.enemyFrontSlots[i].heldCard == null && BoardController.Instance.enemyBackSlots[i].heldCard != null)
                {
                    CardScript card = BoardController.Instance.enemyBackSlots[i].RemoveCard();
                    yield return StartCoroutine(BoardController.Instance.enemyFrontSlots[i].SetHeldCard((CreatureScript)card));
                }
            }
            //spawns cards on the back
            if(turnCount <= GameController.Instance.turnCount)
            {
                for (int i = 0; i < BoardController.Instance.enemyBackSlots.Count; i++)
                {
                    if (Random.Range(0, 1f) < .5f || BoardController.Instance.enemyBackSlots[i].heldCard != null)
                        continue;
                    CardScript newCard = null;
                    string newCardID = GameController.Instance.GetCardFromPool();
                    Debug.Log("NextTurn: Instantiating " + newCardID);
                    newCard = GameController.Instance.InstantiateCard(newCardID, BoardController.Instance.enemyBackSlots[i].transform.position, 1);
                    StartCoroutine(BoardController.Instance.enemyBackSlots[i].SetHeldCard((CreatureScript)newCard));
                }
            }
        }
        turnCount++;
        GetTriggers("OnNextTurn", SlotType.Any);
        if (turnOwner == 0)
        {
            GetTriggers("OnPlayerTurn", SlotType.PlayerSlot);
            GetTriggers("OnEnemyTurn", SlotType.EnemyFrontSlot);
        }
        else
        {
            GetTriggers("OnPlayerTurn", SlotType.EnemyFrontSlot);
            GetTriggers("OnEnemyTurn", SlotType.PlayerSlot);
        }
        yield return StartCoroutine(CheckDead());
        yield return 0;
    }
    public IEnumerator CheckDead()
    {
        SlotScript[] slots = BoardController.Instance.GetSlots(SlotType.Any, SlotStatus.Full);
        for(int i = 0; i < slots.Length; i++)
        {
            if(slots[i].heldCard.defense <= 0)
            {
                yield return StartCoroutine(DestroyCreature(slots[i]));
            }
        }
    }
    public IEnumerator AskForAction(int _owner, TurnAction _possibleActions)
    {
        //sets array of actions for the owner to respond
        actions[_owner] = new PhaseAction(_possibleActions);
        Debug.Log("AskForAction: Waiting for " + _owner.ToString() + "'s response");

        //setup interactables
        CardPreview.Instance.visualizable = true;
        if(_owner == 0)
        {
            if(_possibleActions == TurnAction.TurnAction)
            {
                PlayerHand.Instance.interactable = true;
            }
            else
            {
                PlayerHand.Instance.interactable = false;
            }

            BoardController.Instance.SetPlayerActiveSlots();
            BoardController.Instance.continueButton.transform.parent.gameObject.SetActive(true);
            BoardController.Instance.continueButton.text = _possibleActions == TurnAction.TurnAction ? "Next Turn" : "Accept";
        }
        else
        {
            PlayerHand.Instance.interactable = false;
            BoardController.Instance.SetSlotsInteractable(false, SlotType.PlayerSlot);
            BoardController.Instance.continueButton.transform.parent.gameObject.SetActive(false);
        }

        while(actions[_owner].action == TurnAction.None)
        {
            if (stack.Count != 0 && _possibleActions == TurnAction.TurnAction)
                break;

            yield return 0;
        }

        //clean up
        PlayerHand.Instance.interactable = false;
        BoardController.Instance.ResetPlayerActiveSlots();
        BoardController.Instance.continueButton.transform.parent.gameObject.SetActive(false);
        CardPreview.Instance.visualizable = false;
    }
    public void SetAction(PhaseAction _action, int _owner)
    {
        if(actions[_owner] != null)
        {
            switch (actions[_owner].possibleActions)
            {
                case TurnAction.ResponseAction:
                    if(_action.action == TurnAction.PlayHability || _action.action == TurnAction.Continue)
                    {
                        actions[_owner] = _action;
                        Debug.Log("SetAction: Player " + _owner + " set action " + _action.action.ToString());
                    }
                    break;
                case TurnAction.TurnAction:
                    actions[_owner] = _action;
                    Debug.Log("SetAction: Player " + _owner + " set action " + _action.action.ToString());
                    break;
                default:
                    Debug.Log("Current action: " + actions[_owner].action.ToString());
                    break;
            }
        }
    }
    public IEnumerator SetTurnOwner(int _owner)
    {
        turnOwner = _owner;
        if(_owner == 0)
        {
            turnIndicator.transform.position = new Vector3(0, -3f, 0);
        }
        else
        {
            turnIndicator.transform.position = new Vector3(0, 3f, 0);
        }
        yield return 0;
    }
    public IEnumerator DisposeCard(CardScript _card)
    {
        yield return StartCoroutine(_card.MoveCard(_card.transform.position, 0.01f, 30f, "Board", 10));
        CardDictionary.DisposeID(_card.cardInstance.instanceID);
        Destroy(_card.gameObject);
    }
    public void RemoveStackCard(int _index, bool _destroy)
    {
        if (_destroy)
        {
            Destroy(stack[_index].gameObject);
        }
        stack.RemoveAt(_index);
        SortStack();
    }
    public void RemoveStackCard(StackCardScript _card)
    {
        stack.Remove(_card);
        stack.Sort();
    }

    public void AddStackCard(Hability _hability, CardScript _owner, bool _isHability)
    {
        StackCardScript stackCard = GameController.Instance.InstantiateStackCard(_owner, _owner.transform.position, _hability, _isHability);
        stack.Insert(0, stackCard);
        SortStack();
    }
    public void AddStackCard(StackCardScript _card)
    {
        stack.Insert(0, _card);
        SortStack();
    }
    public void SortStack()
    {
        for(int i = 0; i < stack.Count; i++)
        {
            if(i == 0)
            {
                StartCoroutine(stack[i].MoveCard(new Vector3(7.5f, 2.5f - i * .4f, i * .01f), 2.7f, 30f, "Overlay", (stack.Count - i) * 3));
            }
            else
            {
                StartCoroutine(stack[i].MoveCard(new Vector3(7.5f, 2.5f - i * .4f, i * .01f), 2f, 30f, "Overlay", (stack.Count - i) * 3));
            }
            //stack[i].MoveCardFunction(new Vector3(8.5f, 3.5f - i * 1f, i * .01f), 2f, 20f, "Overlay", i * 3);
        }
    }
    public IEnumerator CaptureCreature(SlotScript _slot)
    {
        capturedCards.Add(_slot.heldCard.cardInstance);
        yield return DestroyCreature(_slot);
    }
    public void GetTriggers(string _trigger, CardScript _card)
    {
        List<Hability> triggers = _card.GetTriggers(_trigger);
        for (int i = 0; i < triggers.Count; i++)
        {
            AddStackCard(triggers[i], _card, true);
        }
    }
    public void GetTriggers(string _trigger, SlotType _slot)
    {
        SlotScript[] slots = BoardController.Instance.GetSlots(_slot, SlotStatus.Full, SlotStatus.Any, new CreatureFilter[0] );
        for(int i = 0; i < slots.Length; i++)
        {
            List<Hability> triggers = slots[i].heldCard.ActivateTriggers(_trigger);
            for(int e = 0; e < triggers.Count; e++)
            {
                AddStackCard(triggers[i], slots[i].heldCard, true);
            }
        }
    }

    public void PlayAudio(MatchAudio _audioClip)
    {
        PlayAudio(effectClips[(int)_audioClip]);
    }
    public void PlayAudio(AudioClip _audioClip)
    {
        if (!source1.isPlaying)
        {
            source1.clip = _audioClip;
            source1.Play();
            return;
        }
        if (!source2.isPlaying)
        {
            source2.clip = _audioClip;
            source2.Play();
            return;
        }
        if (!source3.isPlaying)
        {
            source3.clip = _audioClip;
            source3.Play();
            return;
        }
    }
}
[System.Serializable]
public enum MatchAudio
{
    Shuffle,
    Draw,
    PickCard,
    PlayCard,
    SelectStack,
    PlayStack,
    PlaceCreature,
    PlaceTrap,
    Attack,
    DieCreature
}
[System.Serializable]
public class PhaseAction
{
    public TurnAction possibleActions;
    public TurnAction action;
    public CardScript cardToPlay;
    public Hability habilityToPlay;

    public PhaseAction(TurnAction _possibleActions)
    {
        possibleActions = _possibleActions;
        action = TurnAction.None;
        cardToPlay = null;
        habilityToPlay = null;
    }
    public PhaseAction(TurnAction _action, CardScript _cardToPlay, Hability _habilityToPlay)
    {
        action = _action;
        cardToPlay = _cardToPlay;
        habilityToPlay = _habilityToPlay;
    }
}
[System.Serializable]
public enum TurnAction
{
    TurnAction,
    ResponseAction,
    None,
    PlayCreature,
    PlaySpell,
    PlayHability,
    Continue
}
[System.Serializable]
public class StackCard
{
    public CardScript owner;
    public Hability hability;
    public Effect effect;
    public bool isHability;
}
[System.Serializable]
public class CreatureFilter
{
    public int maxAttack;
    public int maxDefense;
    public int maxCost;

    public CreatureFilter(string _maxAttk, string _maxDef, string _maxCost)
    {
        maxAttack = int.Parse(_maxAttk);
        maxDefense = int.Parse(_maxDef);
        maxCost = int.Parse(_maxCost);
    }
    public bool CheckCreature(CreatureScript _creature)
    {
        if(_creature == null)
        {
            return true;
        }
        if(_creature.attack > maxAttack && maxAttack != -1)
        {
            return false;
        }
        if(_creature.defense > maxDefense && maxDefense != -1)
        {
            return false;
        }
        if(_creature.cardCost > maxCost && maxCost != -1)
        {
            return false;
        }
        return true;
    }
}
