using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public bool keepContinuing;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(keepContinuing)
            MatchController.Instance.SetAction(new PhaseAction(TurnAction.Continue, null, null), 1);
    }
}
