using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shooting : MonoBehaviour
{
    public GameObject OrbPrefab;
    public float CooldownTime;
    public bool CanShoot;
    public float ShootForce;
    public Camera PlayerCamera;
    public float OrbScale;
    public float MaxScale = 6f;
    public float increase;
    public GameObject FirePoint;
    public ForceFieldBehaviour PreExistingOrb;
    bool isFirstClick = false;

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
        if (Mouse.current.leftButton.isPressed)
        {
            OrbScale += increase * Time.deltaTime;
            OrbScale = Mathf.Clamp(OrbScale, 2f, MaxScale);
        }
    }

    public void ShootForceField()
    {
        Ray ray = PlayerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
            
        if (CanShoot)
        {
            if (FindObjectOfType<ForceFieldBehaviour>())
            {
                PreExistingOrb = FindObjectOfType<ForceFieldBehaviour>();
                Destroy(PreExistingOrb.gameObject);
            }
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                GameObject newForceField = Instantiate(OrbPrefab, FirePoint.transform.position, Quaternion.identity);
                newForceField.GetComponent<ForceFieldBehaviour>().AttachedToWall = false;
                //GameManager.instance.CurrentOrb = newForceField.GetComponent<ForceFieldBehaviour>();
                var localScale = newForceField.transform.localScale;
                localScale *= OrbScale;
                newForceField.transform.localScale = localScale;
                newForceField.GetComponent<ForceFieldBehaviour>().Size = localScale * OrbScale;

                Vector2 directionToTarget = (hit.transform.position - FirePoint.transform.position).normalized; 
                newForceField.GetComponent<Rigidbody>().AddForce(PlayerCamera.transform.forward * ShootForce, ForceMode.Impulse);
            }   
        }
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (isFirstClick)
            {
                OrbScale = 2f;
                isFirstClick = false;
            }
        }
        else if (context.canceled)
        {
            ShootForceField();
            isFirstClick = true;
        }
    }
    
}
