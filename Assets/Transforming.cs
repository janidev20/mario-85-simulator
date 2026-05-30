using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transforming : MonoBehaviour
{
    public enum TransformType { Moving, Standing }

    [SerializeField] private KeyCode _transformKey = KeyCode.T;

    [SerializeField] private int _currentForm = 0; // 0:FH, 1:transforming, 2:PF, 3:MX
    [SerializeField] private bool _canTransform = true;
    [SerializeField] private bool _isTransforming = false;

    [Header("Transform Settings")]
    [SerializeField] private float _movingTransformDuration = 0.5f;   // in?air transform (FH<->PF)
    [SerializeField] private float _standingTransformDuration = 1.2f; // grounded transform (FH->PF)

    private int _targetForm = 0;
    private int _originalForm = 0;
    private TransformType _currentTransformType = TransformType.Moving;

    public int playerCurrentForm = 0;
    public bool playerIsTransforming = false;
    public static Transforming Instance { get; private set; }

    public float TransformDuration => _currentTransformType == TransformType.Moving ? _movingTransformDuration : _standingTransformDuration;
    public int OriginalForm => _originalForm;
    public TransformType CurrentTransformType => _currentTransformType;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _currentForm = 0;
        _isTransforming = false;
        playerCurrentForm = 0;
        playerIsTransforming = false;
    }

    private void Update()
    {
        ManageTransform();
        SyncValues();
    }

    private void SyncValues()
    {
        playerCurrentForm = _currentForm;
        playerIsTransforming = _isTransforming;
    }

    private void ManageTransform()
    {
        if (Input.GetKeyDown(_transformKey) && _canTransform && !_isTransforming)
        {
            // Determine target form based on cycle: 0 -> 2 -> 3 -> 0
            if (_currentForm == 0) _targetForm = 2;  // FH ? PF
            else if (_currentForm == 2) _targetForm = 3;  // PF ? MX
            else if (_currentForm == 3) _targetForm = 0;  // MX ? FH
            else return;

            _originalForm = _currentForm;

            // Decide transform type only for FH<->PF transitions (others are instant)
            bool isInstant = (_originalForm == 2 && _targetForm == 3) || (_originalForm == 3 && _targetForm == 0);
            if (isInstant)
            {
                // Instant switch, no animation, no waiting
                _currentForm = _targetForm;
                Debug.Log($"Instant transform: {_originalForm} -> {_currentForm}");
                return;
            }

            // For FH<->PF transitions, use the existing animation logic
            bool isGrounded = Movement.Instance != null && Movement.Instance._playerIsGrounded;
            _currentTransformType = isGrounded ? TransformType.Standing : TransformType.Moving;
            Debug.Log($"Transform: {(_originalForm == 0 ? "FH->PF" : "PF->FH")}, Type: {_currentTransformType}");
            StartCoroutine(InitiateTransform());
        }
    }

    IEnumerator InitiateTransform()
    {
        _canTransform = false;
        _isTransforming = true;
        _currentForm = 1; // transforming state

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // If grounded (standing transform), stop movement immediately
        if (_currentTransformType == TransformType.Standing)
        {
            if (rb != null) rb.velocity = new Vector2(0, rb.velocity.y);
        }

        float duration = (_currentTransformType == TransformType.Moving) ? _movingTransformDuration : _standingTransformDuration;
        yield return new WaitForSeconds(duration);

        _currentForm = _targetForm;
        yield return null;

        _isTransforming = false;
        _canTransform = true;
        Debug.Log($"Transform complete. New form: {_currentForm}");
    }
}