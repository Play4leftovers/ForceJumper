using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float MouseSensitivityX = 100f;
    public float MouseSensitivityY = 100f;
    
    private Vector2 rotation = Vector2.zero;
    public Transform Heading;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Update()
    {
        Look();
    }
    
    public void Look()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * MouseSensitivityX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * MouseSensitivityY;
        
        rotation.y += mouseX;
        
        rotation.x -= mouseY;
        rotation.x = Mathf.Clamp(rotation.x, -90f, 90f);

        transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0);
        Heading.rotation = Quaternion.Euler(0, rotation.y, 0);
    }
}
