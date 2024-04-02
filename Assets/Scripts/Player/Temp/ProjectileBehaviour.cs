using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    public float ShootingForce;
    public float TimeToDestroy;
    public GameObject Impact;
    public Vector3 ImpactPoint;
    
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, TimeToDestroy);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GetComponent<Rigidbody>().AddForce(ShootingForce * transform.forward);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("hit player!");
            //GameManager.instance.PlayerHP--;
        }

        if (other.gameObject.CompareTag("ForceField"))
        {
            //other.GetComponent<OrbBehaviour>().ImpactPoint = transform.position;
            Debug.Log("hit orb!");
            Instantiate(Impact, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
        }

        //Impact.GetComponent<ParticleSystem>().startColor = other.GetComponent<Renderer>().material.GetVector("Shield Pattern Color");

        Destroy(gameObject);
    }
}
