using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Node : MonoBehaviour
{
    public int NodeRadius;
    public TMP_Text CostField;
    public float CostSoFar;
    public bool isStartNode, isEndNode;
    public Node startNodeObj; 
    public List<Connection> connection = new List<Connection>();


    private void Awake()
    {
        if (isStartNode)
            CostSoFar = 0;
        else
            CostSoFar = Mathf.Infinity;

        if (CostField != null)
        {
            CostField.text = "Cost: " + CostSoFar.ToString();
        }
    }

    void OnMouseDown()
    {
        foreach (var n in Dijkstra.EveryNode)
        {
            n.isStartNode = false;
        }
        isStartNode = true;
    }
}
[System.Serializable]
public class Connection
{
    public Node toNode;
    public float cost;
}
