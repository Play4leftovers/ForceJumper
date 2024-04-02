using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementPhysics : MonoBehaviour
{
    private Rigidbody _rb;
    private Vector2 _inputDir;
    
    [Header("Movement")]
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private float accelerationSpeed = 10;
    [SerializeField] private float maxSpeed = 8;
    //[SerializeField] private float sprintMultiplier = 2;
    [SerializeField] private Vector3 playerVelocity;
    [SerializeField] private float groundDrag;
    [SerializeField] private float airDrag;

    [Header("Gravity")] 
    public Transform groundCheck;
    public LayerMask groundMask;
    public float groundDistance = 0.4f;

    private bool _isGrounded;
    
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    #region Input Management
    public void Jump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        Debug.Log("Jump!");
    }
    
    public void Movement(InputAction.CallbackContext context)
    {
        _inputDir = context.ReadValue<Vector2>();
    }
    #endregion

    #region Updates
    private void Update()
    {
        GroundCheck();
        SpeedControl();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }
    #endregion

    
    void GroundCheck()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (_isGrounded)
        {
            _rb.drag = groundDrag;
        }
        else
        {
            _rb.drag = airDrag;
        }
    }
    
    void MovePlayer()
    {
        var transform1 = transform;
        Vector3 moveDir = transform1.right * _inputDir.x + transform1.forward * _inputDir.y;

        _rb.AddForce(moveDir * accelerationSpeed, ForceMode.Force);
    }

    void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        if (flatVelocity.magnitude > maxSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * maxSpeed;
            _rb.velocity = new Vector3(limitedVelocity.x, _rb.velocity.y, limitedVelocity.z);
        }
    }
}