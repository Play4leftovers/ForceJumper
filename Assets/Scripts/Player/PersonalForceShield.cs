using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PersonalForceShield : MonoBehaviour
{
    public GameObject ForceFieldPrefab;
    public float OrbScale;
    public float Duration;
    public bool Active;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ActivatePersonalForceShield(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (Active) return;
            
            GameObject newForceField = SpawnForceField();
            newForceField.transform.SetParent(transform);
            Destroy(newForceField, Duration);
            Invoke(nameof(Deactivate), Duration);
        }
    }
    
    public GameObject SpawnForceField()
    {
        GameObject newForceField = Instantiate(ForceFieldPrefab, transform.position, Quaternion.identity);
        
        ForceFieldBehaviour forceFieldScript = newForceField.GetComponent<ForceFieldBehaviour>();
        Rigidbody forceFieldRigidbody = newForceField.GetComponent<Rigidbody>();
        
        forceFieldScript.AttachedToWall = false;
        forceFieldScript.Type = ForceFieldBehaviour.ShieldType.Personal;
        forceFieldScript.Owner = gameObject;
        
        forceFieldRigidbody.useGravity = false;
        forceFieldRigidbody.isKinematic = true;

        //GameManager.instance.CurrentOrb = newForceField.GetComponent<ForceFieldBehaviour>();
        var localScale = newForceField.transform.localScale;
        localScale *= OrbScale;
        newForceField.transform.localScale = localScale;
        newForceField.GetComponent<ForceFieldBehaviour>().Size = localScale * OrbScale;
        Active = true;
        return newForceField;
    }

    public void Deactivate()
    {
        Active = false;
    }
}
