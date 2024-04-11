using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Vector3 worldPosition;

    public bool walkable;
    public bool occupied;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;
    public int fCost => gCost + hCost;

    public Node parent;

    public GameObject occupant;

    private GameObject _gameObject;

    public void Awake()
    {
        if (Grid.Instance != null)
            Grid.Instance.allTiles.Add(this);
    }

    // Returns a list of the node's neighbors
    public List<Node> GetNeighbors()
    {
        List<Node> neighbors = new List<Node>();

        // Right
        if (gridX + 1 < Grid.Instance.gridSizeX)
        {
            AddNeighbor(gridX + 1, gridY, neighbors);
        }
        // Left
        if (gridX - 1 >= 0)
        {
            AddNeighbor(gridX - 1, gridY, neighbors);
        }
        // Up
        if (gridY + 1 < Grid.Instance.gridSizeY)
        {
            AddNeighbor(gridX, gridY + 1, neighbors);
        }
        // Down
        if (gridY - 1 >= 0)
        {
            AddNeighbor(gridX, gridY - 1, neighbors);
        }

        return neighbors;
    }

    // Once an offset is applied, this method will move from the node and add whatever node it finds at the specified offset (for example, gridX, gridY + 1) to add the node 1 space)
    private void AddNeighbor(int x, int y, List<Node> neighbors)
    {
        if (x >= 0 && x < Grid.Instance.gridSizeX && y >= 0 && y < Grid.Instance.gridSizeY)
        {
            Node neighbor = Grid.Instance.grid[x, y].GetComponent<Node>();
            if (neighbor.walkable && !neighbor.occupied && neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }
    }

    public void SetAsOccupied(GameObject newOccupant)
    {
        occupant = newOccupant;

        walkable = false;
        occupied = true;

        Grid.Instance.occupiedTiles.Add(this);

        if (Grid.Instance.unoccupiedTiles.Contains(this))
        {
            Grid.Instance.unoccupiedTiles.Remove(this);
        }
    }

    public void SetAsUnoccupied()
    {
        occupant = null;

        walkable = true;
        occupied = false;

        Grid.Instance.unoccupiedTiles.Add(this);

        if (Grid.Instance.occupiedTiles.Contains(this))
        {
            Grid.Instance.occupiedTiles.Remove(this);
        }
    }

    public GameObject GetGameObject()
    {
        return _gameObject;
    }

    public void SetGameObject(GameObject obj)
    {
        _gameObject = obj;
    }
}
