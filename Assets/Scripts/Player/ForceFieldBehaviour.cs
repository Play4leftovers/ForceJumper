using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceFieldBehaviour : MonoBehaviour
{
    public LayerMask HitMask;
    public Vector3 localPos;
    public Vector3 ImpactPoint;
    public float Health = 3;
    public bool AttachedToWall = false;
    public Vector3 Size;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        CheckForDestroy();
    }

    private void OnTriggerEnter(Collider other)
    {
        /*RaycastHit hit;
        Ray ray = new Ray(transform.position + transform.localScale/2, Player.transform.forward);
        Vector2 directionToTarget = transform.forward;*/

        //Check if we hit something that we are supposed to hit.
        // if (Physics.Raycast(ray, out hit, Mathf.Infinity, HitMask))
        // {
        //     Debug.Log("hit wall!");
        //     GetComponent<Rigidbody>().isKinematic = true;
        // }
        
        if (other.gameObject.CompareTag("Wall"))
        {
            Debug.Log("hit wall!");
            GetComponent<Rigidbody>().isKinematic = true;
            AttachedToWall = true;
        }
        
        if (other.gameObject.CompareTag("Orb"))
        {
            Destroy(other.gameObject);
        }

        if (other.gameObject.CompareTag("Bullet"))
        {
            Health--;
            ImpactPoint = other.transform.position;
            GetComponent<Renderer>().material
                .SetVector("_HitPosition", transform.InverseTransformPoint(ImpactPoint));
        }
    }

    public void CheckForDestroy()
    {
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
