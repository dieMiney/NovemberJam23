using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public float Score { get; private set; }
    public float HighScore { get; private set; }

    public bool isCounting = true;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        HighScore = PlayerPrefs.GetFloat("HighScore", 0);
    }

    public void IncreaseScore(float amount)
    {
        Score += amount;
    }

    /// <summary>
    /// Saves the current score if it is higher than the previous high score.
    /// </summary>
    /// <returns>True if the score was saved, false otherwise.</returns>
    public bool SaveScore()
    {
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetFloat("HighScore", HighScore);
            return true;
        }
        return false;
    }

    void Update()
    {
        if (isCounting)
            IncreaseScore(1 * Time.deltaTime);
    }
}