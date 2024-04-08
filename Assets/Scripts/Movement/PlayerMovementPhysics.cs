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

    private bool _inputStopper = false;
    
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 8;
    [SerializeField] private float accelerationSpeed = 10;
    private Vector3 _playerVelocity;
    
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
    private RaycastHit _slopeHit;
    private bool _exitingSlope;

    [Header("Dashing")] 
    private bool _dashing;
    [SerializeField] private float dashingDuration = 0.1f;
    [SerializeField] private float dashingSpeed = 2.5f; //Multiplier of maxSpeed
    [SerializeField] private float dashingAcceleration = 10.0f; //Multiplier of accelerationSpeed

    public MovementState state;
    public enum MovementState
    {
        Walking,
        Air,
        Dash
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

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && !_dashing)
        {
            _dashing = true;
            _inputStopper = true;
            _exitingSlope = true;
            maxSpeed *= dashingSpeed;
            accelerationSpeed *= dashingAcceleration;
            Invoke(nameof(StopDash), dashingDuration);
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
    #endregion

    #region StateHandler
    private void StateHandler()
    {
        if (!_isGrounded && !_dashing)
        {
            state = MovementState.Air;
        }
        else if(_dashing)
        {
            state = MovementState.Dash;
        }
        else
        {
            state = MovementState.Walking;
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
        
        if(!_inputStopper) _moveDir = transform1.right * _inputDir.x + transform1.forward * _inputDir.y;
        
        if (onSlope && !_exitingSlope)
        {
            _rb.AddForce(GetSlopeMoveDirection() * (accelerationSpeed * 10), ForceMode.Force);
            if (_rb.velocity.y > 0)
            {
                _rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            } 
        }
        
        if (_isGrounded) _rb.AddForce(_moveDir * (accelerationSpeed * 10), ForceMode.Force);
        else if(!_isGrounded) _rb.AddForce(_moveDir * (accelerationSpeed * airSpeedMultiplier * 10), ForceMode.Force);

        _rb.useGravity = !onSlope;
    }
    
    void SpeedControl()
    {
        var tempVelocity = _rb.velocity;

        if (OnSlope() && !_exitingSlope)
        {
            if (tempVelocity.magnitude > maxSpeed)
                _rb.velocity = tempVelocity.normalized * maxSpeed;
        }
        else
        {
            Vector3 flatVelocity = new Vector3(tempVelocity.x, 0f, tempVelocity.z);
            if (flatVelocity.magnitude > maxSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * maxSpeed;
                _rb.velocity = new Vector3(limitedVelocity.x, _rb.velocity.y, limitedVelocity.z);
            }
        }

        if (_dashing)
        {
            if (tempVelocity.y > maxSpeed / dashingSpeed)
            {
                _rb.velocity = new Vector3(_rb.velocity.x, maxSpeed / dashingSpeed, _rb.velocity.z);
            }
        }
    }
    
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(_moveDir, _slopeHit.normal).normalized;
    }
    
    bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, _playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
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
        _exitingSlope = true;
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
        _exitingSlope = false;
    }

    void StopDash()
    {
        maxSpeed /= dashingSpeed;
        accelerationSpeed /= dashingAcceleration;
        _dashing = false;
        _inputStopper = false;
        _exitingSlope = true;
    }
}