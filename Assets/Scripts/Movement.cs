using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class Movement : MonoBehaviour
{
    // components
    [Description("Components")]
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] BoxCollider2D _collider;

    // movement values
    [Description("Values, Specifications")]
    [SerializeField] private Vector2 _direction;
    [SerializeField] private float _maxWalkSpeed = 8f;
    [SerializeField] private float _maxRunSpeed = 15f;
    [SerializeField] private float _acceleration = 50f;
    [SerializeField] private float _deceleration = 40f;
    [SerializeField] private float _groundFriction = 0.96f;
    [SerializeField] private bool _moving = false;
    [SerializeField] private bool _running = false;
    [SerializeField] private bool _jumping = false;
    [SerializeField] private bool _sliding = false;
    [SerializeField] private bool _crouching = false;

    // jump variables
    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _variableJumpHeightMultiplier = 0.5f;
    [SerializeField] private float _fallGravityMultiplier = 2f;
    [SerializeField] private float _airControlMultiplier = 0.2f;
    [SerializeField] private float _minAirSpeed = 3f;

    [Header("WAHOO Jump Settings (MX form on WahooJump ground)")]
    [SerializeField] private float _wahooJumpForce = 18f;
    [SerializeField] private float _wahooVariableJumpHeightMultiplier = 0.4f;
    [SerializeField] private float _wahooFallGravityMultiplier = 2.5f;
    [SerializeField] private float _wahooAirControlMultiplier = 0.3f;
    [SerializeField] private float _wahooMinAirSpeed = 5f;

    private bool _jumpPressed;
    private bool _isJumping;
    private bool _isWahooJumping;
    private float _jumpStartVelocityX;
    private string _currentGroundTag = "";

    // sliding variables
    [Header("Sliding Settings")]
    [SerializeField] private float _slideDuration = 0.35f;
    [SerializeField] private float _landingSlideDuration = 0.4f;
    [SerializeField] private float _landingSpeedThreshold = 5f;
    private float _slideTimer = 0f;
    private float _lastDirection = 0f;
    private bool _hasTriggeredSlide = false;
    private bool _wasGrounded = true;
    private float _timeSinceLastJump = 0f;

    // crouch variables
    [Header("Crouch Settings")]
    [SerializeField] private float _crouchHeightReduction = 0.5f;
    private Vector2 _originalColliderSize;
    private Vector2 _originalColliderOffset;

    // PF Fall variables

    private bool _isFalling = false;

    // shared values
    public static Movement Instance { get; private set; }
    public bool _playerMoving;
    public bool _playerJumping;
    public bool _playerWahooJumping;
    public bool _playerSliding;
    public bool _playerIsGrounded;
    public bool _playerCrouching;
    public bool _playerRunning;
    public bool _playerFalling;
    public Vector2 _playerDirection;

    // ground check
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask _groundLayer;
    private bool _isGrounded;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();

        _originalColliderSize = _collider.size;
        _originalColliderOffset = _collider.offset;

        _rb.gravityScale = 1f;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        DirectionInput();
        JumpInput();
        CrouchInput();
        ShareValues();
        CheckGrounded();

        if (_timeSinceLastJump > 0) _timeSinceLastJump -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        MovementLogic();
        JumpPhysics();
        FallLogic();
    }

    private void ShareValues()
    {
        _playerMoving = _moving;
        _playerJumping = _jumping;
        _playerWahooJumping = _isWahooJumping;
        _playerSliding = _sliding;
        _playerIsGrounded = _isGrounded;
        _playerCrouching = _crouching;
        _playerRunning = _running;
        _playerDirection = _direction;
        _playerFalling = _isFalling;
    }

    private void MovementLogic()
    {
        // Prevent movement during standing transform
        if (Transforming.Instance != null && Transforming.Instance.playerIsTransforming &&
            Transforming.Instance.CurrentTransformType == Transforming.TransformType.Standing)
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);
            _moving = false;
            _running = false;
            return;
        }

        bool wasGroundedPrevious = _isGrounded;
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);

        // Landing slide detection
        if (!wasGroundedPrevious && _isGrounded)
        {
            float landingSpeed = Mathf.Abs(_rb.velocity.x);
            if (landingSpeed > _landingSpeedThreshold && _direction.x != 0)
            {
                bool isOppositeDirection = (_rb.velocity.x > 0 && _direction.x < 0) || (_rb.velocity.x < 0 && _direction.x > 0);
                if (isOppositeDirection)
                {
                    _sliding = true;
                    _slideTimer = _landingSlideDuration;
                }
            }
        }

        // Slide timer
        if (_sliding)
        {
            _slideTimer -= Time.fixedDeltaTime;
            if (_slideTimer <= 0) _sliding = false;
        }

        // Direction change slide
        if (_isGrounded && !_crouching && !_sliding)
        {
            bool isMovingSignificantly = Mathf.Abs(_rb.velocity.x) > 3f;
            if (_direction.x != 0 && _lastDirection != 0 && _lastDirection != _direction.x && isMovingSignificantly)
            {
                _sliding = true;
                _slideTimer = _slideDuration;
            }
        }

        if (_direction.x != 0) _lastDirection = _direction.x;

        float targetSpeed = 0f;
        float currentMaxSpeed;

        if (_direction.x != 0 && !_crouching && !_sliding)
        {
            _moving = true;
            _running = Input.GetKey(KeyCode.X);
            targetSpeed = _running ? _maxRunSpeed : _maxWalkSpeed;
            targetSpeed *= _direction.x;
            currentMaxSpeed = _running ? _maxRunSpeed : _maxWalkSpeed;
        }
        else if (_direction.x != 0 && _crouching)
        {
            _moving = true;
            _running = false;
            targetSpeed = _maxWalkSpeed * _direction.x;
            currentMaxSpeed = _maxWalkSpeed;
        }
        else
        {
            _moving = false;
            _running = false;
            currentMaxSpeed = _maxWalkSpeed;
        }

        // Air control – use Wahoo parameters if MX form and Wahoo jumping
        bool isMXForm = (Transforming.Instance != null && Transforming.Instance.playerCurrentForm == 3);
        bool useWahooAir = (isMXForm && _isWahooJumping);
        float currentAirControl = useWahooAir ? _wahooAirControlMultiplier : _airControlMultiplier;
        float currentMinAirSpeed = useWahooAir ? _wahooMinAirSpeed : _minAirSpeed;

        if (!_isGrounded && (_isJumping || _isWahooJumping))
        {
            if (_direction.x != 0)
            {
                bool movingWithVelocity = (_rb.velocity.x > 0 && _direction.x > 0) || (_rb.velocity.x < 0 && _direction.x < 0);
                bool isStandingJump = Mathf.Abs(_jumpStartVelocityX) < 0.5f;

                if (movingWithVelocity || isStandingJump)
                {
                    float airAcceleration = _acceleration * currentAirControl;
                    float maxAirSpeed = isStandingJump ? currentMinAirSpeed : Mathf.Abs(_jumpStartVelocityX);
                    _rb.velocity = new Vector2(
                        Mathf.MoveTowards(_rb.velocity.x, targetSpeed, airAcceleration * Time.fixedDeltaTime),
                        _rb.velocity.y
                    );
                    _rb.velocity = new Vector2(Mathf.Clamp(_rb.velocity.x, -maxAirSpeed, maxAirSpeed), _rb.velocity.y);
                }
                else
                {
                    float airDeceleration = _deceleration * 0.2f;
                    _rb.velocity = new Vector2(
                        Mathf.MoveTowards(_rb.velocity.x, 0, airDeceleration * Time.fixedDeltaTime),
                        _rb.velocity.y
                    );
                }
            }
        }
        else
        {
            // Ground movement
            if (_direction.x != 0)
            {
                float accelerationRate = _sliding ? _acceleration * 0.3f : _acceleration;
                _rb.velocity = new Vector2(
                    Mathf.MoveTowards(_rb.velocity.x, targetSpeed, accelerationRate * Time.fixedDeltaTime),
                    _rb.velocity.y
                );
            }
            else
            {
                if (_isGrounded)
                {
                    float frictionMultiplier = _sliding ? 0.99f : _groundFriction;
                    float newVelocityX = _rb.velocity.x * frictionMultiplier;
                    if (Mathf.Abs(newVelocityX) < 0.05f) newVelocityX = 0;
                    _rb.velocity = new Vector2(newVelocityX, _rb.velocity.y);
                }
            }
        }

        if (_isGrounded)
        {
            _rb.velocity = new Vector2(Mathf.Clamp(_rb.velocity.x, -currentMaxSpeed, currentMaxSpeed), _rb.velocity.y);
        }
    }

    private void FallLogic()
    {
        if (_isFalling)
        {
            if (_isWahooJumping)
            {
                _isFalling = false;
                return;
            }
        }
    }

    private void DirectionInput()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        if (horizontalInput > 0) _direction.x = 1;
        else if (horizontalInput < 0) _direction.x = -1;
        else _direction.x = 0;
    }

    private void JumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Z) && !_jumping && !_crouching && !_sliding && _isGrounded)
        {
            _jumpPressed = true;
            _jumpStartVelocityX = _rb.velocity.x;

            bool isMXForm = (Transforming.Instance != null && Transforming.Instance.playerCurrentForm == 3);
            bool canWahooJump = isMXForm && _currentGroundTag == "WahooJump";

            if (canWahooJump)
            {
                // Wahoo Jump for MX form on special ground
                _isWahooJumping = true;
                _isJumping = false;
                _jumping = true;
                _rb.velocity = new Vector2(_rb.velocity.x, _wahooJumpForce);
            }
            else
            {
                // Normal jump for FH, PF, or MX on normal ground
                _isJumping = true;
                _isWahooJumping = false;
                _jumping = true;
                _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce);
            }
            _timeSinceLastJump = 0.5f;
        }

        if (Input.GetKeyUp(KeyCode.Z) && _rb.velocity.y > 0)
        {
            if (_isJumping && !_isWahooJumping)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * _variableJumpHeightMultiplier);
                _isJumping = false;
            }
            else if (_isWahooJumping)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * _wahooVariableJumpHeightMultiplier);
                _isWahooJumping = false;
            }
        }
    }

    private void JumpPhysics()
    {
        if (_rb.velocity.y < 0)
        {
            if (_isJumping && !_isWahooJumping)
            {
                _rb.velocity += Vector2.up * Physics2D.gravity.y * (_fallGravityMultiplier - 1) * Time.fixedDeltaTime;
            }
            if (_isWahooJumping)
            {
                _rb.velocity += Vector2.up * Physics2D.gravity.y * (_wahooFallGravityMultiplier - 1) * Time.fixedDeltaTime;
            }
        }

        if (_isGrounded && _rb.velocity.y <= 0)
        {
            _jumping = false;
            _isJumping = false;
            _isWahooJumping = false;
            _jumpPressed = false;
        }

        if (_rb.velocity.y <= 0) _jumpPressed = false;
    }

    private void CrouchInput()
    {
        if ((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) && !_moving && _isGrounded && !_jumping && !_sliding)
        {
            if (!_crouching) Crouch(true);
        }
        else
        {
            if (_crouching && CanStandUp()) Crouch(false);
        }
    }

    private void Crouch(bool crouch)
    {
        _crouching = crouch;
        if (crouch)
        {
            _collider.size = new Vector2(_originalColliderSize.x, _originalColliderSize.y * _crouchHeightReduction);
            _collider.offset = new Vector2(_originalColliderOffset.x, _originalColliderOffset.y - (_originalColliderSize.y * (1 - _crouchHeightReduction) / 2));
        }
        else
        {
            _collider.size = _originalColliderSize;
            _collider.offset = _originalColliderOffset;
        }
    }

    private bool CanStandUp()
    {
        float checkDistance = _originalColliderSize.y - _collider.size.y;
        RaycastHit2D hit = Physics2D.BoxCast(transform.position + (Vector3)_collider.offset,
            new Vector2(_collider.size.x * 0.9f, checkDistance),
            0, Vector2.up, checkDistance, _groundLayer);
        return hit.collider == null;
    }

    private void CheckGrounded()
    {
        if (_groundCheck == null)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, _collider.bounds.extents.y + 0.1f, _groundLayer);
            _isGrounded = hit.collider != null;
            _currentGroundTag = _isGrounded ? hit.collider.tag : "";
        }
        else
        {
            Collider2D hit = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
            _isGrounded = hit != null;
            _currentGroundTag = _isGrounded ? hit.tag : "";
        }
    }

    public bool IsSliding() => _sliding;

    private void OnValidate()
    {
        if (_groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.parent = transform;
            groundCheckObj.transform.localPosition = new Vector3(0, -_collider.bounds.extents.y, 0);
            _groundCheck = groundCheckObj.transform;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("fallTrigger"))
        {

            if (!_isGrounded && !_jumping)
            {
                _isFalling = true;
            }
                       
        }
      
    }
}