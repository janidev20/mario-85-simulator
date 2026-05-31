using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoombaMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Collider2D _collider;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Animator _goombaAnimator;

    [Header("Movement Variables")]
    [SerializeField] private float _moveSpeed = 3.0f;
    [SerializeField] private Vector2 direction = Vector2.zero;

    bool directionNeedsToChange = false;
    private bool _isDead = false;
    public bool _goombaIsDead = false;

    private void Start()
    {
        // change direction to 1 (right)
        direction = new Vector2(1, 0);

        _collider = GetComponent<Collider2D>();
        _rb = GetComponent<Rigidbody2D>();
    }

    void ShareValues()
    {
        _goombaIsDead = _isDead;
    }

    private void Update()
    {
        ShareValues();
        HandleDeathLogic();
    }

    private void FixedUpdate()
    {
        MovementBehaviour();
        DirectionLogic();
    }

    void HandleDeathLogic()
    {
        if (_isDead)
        {
            StartCoroutine("DeathLogic");
        }
    }

    IEnumerator DeathLogic()
    {
        _isDead = false;
        _collider.enabled = false;
        _goombaAnimator.SetTrigger("kill");

        yield return new WaitForSeconds(3);

        Destroy(this.gameObject);
    }

    void MovementBehaviour()
    {
        _rb.velocity = new Vector2(direction.x * _moveSpeed * Time.deltaTime, _rb.velocity.y);
    }

    void DirectionLogic()
    {
        if (directionNeedsToChange)
        {
            if (direction.x > 0)
            {
                direction = new Vector2(-1, 0);
                directionNeedsToChange = false;
            } else if (direction.x < 0)
            {
                direction = new Vector2(1, 0);
                directionNeedsToChange = false;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Wall"))
        {
            directionNeedsToChange = true;
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") && (Transforming.Instance.playerCurrentForm == 2 || Transforming.Instance.playerCurrentForm == 3))
        {
            _isDead = true;
        }
    }
}   
