using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [Header("Logistics")]
    public Grid gridReference;
    public bool unitIsSelected;

    public GameObject selectedUnit;

    [Header("Lists And Information")]

    public List<Unit> allUnits = new List<Unit>();
    public List<Node> allTiles = new List<Node>();
    public List<Node> unoccupiedTiles = new List<Node>();
    public List<Node> occupiedTiles = new List<Node>();

    public List<Unit> playerTeam = new List<Unit>();
    public List<Unit> enemyTeam = new List<Unit>();

    public GameObject Tile;

    public List<Node> currentPath = new List<Node>();

    public static Grid Instance { get; private set; }
    public GameObject[,] grid;
    public GameObject selectedNode;
    public GameObject TileObject;

    [Header("Node And Grid Properties")]
    public Vector2 gridSize;
    public int gridSizeX, gridSizeY;
    public float nodeDiameter;
    public float nodeSize;
    public float highestAllowedPoint = 100;

    [Header("Debug")]
    public bool VisualizeGrid;

    // Set all tiles as unoccupied by default, and establish the instance.
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            unoccupiedTiles = allTiles;
        }
    }

    // Initialize all the node properties and create the grid.
    void Start()
    {
        nodeDiameter = nodeSize * 2;
        gridSizeX = Mathf.RoundToInt(gridSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridSize.y / nodeDiameter);
        CreateGrid();

        int centerX = gridSizeX / 2;
        int centerY = gridSizeY / 2;

        if (centerX >= 0 && centerX < gridSizeX && centerY >= 0 && centerY < gridSizeY)
        {
            selectedNode = grid[centerX, centerY];
        }
    }

    void Update()
    {
        if (grid == null) return;

        // For debug purposes, set the color of the grid tiles to show how the path is being created.
        if (VisualizeGrid)
        {
            for (int i = 0; i < currentPath.Count; i++)
            {
                if (currentPath[i] != currentPath[currentPath.Count - 1])
                {
                    currentPath[i].GetComponent<MeshRenderer>().material.color = Color.yellow;
                }
                else
                {
                    currentPath[i].GetComponentInChildren<Renderer>().material.color = Color.red;
                }
            }
        }
    }

    //Selects a random tile from the current path, or if there is no path, from the entire grid.
    public void SelectRandomTile()
    {
        if (currentPath.Count > 0)
        {
            GameObject StartNode = currentPath[currentPath.Count - 1].gameObject;
            GameObject EndNode = allTiles[Random.Range(0, allTiles.Count)].gameObject;

            if (EndNode.GetComponent<Node>().walkable)
            {
                AStarPathfinding.FindPath(StartNode, EndNode, gridReference);
            }
        }
        else
        {
            GameObject StartNode = allTiles[Random.Range(0, allTiles.Count)].gameObject;
            GameObject EndNode = allTiles[Random.Range(0, allTiles.Count)].gameObject;

            if (EndNode.GetComponent<Node>().walkable)
            {
                AStarPathfinding.FindPath(StartNode, EndNode, gridReference);
            }
        }

    }

    //Remove node. This should be done with caution, it is better to use the tags "Wall" to prevent tiles from spawning rather than deleting them after the grid is set.
    public void RemoveNode(Node node, List<Node> knownNeighbors)
    {
        List<Node> neighbors = node.GetNeighbors();
        foreach (Node neighbor in neighbors)
        {
            neighbors.Remove(node);
        }

        grid[node.gridX, node.gridY] = null;
    }

    void CreateGrid()
    {
        grid = new GameObject[gridSizeX, gridSizeY];
        Vector3 gridStartPosition = transform.position - Vector3.right * gridSizeX / 2 - Vector3.forward * gridSizeY / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = gridStartPosition + Vector3.right * (x * nodeDiameter + nodeSize) + Vector3.forward * (y * nodeDiameter + nodeSize);
                bool walkable = true;

                RaycastHit hit;
                int groundLayerMask = LayerMask.GetMask("Ground");

                if (Physics.Raycast(worldPoint + Vector3.up * highestAllowedPoint, Vector3.down, out hit, Mathf.Infinity, groundLayerMask))
                {
                    worldPoint.y = hit.point.y;

                    // Rotate the tile based on the normal of the hit surface (can handle slopes)
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                    // Spawn the tile at the world point, pass in the normal of the hit surface and set its rotation to match.
                    GameObject tile = Instantiate(TileObject, worldPoint, rotation);

                    Node node = tile.GetComponent<Node>();
                    node.walkable = walkable;

                    // For debug purposes, show the grid tile and its position/rotation.
                    if (VisualizeGrid)
                    {
                        tile.GetComponent<MeshRenderer>().enabled = true;
                    }

                    if (!node.walkable)
                    {
                        tile.GetComponentInChildren<Renderer>().material.color = Color.blue;
                        tile.GetComponentInChildren<MeshRenderer>().enabled = false;
                    }

                    node.gridX = x;
                    node.gridY = y;
                    node.worldPosition = worldPoint;
                    grid[x, y] = tile;
                    SetGrid(this);
                }
                else
                {
                    // If the raycast does not hit any ground, node is unreachable/deactivated.
                    walkable = false;
                    worldPoint.y = transform.position.y;

                    // Instantiate the tile with default rotation (0).
                    GameObject tile = Instantiate(TileObject, worldPoint, Quaternion.identity);

                    Node node = tile.GetComponent<Node>();
                    node.walkable = walkable;


                    // For debug purposes, show the grid tile and its position/rotation.
                    if (VisualizeGrid)
                    {
                        tile.GetComponent<MeshRenderer>().enabled = true;
                    }

                    if (!node.walkable)
                    {
                        tile.GetComponentInChildren<Renderer>().material.color = Color.blue;
                        tile.GetComponentInChildren<MeshRenderer>().enabled = false;
                    }

                    // Set the node x-and-y position, and update the grid accordingly.
                    node.gridX = x;
                    node.gridY = y;
                    node.worldPosition = worldPoint;
                    grid[x, y] = tile;
                    SetGrid(this);
                }
            }
        }
    }
    public void SetGrid(Grid grid)
    {
        gridReference = this;
    }

    // Gets a node from world point, can be used for getting tiles in proximity of the player, or getting the nearest tile.
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = Mathf.Clamp01((worldPosition.x + gridSize.x / 2) / gridSize.x);
        float percentY = Mathf.Clamp01((worldPosition.z + gridSize.y / 2) / gridSize.y);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        x = Mathf.Clamp(x, 0, gridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSizeY - 1);

        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
        {
            return grid[x, y].GetComponent<Node>();
        }
        else
        {
            return null;
        }
    }
}
