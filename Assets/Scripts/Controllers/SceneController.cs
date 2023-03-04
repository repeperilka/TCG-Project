using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene _scene, LoadSceneMode _sceneMode)
    {
        switch (_scene.buildIndex)
        {
            case 0:
                break;
            case 1:
                StartCoroutine(OverworldController.Instance.StartScene());
                break;
            default:
                break;
        }
    }
    public void LoadScene(int _sceneIndex)
    {
        SceneManager.LoadScene(_sceneIndex);
    }
    public void LoadSceneAsync(int _sceneIndex)
    {
        SceneManager.LoadSceneAsync(_sceneIndex, LoadSceneMode.Single);
    }

}
