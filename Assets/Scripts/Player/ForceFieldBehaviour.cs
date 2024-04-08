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
        if (other.gameObject.CompareTag("ForceField"))
        {
            Destroy(other.gameObject);
        }

        if (other.gameObject.CompareTag("Bullet"))
        {
            Health--;
            ImpactPoint = other.transform.position;
            GetComponent<Renderer>().material
                .SetVector("_HitPosition", transform.InverseTransformPoint(ImpactPoint));
            Destroy(other.gameObject);
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
