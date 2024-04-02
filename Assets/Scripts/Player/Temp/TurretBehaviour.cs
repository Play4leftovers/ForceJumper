using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBehaviour : MonoBehaviour
{
    public Shooting Player;

    public Vector3 minRotationLimits;
    public Vector3 maxRotationLimits;

    public float lookSpeed;

    public bool CanShoot;

    public GameObject BulletPrefab;

    public Transform FirePoint;

    public float Cooldown;

    public float Range;

    public LayerMask DetectionMask;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Player)
        {
            Ray ray = new Ray(transform.position, Player.PlayerCamera.transform.position - transform.position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Range, DetectionMask))
            {
                Debug.Log("turret looking directly at the player");
                transform.LookAt(Player.PlayerCamera.transform.position);

                if (CanShoot)
                {
                    StartCoroutine(Shoot());
                }
            }
        }
        else
        {
            Player = FindObjectOfType<Shooting>();
        }
    }

    IEnumerator Shoot()
    {
        CanShoot = false;
        Instantiate(BulletPrefab, FirePoint.position, transform.rotation);
        yield return new WaitForSeconds(Cooldown);
        CanShoot = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}
