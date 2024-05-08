using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Quaternion = System.Numerics.Quaternion;

public class GameManager : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public Transform CheckpointsParent;
    public List<Checkpoint> Checkpoints;
    public Checkpoint CurrentCheckpoint;
    
    // Start is called before the first frame update
    void Start()
    {
        Checkpoints = CheckpointsParent.GetComponentsInChildren<Checkpoint>().ToList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitPlayer()
    {
        //TODO: reset variables here
    }

    public void RespawnPlayer(Vector3 SpawnPosition, UnityEngine.Quaternion SpawnRotation)
    {
        Instantiate(PlayerPrefab, SpawnPosition, SpawnRotation);
        InitPlayer();
    }
}
