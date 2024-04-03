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
    private float _maxSpeed;
    private float _accelerationSpeed;
    [SerializeField] private float sprintMaxSpeed = 12;
    [SerializeField] private float walkMaxSpeed = 8;
    [SerializeField] private float sprintAccelerationSpeed = 16;
    [SerializeField] private float walkAccelerationSpeed = 10;
    [SerializeField] private float groundDrag;
    [SerializeField] private float airDrag;
    [SerializeField] private float airSpeedMultiplier = 0.4f;
    [SerializeField] private Vector3 playerVelocity;
    [SerializeField] private bool sprinting = false;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private bool doubleJump = false;
    
    [Header("Ground Check")] 
    public Transform groundCheck;
    public LayerMask groundMask;
    public float groundDistance = 0.4f;

    private bool _isGrounded;


    public MovementState state;
    public enum MovementState
    {
        Walking,
        Sprinting,
        Air
    }
    
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    #region Input Management
    public void Jump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        if(_isGrounded) _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        else DoubleJump();
    }

    public void Movement(InputAction.CallbackContext context)
    {
        _inputDir = context.ReadValue<Vector2>();
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            sprinting = true;
        }

        if (context.canceled)
        {
            sprinting = false;
        }
    }
    #endregion

    #region Updates
    private void Update()
    {
        GroundCheck();
        SpeedControl();
        StateHandler();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void StateHandler()
    {
        if (_isGrounded && sprinting)
        {
            state = MovementState.Sprinting;
            _accelerationSpeed = sprintAccelerationSpeed;
            _maxSpeed = sprintMaxSpeed;
        }
        else if (!_isGrounded)
        {
            state = MovementState.Air;
        }
        else
        {
            state = MovementState.Walking;
            _accelerationSpeed = walkAccelerationSpeed;
            _maxSpeed = walkMaxSpeed;
        }
    }
    #endregion

    
    void GroundCheck()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (_isGrounded)
        {
            _rb.drag = groundDrag;
            if (doubleJump) doubleJump = false;
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

        if (_isGrounded) _rb.AddForce(moveDir * (_accelerationSpeed * 10), ForceMode.Force);
        else _rb.AddForce(moveDir * (_accelerationSpeed * airSpeedMultiplier * 10), ForceMode.Force);
    }

    void SpeedControl()
    {
        var tempVelocity = _rb.velocity;
        Vector3 flatVelocity = new Vector3(tempVelocity.x, 0f, tempVelocity.z);
        if (flatVelocity.magnitude > _maxSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * _maxSpeed;
            _rb.velocity = new Vector3(limitedVelocity.x, _rb.velocity.y, limitedVelocity.z);
        }
    }

    void DoubleJump()
    {
        if (doubleJump) return;
        _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        doubleJump = true;
    }
}