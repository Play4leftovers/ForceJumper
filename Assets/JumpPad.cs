using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float force = 25f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        //other.transform.GetComponent<Rigidbody>().AddForce(Vector3.up * Force, ForceMode.Impulse);
        var temp = other.transform.GetComponent<Rigidbody>().velocity;
        other.transform.GetComponent<Rigidbody>().velocity = new Vector3(temp.x, force, temp.z);
    }
}
