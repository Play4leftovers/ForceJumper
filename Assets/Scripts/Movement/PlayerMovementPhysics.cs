using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [HideInInspector] public float currentSpeed;
    
    private bool _inputStopper = false;
    
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 8;
    [SerializeField] private float accelerationForce = 10;
    [SerializeField] private float speedChangeMultiplier = 2.5f;
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
    public bool doubleJump = false;
    [SerializeField] private float jumpCooldown = 0.3f;
    private bool _jumping;
    private bool _readyToJump = true;

    [Header("Ground Check")] 
    private float _playerHeight;
    public LayerMask groundMask;
    private bool _isGrounded;

    [Header("Slope Control")] 
    [SerializeField] private bool onSlope;
    [SerializeField] private float maxSlopeAngle = 45.0f;
    private RaycastHit _slopeHit;
    private bool _exitingSlope;

    [Header("Dashing")] 
    [SerializeField] private float dashingDuration = 0.1f;
    [SerializeField] private float dashingSpeed = 2.5f; //Multiplier of maxSpeed
    [SerializeField] private float dashingSpeedChangeMultiplier = 40;
    [SerializeField] private float dashCooldown = 2.0f;
    public bool dashReady = true;
    private bool _dashing;
    private bool _keepMomentum;
    private float _preDashFloatingMaxSpeed;
    
    [Header("Crouch and Sliding")] 
    [SerializeField] private float crouchSpeed = 0.5f; //Multiplier of maxSpeed
    [SerializeField] private float slopeSlideSpeed = 3f; //Multiplier of maxSpeed
    [SerializeField] private float crouchYScale = 0.5f;
    [SerializeField] private float slopeSpeedChangeMultiplier = 7.5f;
    private bool _crouching;

    [Header("Wallrunning")] 
    [SerializeField] private float wallRunningSpeed = 1.5f;
    [SerializeField] private float wallrunningExitDuration = 0.2f;
    [SerializeField] private float wallrunningDownSpeed = 0.5f;
    public LayerMask wallMask;
    private RaycastHit _outLeft;
    private RaycastHit _outRight;
    private bool _wallRunning;
    private bool _exitingWallrun;
    private bool _wallToRight;
    private bool _wallToLeft;
    private Vector3 _wallrunningWallParallelDirection;
    
    [Header("States")] 
    public MovementState state;
    private MovementState _previousState;
    private MovementState _lastFrameState;
    
    public enum MovementState
    {
        Standing,
        Walking,
        Air,
        Dash,
        Crouching,
        Sliding,
        Wallrunning
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
        if (context.performed && !_dashing && !_crouching && dashReady)
        {
            _dashing = true;
            _preDashFloatingMaxSpeed = _floatingMaxSpeed;
            _inputStopper = true;
            _exitingSlope = true;
            dashReady = false;
            Invoke(nameof(StopDash), dashingDuration);
        }
    }
    #endregion
    
    #region Updates
    private void Update()
    {
        GroundCheck();
        WallrunningCheck();
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
        _lastFrameState = state;
        //State - Dashing
        if(_dashing)
        {
            state = MovementState.Dash;
            _desiredMoveSpeed = maxSpeed * dashingSpeed;
        }
        //State - Wallrunning
        else if (_wallRunning)
        {
            state = MovementState.Wallrunning;
            doubleJump = false;
            _desiredMoveSpeed = maxSpeed * wallRunningSpeed;
        }
        //State - Air
        else if (!_isGrounded)
        {
            state = MovementState.Air;
            _desiredMoveSpeed = maxSpeed;
        }
        //State - Sliding Slope
        else if (_crouching && onSlope)
        {
            state = MovementState.Sliding;
            _desiredMoveSpeed = maxSpeed * slopeSlideSpeed;
        }
        //State - Sliding Crouch
        else if (_crouching && (_floatingMaxSpeed > _desiredMoveSpeed))
        {
            state = MovementState.Sliding;
            _desiredMoveSpeed = maxSpeed * crouchSpeed;
        }
        //State - Crouch
        else if (_crouching)
        {
            state = MovementState.Crouching;
            _desiredMoveSpeed = maxSpeed * crouchSpeed;
        }
        //State - Walking
        else if (_inputDir.magnitude != 0)
        {
            state = MovementState.Walking;
            _desiredMoveSpeed = maxSpeed;
        }
        //State - Standing
        else
        {
            state = MovementState.Standing;
            _desiredMoveSpeed = maxSpeed;
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
        if (_lastFrameState != state)
        {
            _previousState = _lastFrameState;
        }
    }
    #endregion
    
    #region Ground, Wall, and Slope Check
    void GroundCheck()
    {
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, _playerHeight*0.5f+0.3f, groundMask);
        if (_isGrounded && state is not (MovementState.Dash or MovementState.Sliding))
        {
            _rb.drag = groundDrag;
            if (doubleJump) doubleJump = false;
        }
        else
        {
            _rb.drag = airDrag;
        }
    }

    //Todo: add more conditions 
    //Moving up (or possibly down) at a steady pace while wallrunning
    //Check so that it actually has a max angle you can look while wallrunning
    //When you let go of "jump" while wallrunning, it jumps a bit away from the wall and upwards

    void WallrunningCheck()
    {
        _wallToRight = Physics.Raycast(transform.position, transform.right, out _outRight, 1, wallMask);
        _wallToLeft = Physics.Raycast(transform.position, -transform.right, out _outLeft, 1, wallMask);

        if (_jumping && (_wallToLeft || _wallToRight) && !_exitingWallrun)
        {
            _wallRunning = true;
            _rb.useGravity = false;
            var velocity = _rb.velocity;
            velocity = new Vector3(velocity.x, wallrunningDownSpeed, velocity.z);
            _rb.velocity = velocity;

            if (_wallToRight)
            {
                _wallrunningWallParallelDirection = Quaternion.AngleAxis(90, Vector3.up) * _outRight.normal;
            }
            else
            {
                _wallrunningWallParallelDirection = Quaternion.AngleAxis(-90, Vector3.up) * _outLeft.normal;
            }
        }
        else
        {
            if (_wallRunning)
            {
                ExitingWallrun();
                Invoke(nameof(ExitingWallrun), wallrunningExitDuration);
            }
            _wallRunning = false;
            _rb.useGravity = true;
        }
    }

    void ExitingWallrun()
    {
        _exitingWallrun = !_exitingWallrun;
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
        
        if(!_inputStopper) _moveDir = transform1.right * _inputDir.x + transform1.forward * _inputDir.y;
        if (onSlope && !_exitingSlope)
        {
            _rb.AddForce(GetSlopeMoveDirection() * (_floatingMaxSpeed * accelerationForce * 2), ForceMode.Force);
            if (_rb.velocity.y > 0)
            {
                _rb.AddForce(Vector3.down * (accelerationForce * 8f), ForceMode.Force);
            } 
        }
        
        else if (_wallRunning)
        {
            _rb.AddForce(_wallrunningWallParallelDirection * (_floatingMaxSpeed * accelerationForce), ForceMode.Force);
        }
        
        else switch (_isGrounded || _dashing)
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

        currentSpeed = _rb.velocity.magnitude;
    }

    private IEnumerator SmoothLerpMoveSpeed()
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
                time += Time.deltaTime * slopeSpeedChangeMultiplier * speedChangeMultiplier * slopeAngleIncrease;
            }
            else if (_dashing || _previousState == MovementState.Dash)
            {
                time += Time.deltaTime * speedChangeMultiplier * dashingSpeedChangeMultiplier;
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
        if (_isGrounded && _readyToJump && _jumping && !_wallRunning)
        {
            _readyToJump = false;
            Jumping();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    
    void Jumping()
    {
        _exitingSlope = true;
        var velocity = _rb.velocity;
        if (velocity.y < 0)
        {
            velocity = new Vector3(velocity.x, 0, velocity.z);
            _rb.velocity = velocity;
        }
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void DoubleJump()
    {
        var velocity = _rb.velocity;
        if (velocity.y < 0)
        {
            velocity = new Vector3(velocity.x, 0, velocity.z);
            _rb.velocity = velocity;
        }
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
        _desiredMoveSpeed = _preDashFloatingMaxSpeed;
        _dashing = false;
        _inputStopper = false;
        _exitingSlope = false;
        Invoke(nameof(ResetDashAvailability), dashCooldown);
    }

    void ResetDashAvailability()
    {
        dashReady = true;
    }
    #endregion
}