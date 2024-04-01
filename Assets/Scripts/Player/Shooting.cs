using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public GameObject OrbPrefab;
    public float CooldownTime;
    public bool CanShoot;
    public float ShootForce;
    public Camera PlayerCamera;
    [Range(2, 6)] public float OrbScale;
    public float increase;
    public GameObject FirePoint;
    public ForceFieldBehaviour PreExistingOrb;

    private void Awake()
    {
    }
    
    // Start is called before the first frame update
    void Start()
    {
        CanShoot = true;
        PlayerCamera = transform.GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = PlayerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Input.GetMouseButtonDown(0))
        {
            OrbScale = 2;   
        }

        if (Input.GetMouseButton(0))
        {
            if (OrbScale >= 2 && OrbScale <= 6)
            {
                OrbScale += increase;
            }
        }

        /*if (GameManager.instance.CurrentOrb)
        {
            if (!GameManager.instance.CurrentOrb.AttachedToWall)
            {
                CanShoot = false;   
            }
            else if (GameManager.instance.CurrentOrb.AttachedToWall)
            {
                CanShoot = true;
            }
        }
        else
        {
            Debug.Log("No orb detected in the scene, can shoot again.");
            CanShoot = true;
        }*/
        
        if (Input.GetMouseButtonUp(0))
        {
            if (CanShoot)
            {
                if (FindObjectOfType<ForceFieldBehaviour>())
                {
                    PreExistingOrb = FindObjectOfType<ForceFieldBehaviour>();
                    Destroy(PreExistingOrb.gameObject);
                }
            
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    GameObject newOrb = Instantiate(OrbPrefab, FirePoint.transform.position, Quaternion.identity);
                    newOrb.GetComponent<ForceFieldBehaviour>().AttachedToWall = false;
                    //GameManager.instance.CurrentOrb = newOrb.GetComponent<ForceFieldBehaviour>();
                    var localScale = newOrb.transform.localScale;
                    localScale *= OrbScale;
                    newOrb.transform.localScale = localScale;
                    newOrb.GetComponent<ForceFieldBehaviour>().Size = localScale * OrbScale;

                    Vector2 directionToTarget = (hit.transform.position - FirePoint.transform.position).normalized; 
                    newOrb.GetComponent<Rigidbody>().AddForce(FirePoint.transform.forward * ShootForce, ForceMode.Impulse);
                }   
            }
        }
    }
}
