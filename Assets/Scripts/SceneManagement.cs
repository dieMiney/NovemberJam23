using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public void LoadScene(int sceneID)
    {
        SceneManager.LoadSceneAsync(sceneID);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
