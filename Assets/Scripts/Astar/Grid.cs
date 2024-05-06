using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Grid gridReference;
    public bool unitIsSelected;

    public GameObject selectedUnit;

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
    public Vector2 gridSize;
    public float nodeDiameter;
    public float nodeSize;
    public int gridSizeX, gridSizeY;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            unoccupiedTiles = allTiles;
        }
    }

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
            SelectNodeGameObject(selectedNode);
        }
    }

    void Update()
    {
        if (grid == null) return;

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

                // Perform a raycast downwards to determine the height of the node
                RaycastHit hit;
                if (Physics.Raycast(worldPoint + Vector3.up * 100f, Vector3.down, out hit, Mathf.Infinity))
                {
                    if (hit.collider.CompareTag("Ground"))
                    {
                        worldPoint.y = hit.point.y;
                    }
                }
                else
                {
                    Debug.LogWarning("Raycast did not hit anything below the node. Adjusting Y position based on default height.");
                    // If the raycast does not hit anything, use a default Y position
                    worldPoint.y = transform.position.y;
                }

                Collider[] colliders = Physics.OverlapSphere(worldPoint, nodeSize);
                bool walkable = true;
                foreach (Collider col in colliders)
                {
                    if (col.CompareTag("Wall"))
                    {
                        walkable = false;
                    }

                }

                GameObject tile = Instantiate(TileObject, worldPoint, Quaternion.identity);
                Node node = tile.GetComponent<Node>();
                node.walkable = walkable;

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
        }
    }
    public void SetGrid(Grid grid)
    {
        gridReference = this;
    }

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

    void SelectNodeGameObject(GameObject node)
    {
        node.GetComponentInChildren<Renderer>().material.color = Color.green;
    }

    void DeselectNodeGameObject(GameObject node)
    {
        node.GetComponentInChildren<Renderer>().material.color = Color.yellow;
    }
}
