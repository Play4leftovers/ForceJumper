using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerMovementPhysics : MonoBehaviour
{
    #region Variables
    private Rigidbody _rb;
    private Vector2 _inputDir;
    private Vector3 _moveDir;
    private CapsuleCollider _capsuleCollider;

    private bool _inputStopper = false;
    
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 8;
    [SerializeField] private float accelerationForce = 10;
    [SerializeField] private float speedChangeMultiplier = 2.5f;
    [SerializeField] private float slopeSpeedChangeMultiplier = 1.5f;
    private float _floatingMaxSpeed;
    private Vector3 _playerVelocity;
    private float _desiredMoveSpeed;
    private float _lastDesiredMoveSpeed;
    private const float SpeedChangeThreshold = 4f;

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

    [Header("Slope Control")] 
    [SerializeField] private bool onSlope;
    [SerializeField] private float maxSlopeAngle = 45.0f;
    private RaycastHit _slopeHit;
    private bool _exitingSlope;

    [Header("Dashing")] 
    [SerializeField] private float dashingDuration = 0.1f;
    [SerializeField] private float dashingSpeed = 2.5f; //Multiplier of maxSpeed
    private bool _dashing;

    [Header("Crouch and Sliding")] 
    [SerializeField] private float crouchSpeed = 0.5f; //Multiplier of maxSpeed
    [SerializeField] private float slopeSlideSpeed = 3f; //Multiplier of maxSpeed
    [SerializeField] private float crouchYScale = 0.5f;
    private bool _crouching;

    public MovementState state;
    public enum MovementState
    {
        Standing,
        Walking,
        Air,
        Dash,
        Crouching,
        Sliding
    }
    #endregion
    
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _playerHeight  = _capsuleCollider.height;
        _floatingMaxSpeed = _desiredMoveSpeed = maxSpeed;
    }

    #region Input Management
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && !_crouching)
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

    public void Crouch(InputAction.CallbackContext context)
    {
        if (context.performed && !_dashing)
        {
            _crouching = true;
            var tempGrounded = _isGrounded;
            _capsuleCollider.height *= crouchYScale;
            if (tempGrounded)
            {
                _rb.AddForce(Vector3.down * accelerationForce * 0.5f, ForceMode.Impulse);
            }
        }

        if (context.canceled)
        {
            _crouching = false;
            _desiredMoveSpeed = maxSpeed;
            _capsuleCollider.height /= crouchYScale;
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && !_dashing && !_crouching)
        {
            _dashing = true;
            _inputStopper = true;
            _exitingSlope = true;
            _desiredMoveSpeed = maxSpeed * dashingSpeed;
            _floatingMaxSpeed = _desiredMoveSpeed;
            Invoke(nameof(StopDash), dashingDuration);
        }
    }
    #endregion
    
    #region Updates
    private void Update()
    {
        GroundCheck();
        OnSlope();
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
            _desiredMoveSpeed = maxSpeed;
        }
        else if(_dashing)
        {
            state = MovementState.Dash;
        }
        else if (_crouching && onSlope)
        {
            state = MovementState.Sliding;
            _desiredMoveSpeed = maxSpeed * slopeSlideSpeed;
        }
        else if (_crouching && (_floatingMaxSpeed > _desiredMoveSpeed))
        {
            state = MovementState.Sliding;
            _desiredMoveSpeed = maxSpeed * crouchSpeed;
        }
        else if (_crouching)
        {
            state = MovementState.Crouching;
            _desiredMoveSpeed = maxSpeed * crouchSpeed;
        }
        else if (_inputDir.magnitude != 0)
        {
            state = MovementState.Walking;
            _desiredMoveSpeed = maxSpeed;
        }
        else
        {
            state = MovementState.Standing;
            _desiredMoveSpeed = 0;
        }

        if (Mathf.Abs(_desiredMoveSpeed - _lastDesiredMoveSpeed) > SpeedChangeThreshold)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothLerpMoveSpeed());
        }
        else
        {
            _floatingMaxSpeed = _desiredMoveSpeed;
        }
        
        _lastDesiredMoveSpeed = _desiredMoveSpeed;
    }
    #endregion
    
    #region Ground and Slope Check
    void GroundCheck()
    {
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, _playerHeight*0.5f+0.3f, groundMask);
        if (_isGrounded || state != MovementState.Dash)
        {
            _rb.drag = groundDrag;
            if (doubleJump) doubleJump = false;
        }
        else
        {
            _rb.drag = airDrag;
        }
    }
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(_moveDir, _slopeHit.normal).normalized;
    }
    
    void OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, _playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            if (angle < maxSlopeAngle && angle != 0) onSlope = true;
            else onSlope = false;
        }
        else
        {
            onSlope = false;
        }
    }
    #endregion
    
    #region Movement and Speed Control
    void MovePlayer()
    {
        var transform1 = transform;
        
        //if(!_inputStopper && _inputDir.magnitude != 0) _moveDir = transform1.right * _inputDir.x + transform1.forward * _inputDir.y;
        //Currently unusable. Intended to make you continue moving even when not holding down a key as if to slow down to standing still rather than stopping on a dime.
        //It works, but does not feel good. Current problem is that directional momentum is not maintained.
        
        if(!_inputStopper) _moveDir = transform1.right * _inputDir.x + transform1.forward * _inputDir.y;
        if (onSlope && !_exitingSlope)
        {
            _rb.AddForce(GetSlopeMoveDirection() * (_floatingMaxSpeed * accelerationForce * 2), ForceMode.Force);
            if (_rb.velocity.y > 0)
            {
                _rb.AddForce(Vector3.down * (accelerationForce * 8f), ForceMode.Force);
            } 
        }
        
        else switch (_isGrounded)
        {
            case true:
                _rb.AddForce(_moveDir * (_floatingMaxSpeed * accelerationForce), ForceMode.Force);
                break;
            case false:
                _rb.AddForce(_moveDir * (_floatingMaxSpeed * airSpeedMultiplier * 10), ForceMode.Force);
                break;
        }

        _rb.useGravity = !onSlope;
    }
    
    void SpeedControl()
    {
        var tempVelocity = _rb.velocity;

        if (onSlope && !_exitingSlope)
        {
            if (tempVelocity.magnitude > _floatingMaxSpeed)
                _rb.velocity = tempVelocity.normalized * _floatingMaxSpeed;
        }
        else
        {
            Vector3 flatVelocity = new Vector3(tempVelocity.x, 0f, tempVelocity.z);
            if (flatVelocity.magnitude > _floatingMaxSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * _floatingMaxSpeed;
                _rb.velocity = new Vector3(limitedVelocity.x, _rb.velocity.y, limitedVelocity.z);
            }
        }

        if (_dashing)
        {
            if (tempVelocity.y > _floatingMaxSpeed / dashingSpeed)
            {
                _rb.velocity = new Vector3(_rb.velocity.x, _floatingMaxSpeed / dashingSpeed, _rb.velocity.z);
            }
        }
    }

    private IEnumerator SmoothLerpMoveSpeed() //Requires rework as it is too slow in its changes
    {
        float time = 0;
        float diff = Mathf.Abs(_desiredMoveSpeed - _floatingMaxSpeed);
        float startValue = _floatingMaxSpeed;

        while (time < diff)
        {
            _floatingMaxSpeed = Mathf.Lerp(startValue, _desiredMoveSpeed, time / diff);

            if (onSlope)
            {
                float slopeAngle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);
                time += Time.deltaTime * slopeSpeedChangeMultiplier * slopeAngleIncrease;
            }
            else
            {
                time += Time.deltaTime * speedChangeMultiplier;
            }
            yield return null;
        }

        _floatingMaxSpeed = _desiredMoveSpeed;
    }
    #endregion
    
    #region Jumping and Dashing
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
        _desiredMoveSpeed = maxSpeed;
        _floatingMaxSpeed = _desiredMoveSpeed;
        //Temporary fix to allow for rapid dashes. Will implement gradual decrease in speed to make it feel less jerky
        _dashing = false;
        _inputStopper = false;
        _exitingSlope = false;
    }
    #endregion
}