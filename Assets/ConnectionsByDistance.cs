using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionsByDistance : MonoBehaviour
{
    Node thisNode;

    private void Start()
    {
        thisNode = transform.GetComponent<Node>();
        thisNode.connection.Clear();
    }
    private void OnTriggerEnter(Collider other)
    {
        var c = new Connection();
        c.toNode = other.GetComponent<Node>();
        if (thisNode.connection.Contains(c) == false)
        {
            thisNode.connection.Add(c);
        }
    }
    private void Update()
    {
        foreach (var c in thisNode.connection)
        {
            c.cost = Vector3.Distance(thisNode.transform.position, c.toNode.transform.position);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //foreach (Connection c in thisNode.connection)
        //{
        //    if (c.toNode == other.GetComponent<Node>())
        //        thisNode.connection.Remove(c);
        //}
        //if(thisNode.isStartNode == false)
        //    thisNode.CostSoFar = Mathf.Infinity;
        //removing the collided node from our node's connection list
        for (int i = thisNode.connection.Count - 1; i >= 0; i--)
        {
            var c = thisNode.connection[i];
            if (c.toNode == other.GetComponent<Node>())
            {
                thisNode.connection.Remove(c);
            }
        }
        //removing our node from the collided node's connection list and resetting its properties
        var otherNode = other.GetComponent<Node>();
        for (int i = otherNode.connection.Count - 1; i >= 0; i--)
        {
            if (otherNode.connection[i].toNode == thisNode)
            {
                if (thisNode.isStartNode == false)
                    thisNode.CostSoFar = Vector3.Distance(thisNode.transform.position, otherNode.transform.position);
                otherNode.connection.Remove(otherNode.connection[i]);
            }
            otherNode.startNodeObj = null;
        }
    }
}
