using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;

public class UnitMovement : MonoBehaviour
{
    private Unit UnitData;
    void Start()
    {
        UnitData = GetComponent<Unit>();

        SetStartPosition();
    }

    void SetStartPosition()
    {
        Node tile = Grid.Instance.unoccupiedTiles[Random.Range(0, Grid.Instance.unoccupiedTiles.Count)];

        gameObject.transform.position = tile.transform.position;

        tile.SetAsOccupied(gameObject);
    }

    public void SetNewPosition(GameObject tile)
    {
        gameObject.transform.position = tile.transform.position + new Vector3(0, 1, 0);

        tile.GetComponent<Node>().SetAsOccupied(tile);
    }
}
