using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Dijkstra : MonoBehaviour
{
    public bool calculatedDistance;
    List<Node> OpenList = new List<Node>();
    List<Node> ClosedList = new List<Node>();
    List<Node> AllNodes = new List<Node>();
    List<Node> Path = new List<Node>();
    public Transform NodesContainer;
    public static List<Node> EveryNode = new List<Node>(); 
    private Node lowestCostNode;
    public Node StartNode, EndNode;

    void AddNodesFromTransformToAllNodesList(Transform trans)
    {
        foreach (Transform child in trans)
        {
            AllNodes.Add(child.GetComponent<Node>());
        }
    }

    private void Awake()
    {
        /*
         * 1)Dijkstra works in iterations.At each iteration it considers one node of the graph and
         * follows its outgoing connections, storing the node at the other end of those connections in
         * a pending list.
         * 
         * When the algorithm begins only the start node is placed in this list, so at the
         * first iteration it considers the start node. At successive iterations it chooses a node from the
         * list using an algorithm I’ll describe shortly. I’ll call each iteration’s node the “current node.”
         * When the current node is the goal, the algorithm is done. If the list is ever emptied, we know
         * that goal cannot be reached.
         * 
         * 
         * At each iteration, the algorithm chooses the node from the open list that has the smallest
         * cost-so-far. This is then processed in the normal way. The processed node is then removed
         * from the open list and placed on the closed list.
         * 
         * The basic Dijkstra algorithm terminates when the open list is empty: it has considered every
         * node in the graph that be reached from the start node, and they are all on the closed list.
         * 
         * There is one complication. When we follow a connection from the current node, we’ve
         * assumed that we’ll end up at an unvisited node. We may instead end up at a node that is either
         * open or closed, and we’ll have to deal slightly differently with them.
        */
        foreach (Transform child in NodesContainer)
        {
            EveryNode.Add(child.GetComponent<Node>());
        }

        StartCoroutine(ArrangeAllNodesByLowestCost());
        AddNodesFromTransformToAllNodesList(NodesContainer);
        FindStartAndEndNodes();
    }

    IEnumerator ArrangeAllNodesByLowestCost()
    {
        yield return new WaitForEndOfFrame();
        
        //Choosing a node with lowest connection cost and add it to the open list
        lowestCostNode = AllNodes.OrderBy(c => c.CostSoFar).ToList().First();
        OpenList.Add(lowestCostNode);
    }

    private void Update()
    {
        StartCoroutine(ClearAllLineRenderers());
        AllNodes.Clear();
        ClosedList.Clear();
        OpenList.Clear();
        Path.Clear();

        FindStartAndEndNodes();
        AddNodesFromTransformToAllNodesList(NodesContainer);
        CalculateNodeConnectionsByDistance();
        StartCoroutine(ArrangeAllNodesByLowestCost());

        //workout costs

        StartCoroutine(WorkoutPath());

        if (EndNode.startNodeObj == null) return;
        
        StartCoroutine(RetrievePath());

        StartCoroutine(DisplayPath());
    }
    void CalculateNodeConnectionsByDistance()
    {
        //TODO add raycasts
        //Make a connection to every node
        for (int i = 0; i < AllNodes.Count; i++)
        {
            var connection = new Connection();
            var currentNodeInLoop = AllNodes[i].GetComponent<Node>();
            connection.toNode = currentNodeInLoop;
            connection.cost = Vector3.Distance(currentNodeInLoop.transform.position, connection.toNode.transform.position);
            //continue working out distance between nodes and costs here, rather than in the Connectionsbydistance script
            //exit if a connection's toNode already contains the currentNodeInLoop
            if (currentNodeInLoop == AllNodes[i]) continue;
            AllNodes[i].connection.Add(connection);
        }
        foreach (var node in AllNodes)
        {
            //if(Vector3.Distance(node.transform.position, ))
        }
    }
    IEnumerator ClearAllLineRenderers()
    {
        yield return new WaitForEndOfFrame();
        AllNodes.Clear();
        AddNodesFromTransformToAllNodesList(NodesContainer);
        foreach (Node n in AllNodes)
        {
            var lr = n.GetComponent<LineRenderer>();
            if (lr)
            {
                lr.SetPosition(0, n.transform.position);
                lr.SetPosition(1, n.transform.position);
            }
        }
    }

    void FindStartAndEndNodes()
    {
        foreach (Node n in AllNodes)
        {
            if (n.isStartNode) StartNode = n;
            if (n.isEndNode) EndNode = n;
        }
    }
    IEnumerator RetrievePath()
    {
        yield return new WaitForEndOfFrame();
        //Start off looking at the End Node and work our way back
        Node currentNode = EndNode;
        //Add the end node
        Path.Add(currentNode);
        //Work backwards, adding the start node of each connection
        //skip node where the path stops (if there is no connection or if its the start node)
        while (currentNode != StartNode && currentNode != null && currentNode.startNodeObj != null)
        {
            currentNode = currentNode.startNodeObj;
            Path.Add(currentNode);
        }
        //Reverse the list so it can be used from start to finish, rather than finish to start
        Path.Reverse();
    }

    IEnumerator DisplayPath()
    {
        yield return new WaitForEndOfFrame();
        foreach (var n in Path)
        {
            var m = n.GetComponent<MeshRenderer>().material = Resources.Load("Green") as Material;
            //If there is a start node, add the path line
            if (n.startNodeObj)
            {
                var lr = n.gameObject.GetComponent<LineRenderer>();
                if(lr == null) lr = n.gameObject.AddComponent<LineRenderer>();
                lr.SetPosition(0, n.transform.position);
                lr.SetPosition(1, n.startNodeObj.transform.position);
                lr.startWidth = 0.25f;
                lr.endWidth = 0.25f;
                lr.material = m;
            }
        }
    }

    IEnumerator WorkoutPath()
    {
        yield return new WaitForEndOfFrame();
        while(OpenList.Count > 0)
        {
            //to node isnt contained inside the closed list
            //process node
            var currentNodeConnectionsByDistance = OpenList[0].connection.Where(n => !ClosedList.Contains(n.toNode)).OrderBy(c => c.cost).ToList();
            foreach (var connection in currentNodeConnectionsByDistance)
            {
                //Work out cost
                float costSoFar = OpenList[0].CostSoFar + connection.cost;
                //If greater, then skip this iteration of the loop
                if (costSoFar < connection.toNode.CostSoFar)
                {
                    //Update Start Node
                    connection.toNode.startNodeObj = OpenList[0];
                    //Workout Node Costs
                    connection.toNode.CostSoFar = OpenList[0].CostSoFar + connection.cost;

                    if (connection.toNode.CostField != null)
                        connection.toNode.CostField.text = "Cost: " + connection.toNode.CostSoFar.ToString();
                }
            }
            //take node from open list and add to closed list
            OpenList.Remove(lowestCostNode);
            AllNodes.Remove(lowestCostNode);
            ClosedList.Add(lowestCostNode);
            //Add the new lowest cost node into the open list
            lowestCostNode = AllNodes.OrderBy(c => c.CostSoFar).ToList().First();
            if (lowestCostNode == EndNode) yield break;
            OpenList.Add(lowestCostNode);
        }
    }
}
