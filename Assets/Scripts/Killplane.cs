using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killplane : MonoBehaviour
{
    public GameManager GameManager;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(other.gameObject);
        GameManager.RespawnPlayer(GameManager.CurrentCheckpoint.SpawnPosition.position, GameManager.CurrentCheckpoint.SpawnPosition.rotation);
    }
}
