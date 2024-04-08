using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrenadeThrowing : MonoBehaviour
{
    public GameObject GrenadePrefab;
    public GameObject OrbPrefab;
    public GameObject FirePoint;
    public Grenade CurrentGrenade;
    public float OrbScale;
    private bool grenadeShot;
    public Camera PlayerCamera;
    public float ThrowForce;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (!grenadeShot)
            {
                SpawnGrenade();
            }
            else
            {
                if (CurrentGrenade != null)
                {
                    SpawnForceField();
                }
            }
        }
    }

    private void SpawnGrenade()
    {
        GameObject newGrenade = Instantiate(GrenadePrefab, FirePoint.transform.position, Quaternion.identity);
        CurrentGrenade = newGrenade.GetComponent<Grenade>();
        newGrenade.GetComponent<Rigidbody>().AddForce(PlayerCamera.transform.forward * ThrowForce, ForceMode.Impulse);
        grenadeShot = true;
    }

    public void SpawnForceField()
    {
        GameObject newOrb = Instantiate(OrbPrefab, CurrentGrenade.transform.position, Quaternion.identity);
        newOrb.GetComponent<ForceFieldBehaviour>().AttachedToWall = false;
        //GameManager.instance.CurrentOrb = newOrb.GetComponent<ForceFieldBehaviour>();
        var localScale = newOrb.transform.localScale;
        localScale *= OrbScale;
        newOrb.transform.localScale = localScale;
        newOrb.GetComponent<ForceFieldBehaviour>().Size = localScale * OrbScale;
        Destroy(CurrentGrenade.gameObject);
        CurrentGrenade = null;
        grenadeShot = false;
    }
}
