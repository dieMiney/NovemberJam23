using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighscoreTMP : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    private void Awake()
    {
        GetComponent<TMPro.TextMeshProUGUI>().text = PlayerPrefs.GetFloat("HighScore", 0).ToString("0");
    }
}
