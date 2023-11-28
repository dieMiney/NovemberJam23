using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacles : MonoBehaviour
{
    private List<Obstacle> children;

    public List<Obstacle> Children { get { return children; } }

    // Start is called before the first frame update
    void Awake()
    {
        children = new List<Obstacle>();
        foreach (Transform child in transform)
        {
            Debug.Log(child.name);
            children.Add(child.GetComponent<Obstacle>());
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
