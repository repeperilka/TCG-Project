using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoreSceneController : MonoBehaviour
{
    public ConversationTextScript conversation;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return StartCoroutine(conversation.ConversationRoutine(new string[] {
                SaveGame.currentSave.saveName,
                "Te necesito",
                "No hay tiempo para explicaciones",
                "Álzate"
                }));
        yield return new WaitForSeconds(2f);
        GameController.Instance.sceneController.LoadScene(1);
    }
}
