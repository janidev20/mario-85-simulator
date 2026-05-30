using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    // components
    [SerializeField] private SpriteRenderer _playerSpriteRenderer;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Transform _playerBody;
    [SerializeField] private BoxCollider2D _playerCollider;

    // FH sprites
    [SerializeField] private Sprite _FHidleSprite;
    [SerializeField] private Sprite _FHjumpSprite;
    [SerializeField] private Sprite _FHslideSprite1;
    [SerializeField] private Sprite _FHslideSprite2;
    [SerializeField] private Sprite _FHcrouchSprite;
    [SerializeField] private Sprite[] _FHwalkSprites;

    // PF sprites
    [SerializeField] private Sprite _PFidleSprite;
    [SerializeField] private Sprite _PFjumpSprite;
    [SerializeField] private Sprite _PFslideSprite1;
    [SerializeField] private Sprite _PFslideSprite2;
    [SerializeField] private Sprite _PFcrouchSprite;
    [SerializeField] private Sprite _PFfallSprite;
    [SerializeField] private Sprite[] _PFwalkSprites;

    // MX sprites
    [SerializeField] private Sprite _MXidleSprite;
    [SerializeField] private Sprite _MXjumpSprite;
    [SerializeField] private Sprite _MXslideSprite1;
    [SerializeField] private Sprite _MXslideSprite2;
    [SerializeField] private Sprite _MXcrouchSprite;
    [SerializeField] private Sprite[] _MXwalkSprites;

    // Wahoo Jump animation sprites
    [SerializeField] private Sprite _wahooJumpAscentSprite;
    [SerializeField] private Sprite[] _wahooDescentSprites;
    [SerializeField] private Sprite _wahooLandSprite;

    // Current used sprites (set by form)
    private Sprite _idleSprite;
    private Sprite _jumpSprite;
    private Sprite _slideSprite1;
    private Sprite _slideSprite2;
    private Sprite _crouchSprite;
    private Sprite _fallSprite;
    private Sprite[] _walkSprites;

    // Transform sprites
    [SerializeField] private Sprite[] _transformSprites;
    [SerializeField] private Sprite[] _standingTransformSprites;

    // Transform animation control
    private int _transformFrame = 0;
    private float _transformElapsedTime = 0f;
    private bool _wasTransforming = false;
    private bool _standingTransformFlipX = false;
    private bool _isStandingTransformActive = false;
    private bool _colliderSwitched = false;
    private bool _originalFlipX = false;

    // Wahoo Jump animation states
    private enum WahooState { None, Ascent, Descent, Land }
    private WahooState _wahooState = WahooState.None;
    private int _descentFrame = 0;
    private float _wahooStateTimer = 0f;
    private bool _wahooLandPlayed = false;

    // Fall animation state


    [Header("Position Settings")]
    [SerializeField] private float _FHPositionY = 0.165f;
    [SerializeField] private float _PFPositionY = 0.239f;
    [SerializeField] private float _MXPositionY = 0.35f;

    // Collider settings
    [Header("FH Collider Settings")]
    [SerializeField] private Vector2 _FHColliderOffset = new Vector2(0.00393438339f, 0.237975538f);
    [SerializeField] private Vector2 _FHColliderSize = new Vector2(0.885808468f, 1.54971349f);

    [Header("PF Collider Settings")]
    [SerializeField] private Vector2 _PFColliderOffset = new Vector2(0.0039345026f, 0.306839466f);
    [SerializeField] private Vector2 _PFColliderSize = new Vector2(1.57444763f, 2.53102398f);

    [Header("MX Collider Settings")]
    [SerializeField] private Vector2 _MXClliderOffset = new Vector2(-0.0301758051f, 0.819046497f);
    [SerializeField] private Vector2 _MXClliderSize = new Vector2(2.25665307f, 3.55543804f);

    [Header("Standing Transform Settings")]
    [SerializeField] private float _colliderSwitchProgress = 0.5f;

    [SerializeField] private float _walkTimer = 0.2f;
    [SerializeField] private int _walkIndex = 0;

    [Header("Animation Speed")]
    [SerializeField] private float _minWalkSpeed = 1f;
    [SerializeField] private float _maxWalkSpeed = 8f;
    [SerializeField] private float _minAnimationTime = 0.05f;
    [SerializeField] private float _maxAnimationTime = 0.2f;
    [SerializeField] private float _velocityThreshold = 0.1f;

    private float _slideAnimationTimer = 0f;
    [SerializeField] private float _slideAnimationDuration = 0.35f;
    [SerializeField] private float _spriteChangeTime = 0.175f;

    private bool _lastSlidingState = false;

    private void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_playerBody == null) _playerBody = transform;
        if (_playerCollider == null) _playerCollider = GetComponent<BoxCollider2D>();
        this.enabled = true;
    }

    private void Start()
    {
        SetFHSpriteSet();
        UpdateBodyPosition();
        SetColliderToFH();
    }

    private void OnEnable()
    {
        if (Transforming.Instance != null)
        {
            UpdateSpriteSet();
            UpdateBodyPosition();
        }
    }

    private void Update()
    {
        if (Transforming.Instance != null)
        {
            UpdateSpriteSet();
            UpdateBodyPosition();
            UpdateCollider();
            AnimateSprite();
            FlipSprite();
        }
    }

    void SetColliderToFH()
    {
        _playerCollider.offset = _FHColliderOffset;
        _playerCollider.size = _FHColliderSize;
    }

    void SetColliderToPF()
    {
        _playerCollider.offset = _PFColliderOffset;
        _playerCollider.size = _PFColliderSize;
    }

    void SetColliderToMX()
    {
        _playerCollider.offset = _MXClliderOffset;
        _playerCollider.size = _MXClliderSize;
    }

    void UpdateCollider()
    {
        if (Transforming.Instance.playerIsTransforming) return;
        int form = Transforming.Instance.playerCurrentForm;
        if (form == 0) SetColliderToFH();
        else if (form == 2) SetColliderToPF();
        else if (form == 3) SetColliderToMX();
    }

    void UpdateBodyPosition()
    {
        int form = Transforming.Instance.playerCurrentForm;
        Vector3 newPos = _playerBody.localPosition;
        if (form == 0) newPos.y = _FHPositionY;
        else if (form == 2) newPos.y = _PFPositionY;
        else if (form == 3) newPos.y = _MXPositionY;
        _playerBody.localPosition = newPos;
    }

    void SetFHSpriteSet()
    {
        _idleSprite = _FHidleSprite;
        _jumpSprite = _FHjumpSprite;
        _slideSprite1 = _FHslideSprite1;
        _slideSprite2 = _FHslideSprite2;
        _crouchSprite = _FHcrouchSprite;
        if (_FHwalkSprites != null && _FHwalkSprites.Length > 0)
        {
            _walkSprites = new Sprite[_FHwalkSprites.Length];
            for (int i = 0; i < _FHwalkSprites.Length; i++)
                _walkSprites[i] = _FHwalkSprites[i];
        }
    }

    void SetPFSpriteSet()
    {
        _idleSprite = _PFidleSprite;
        _jumpSprite = _PFjumpSprite;
        _slideSprite1 = _PFslideSprite1;
        _slideSprite2 = _PFslideSprite2;
        _crouchSprite = _PFcrouchSprite;
        _fallSprite = _PFfallSprite;
        if (_PFwalkSprites != null && _PFwalkSprites.Length > 0)
        {
            _walkSprites = new Sprite[_PFwalkSprites.Length];
            for (int i = 0; i < _PFwalkSprites.Length; i++)
                _walkSprites[i] = _PFwalkSprites[i];
        }
    }

    void SetMXSpriteSet()
    {
        _idleSprite = _MXidleSprite;
        _jumpSprite = _MXjumpSprite;
        _slideSprite1 = _MXslideSprite1;
        _slideSprite2 = _MXslideSprite2;
        _crouchSprite = _MXcrouchSprite;
        if (_MXwalkSprites != null && _MXwalkSprites.Length > 0)
        {
            _walkSprites = new Sprite[_MXwalkSprites.Length];
            for (int i = 0; i < _MXwalkSprites.Length; i++)
                _walkSprites[i] = _MXwalkSprites[i];
        }
    }

    void UpdateSpriteSet()
    {
        if (Transforming.Instance.playerIsTransforming) return;
        int form = Transforming.Instance.playerCurrentForm;
        if (form == 0) SetFHSpriteSet();
        else if (form == 2) SetPFSpriteSet();
        else if (form == 3) SetMXSpriteSet();
    }

    void AnimateSprite()
    {
        // 1. Wahoo Jump animation (only for MX form when Wahoo jumping)
        bool isMX = (Transforming.Instance.playerCurrentForm == 3);
        if (isMX && Movement.Instance._playerWahooJumping)
        {
            HandleWahooJumpAnimation();
            return;
        }
        if (_wahooState != WahooState.None)
        {
            _wahooState = WahooState.None;
            _descentFrame = 0;
            _wahooLandPlayed = false;
        }

        // 2. Transform handling
        if (Transforming.Instance.playerIsTransforming != _wasTransforming)
        {
            if (Transforming.Instance.playerIsTransforming)
            {
                _transformElapsedTime = 0f;
                _colliderSwitched = false;
                if (Transforming.Instance.CurrentTransformType == Transforming.TransformType.Standing &&
                    Transforming.Instance.OriginalForm == 0)
                {
                    _isStandingTransformActive = true;
                    _standingTransformFlipX = !_playerSpriteRenderer.flipX;
                    SetColliderToFH();
                }
            }
            else
            {
                if (_isStandingTransformActive)
                {
                    _isStandingTransformActive = false;
                    _playerSpriteRenderer.flipX = !_playerSpriteRenderer.flipX;
                    SetColliderToPF();
                }
            }
            _wasTransforming = Transforming.Instance.playerIsTransforming;
        }

        if (Transforming.Instance.playerIsTransforming)
        {
            HandleTransformAnimation();
            return;
        }

        _transformFrame = 0;
        _transformElapsedTime = 0f;
        UpdateCollider();

        // 3. Airborne state (not grounded) – always show jump sprite (unless Wahoo active, already handled)
        if (!Movement.Instance._playerIsGrounded)
        {
            if (Movement.Instance._playerFalling)
            {
                _playerSpriteRenderer.sprite = _fallSprite;
                return;
            } else
            {
                _playerSpriteRenderer.sprite = _jumpSprite;
                return;
            }
        }

        // 4. Grounded animations
        // Sliding
        if (Movement.Instance._playerSliding)
        {
            HandleSlideAnimation();
            _slideAnimationTimer = _slideAnimationDuration;
            return;
        }

        if (_slideAnimationTimer > 0)
        {
            _slideAnimationTimer -= Time.deltaTime;
            if (_slideAnimationTimer <= 0)
            {
                _walkIndex = 0;
                _walkTimer = 0.1f;
            }
            else
            {
                HandleSlideAnimation();
                return;
            }
        }

        // Crouching
        if (Movement.Instance._playerCrouching)
        {
            _playerSpriteRenderer.sprite = _crouchSprite;
            return;
        }

        // Moving or idle
        bool hasVelocity = Mathf.Abs(_rb.velocity.x) > _velocityThreshold;
        if (hasVelocity)
            WalkAnimation();
        else
        {
            _playerSpriteRenderer.sprite = _idleSprite;
            _walkIndex = 0;
            _walkTimer = 0.1f;
        }
    }

    void HandleWahooJumpAnimation()
    {
        if (_wahooState == WahooState.None)
        {
            _wahooState = WahooState.Ascent;
            _wahooStateTimer = 0f;
        }

        float verticalVelocity = _rb.velocity.y;

        if (_wahooState == WahooState.Ascent)
        {
            _playerSpriteRenderer.sprite = _wahooJumpAscentSprite;
            if (verticalVelocity < 0)
            {
                _wahooState = WahooState.Descent;
                _descentFrame = 0;
                _wahooStateTimer = 0f;
            }
        }
        else if (_wahooState == WahooState.Descent)
        {
            if (_wahooDescentSprites != null && _wahooDescentSprites.Length >= 2)
            {
                _wahooStateTimer += Time.deltaTime;
                float frameDuration = 0.1f;
                int frame = Mathf.FloorToInt(_wahooStateTimer / frameDuration);
                if (frame >= 2) frame = 1;
                _playerSpriteRenderer.sprite = _wahooDescentSprites[frame];
            }
            if (Movement.Instance._playerIsGrounded && !_wahooLandPlayed)
            {
                _wahooState = WahooState.Land;
                _wahooStateTimer = 0f;
                _wahooLandPlayed = true;
            }
        }
        else if (_wahooState == WahooState.Land)
        {
            _playerSpriteRenderer.sprite = _wahooLandSprite;
            _wahooStateTimer += Time.deltaTime;
            if (_wahooStateTimer >= 0.05f)
            {
                _wahooState = WahooState.None;
            }
        }
    }

    void HandleTransformAnimation()
    {
        bool isMovingTransform = (Transforming.Instance.CurrentTransformType == Transforming.TransformType.Moving);
        bool isReversing = (isMovingTransform && Transforming.Instance.OriginalForm == 2);
        if (!isMovingTransform && Transforming.Instance.OriginalForm == 2) return;

        Sprite[] sprites = isMovingTransform ? _transformSprites : _standingTransformSprites;
        if (sprites == null || sprites.Length == 0) return;

        float frameDuration = Transforming.Instance.TransformDuration / sprites.Length;
        _transformElapsedTime += Time.deltaTime;
        float totalDuration = Transforming.Instance.TransformDuration;
        if (_transformElapsedTime > totalDuration) _transformElapsedTime = totalDuration;

        if (!isMovingTransform && _isStandingTransformActive && !_colliderSwitched)
        {
            float progress = _transformElapsedTime / totalDuration;
            if (progress >= _colliderSwitchProgress)
            {
                SetColliderToPF();
                _colliderSwitched = true;
            }
        }

        int frameIndex = Mathf.FloorToInt(_transformElapsedTime / frameDuration);
        frameIndex = Mathf.Clamp(frameIndex, 0, sprites.Length - 1);
        int displayIndex = isReversing ? (sprites.Length - 1) - frameIndex : frameIndex;
        _playerSpriteRenderer.sprite = sprites[displayIndex];

        if (_isStandingTransformActive)
            _playerSpriteRenderer.flipX = _standingTransformFlipX;
    }

    void HandleSlideAnimation()
    {
        float slideTimeElapsed = _slideAnimationDuration - _slideAnimationTimer;
        _playerSpriteRenderer.sprite = (slideTimeElapsed < _spriteChangeTime) ? _slideSprite1 : _slideSprite2;
    }

    void WalkAnimation()
    {
        float currentSpeed = Mathf.Abs(_rb.velocity.x);
        float animationTime = CalculateAnimationTime(currentSpeed);
        _walkTimer -= Time.deltaTime;
        if (_walkTimer <= 0)
        {
            if (_walkSprites.Length > 0 && _walkIndex < _walkSprites.Length)
                _playerSpriteRenderer.sprite = _walkSprites[_walkIndex];
            _walkIndex++;
            if (_walkIndex >= _walkSprites.Length) _walkIndex = 0;
            _walkTimer = animationTime;
        }
    }

    float CalculateAnimationTime(float currentSpeed)
    {
        if (currentSpeed <= _minWalkSpeed) return _maxAnimationTime;
        if (currentSpeed >= _maxWalkSpeed) return _minAnimationTime;
        float t = (currentSpeed - _minWalkSpeed) / (_maxWalkSpeed - _minWalkSpeed);
        return Mathf.Lerp(_maxAnimationTime, _minAnimationTime, t);
    }

    private void FlipSprite()
    {
        if (_isStandingTransformActive) return;
        if (Movement.Instance._playerDirection.x > 0)
            _playerSpriteRenderer.flipX = false;
        else if (Movement.Instance._playerDirection.x < 0)
            _playerSpriteRenderer.flipX = true;
    }
}