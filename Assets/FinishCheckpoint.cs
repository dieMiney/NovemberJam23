using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishCheckpoint : MonoBehaviour
{
    [SerializeField]
    private GameObject finalScreen;

    void Start()
    {
        finalScreen.SetActive(false);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            finalScreen.SetActive(true);
            Debug.Log("Finish");
            ScoreManager.Instance.isCounting = false;
            if (ScoreManager.Instance.SaveScore())
            {
                Debug.Log("New High Score!");
            }
            else
            {
                Debug.Log("Score: " + ScoreManager.Instance.Score);
                Debug.Log("High Score: " + ScoreManager.Instance.HighScore);
            }
        }
    }
}
