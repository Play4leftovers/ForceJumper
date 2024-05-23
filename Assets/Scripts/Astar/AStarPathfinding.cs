using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    // The main script used by the AI to travel between points in the grid established in Grid.cs. It will calculate the fastest path between two grid tiles,
    // which will then be used by the AI to move through all the elements in a generated list (currentPath) until it reaches the targetNode.

    public List<Node> currentPath = new List<Node>();

    public static List<Node> FindPath(GameObject startPos, GameObject targetPos, Grid grid)
    {
        // Store the start position and the desired end position.

        Node startNode = startPos.GetComponent<Node>();
        Node targetNode = targetPos.GetComponent<Node>();

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        // As long as the open set has elements in it, we will calculate the cost 
        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];

            // Explore all the nodes in openSet and compare the fCost. We start from 1 in the index, since 0 is the startNode.
            for (int i = 1; i < openSet.Count; i++)
            {
                // If the node explored in openSet has a lower fCost, we set the currentNode to be equal to that.
                if (openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            // Set the node as explored. We do that by removing it from the openSet (nodes left to explore) and add it to the list of nodes traversed (closedSet).
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // Check - have we reached our target position? In that case, we have our path ready, and we return it.
            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            // Check and compare the cost to move to each neighbor.
            foreach (Node neighbor in currentNode.GetNeighbors())
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                // If the new cost is lesser (more attractive), or if we haven't added the neighbors to the openSet yet, we update the cost for the neighbors.
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.Parent = currentNode;

                    // If the OpenSet doesn't contain neighbor, we add it to the openSet so that we can explore them.
                    // This makes the pathfinding "branch out" into the grid.
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        // Invalid path!!! We return an empty list of nodes.
        return new List<Node>();
    }

    // We go from the end back to the start position, then reverse it to get our path. We then set the currentPath in Grid to be equal to it.
    // The AI will move along this path from Grid.cs.
    static List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        Grid.Instance.currentPath = path;
        return path;
    }

    // Calculate distance between two nodes. Used in pathfinding.
    static int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return Mathf.Max(distX, distY);
    }
}
