using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController _cc;
    private Vector2 _inputDir;
    
    [Header("Movement")]
    [SerializeField] private float jumpForce = 5;
    [SerializeField] private float moveSpeed = 10;
    //[SerializeField] private float sprintMultiplier = 2;
    [SerializeField] private Vector3 playerVelocity;
    
    void Start()
    {
        Debug.Log("Start");
        _cc = GetComponent<CharacterController>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        Debug.Log("Jump!");
    }
    
    public void Movement(InputAction.CallbackContext context)
    {
        _inputDir = context.ReadValue<Vector2>();
    }
    
    private void Update()
    {
        var transform1 = transform;
        Vector3 moveDir = transform1.right * _inputDir.x + transform1.forward * _inputDir.y;
        _cc.Move(moveDir * moveSpeed * Time.deltaTime);
    }
}