using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    private Vector2 _lookDir;
    private Transform _playerBody;

    private float xRotation = 0f;
    
    public float mouseSensitivity = 100f;
    // Start is called before the first frame update
    void Start()
    {
        _playerBody = transform.parent.GetComponent<Transform>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        _lookDir = _lookDir * (mouseSensitivity * Time.deltaTime);
        xRotation -= _lookDir.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        _playerBody.Rotate(Vector3.up * _lookDir.x);
    }

    public void Look(InputAction.CallbackContext context)
    {
        Debug.Log("Hello look");
        _lookDir = context.ReadValue<Vector2>();
    }
}
