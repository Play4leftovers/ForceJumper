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
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private float moveSpeed = 10;
    //[SerializeField] private float sprintMultiplier = 2;
    [SerializeField] private Vector3 playerVelocity;

    [Header("Gravity")] 
    public Transform groundCheck;
    public LayerMask groundMask;
    public float groundDistance = 0.4f;

    private bool _isGrounded;
    
    void Start()
    {
        Debug.Log("Start");
        _cc = GetComponent<CharacterController>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        playerVelocity.y += jumpForce;
        Debug.Log("Jump!");
    }
    
    public void Movement(InputAction.CallbackContext context)
    {
        _inputDir = context.ReadValue<Vector2>();
    }
    
    private void Update()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (_isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        
        var transform1 = transform;
        Vector3 moveDir = transform1.right * _inputDir.x + transform1.forward * _inputDir.y;
        _cc.Move(moveDir * moveSpeed * Time.deltaTime);

        playerVelocity.y += Physics.gravity.y * Time.deltaTime;

        _cc.Move(playerVelocity * Time.deltaTime);
    }
}