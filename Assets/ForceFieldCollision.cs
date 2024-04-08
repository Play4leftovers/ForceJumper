using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceFieldCollision : MonoBehaviour
{
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
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Ground"))
        {
            Debug.Log("hit wall / ground!");
            GetComponentInParent<Rigidbody>().isKinematic = true;
            GetComponentInParent<Rigidbody>().useGravity = false;
            GetComponentInParent<ForceFieldBehaviour>().AttachedToWall = true;
        }
    }
}
