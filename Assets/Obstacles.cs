using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacles : MonoBehaviour
{
    private List<Obstacle> children;

    public List<Obstacle> Children { get { return children; } }

    // Start is called before the first frame update
    void Start()
    {
        children = new List<Obstacle>();
        foreach (Transform child in transform)
        {
            children.Add(child.GetComponent<Obstacle>());
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
