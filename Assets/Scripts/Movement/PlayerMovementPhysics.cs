using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementPhysics : MonoBehaviour
{
    private Rigidbody _rb;
    private Vector2 _inputDir;
    private Vector3 _moveDir;
    
    [Header("Movement")]
    private float _maxSpeed;
    private float _accelerationSpeed;
    [SerializeField] private float sprintMaxSpeed = 12;
    [SerializeField] private float walkMaxSpeed = 8;
    [SerializeField] private float sprintAccelerationSpeed = 16;
    [SerializeField] private float walkAccelerationSpeed = 10;
    private Vector3 _playerVelocity;
    private bool _sprinting = false;
    
    [Header("Air and Ground Control")]
    [SerializeField] private float groundDrag;
    [SerializeField] private float airDrag;
    [SerializeField] private float airSpeedMultiplier = 0.4f;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private bool doubleJump = false;
    [SerializeField] private float jumpCooldown = 0.3f;
    private bool _jumping = false;
    private bool _readyToJump = true;

    [Header("Ground Check")] 
    private float _playerHeight;
    public LayerMask groundMask;
    public float groundDistance = 0.4f;
    private bool _isGrounded;

    [Header("Slope Control")] [SerializeField]
    private float maxSlopeAngle = 45.0f;
    private RaycastHit slopeHit;
    private bool exitingSlope;


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
        _playerHeight = GetComponent<CapsuleCollider>().height;
    }

    #region Input Management
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _jumping = true;
            if (state == MovementState.Air && doubleJump == false)
            {
                DoubleJump();
            }
        }

        if (context.canceled)
        {
            _jumping = false;
        }
    }

    public void Movement(InputAction.CallbackContext context)
    {
        _inputDir = context.ReadValue<Vector2>();
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _sprinting = true;
        }

        if (context.canceled)
        {
            _sprinting = false;
        }
    }
    #endregion

    #region Updates
    private void Update()
    {
        GroundCheck();
        SpeedControl();
        JumpHandle();
        StateHandler();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void StateHandler()
    {
        if (_isGrounded && _sprinting)
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
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, _playerHeight*0.5f+0.3f, groundMask);
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
        bool onSlope = OnSlope();
        _moveDir = transform1.right * _inputDir.x + transform1.forward * _inputDir.y;
        
        if (onSlope && !exitingSlope)
        {
            _rb.AddForce(GetSlopeMoveDirection() * (_accelerationSpeed * 10), ForceMode.Force);
            if (_rb.velocity.y > 0)
            {
                _rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            } 
        }
        
        if (_isGrounded) _rb.AddForce(_moveDir * (_accelerationSpeed * 10), ForceMode.Force);
        else if(!_isGrounded) _rb.AddForce(_moveDir * (_accelerationSpeed * airSpeedMultiplier * 10), ForceMode.Force);

        _rb.useGravity = !onSlope;
    }
    
    void SpeedControl()
    {
        var tempVelocity = _rb.velocity;

        if (OnSlope() && !exitingSlope)
        {
            if (tempVelocity.magnitude > _maxSpeed)
                _rb.velocity = tempVelocity.normalized * _maxSpeed;
        }
        else
        {
            Vector3 flatVelocity = new Vector3(tempVelocity.x, 0f, tempVelocity.z);
            if (flatVelocity.magnitude > _maxSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * _maxSpeed;
                _rb.velocity = new Vector3(limitedVelocity.x, _rb.velocity.y, limitedVelocity.z);
            }
        }
    }
    
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(_moveDir, slopeHit.normal).normalized;
    }
    
    bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, _playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            if (angle < maxSlopeAngle && angle != 0) return true;
            else return false;
        }
        return false;
    }

    void JumpHandle()
    {
        if (_isGrounded && _readyToJump && _jumping)
        {
            _readyToJump = false;
            Jumping();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    
    void Jumping()
    {
        exitingSlope = true;
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void DoubleJump()
    {
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        doubleJump = true;
    }

    void ResetJump()
    {
        _readyToJump = true;
        exitingSlope = false;
    }
}