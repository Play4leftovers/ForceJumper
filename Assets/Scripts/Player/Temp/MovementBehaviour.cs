using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementBehaviour : MonoBehaviour
{
    public float HorizontalMovement;
    public float VerticalMovement;
    public Vector3 MovementDirection;
    public float MovementSpeed;
    public Camera PlayerCamera;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HorizontalMovement = Input.GetAxisRaw("Horizontal");
        VerticalMovement = Input.GetAxisRaw("Vertical");
        
        MovementDirection = PlayerCamera.GetComponent<CameraController>().Heading.forward * VerticalMovement +
                            PlayerCamera.GetComponent<CameraController>().Heading.right * HorizontalMovement;
        
        GetComponent<Rigidbody>().AddForce(MovementDirection.normalized * MovementSpeed, ForceMode.Force);
    }
}
