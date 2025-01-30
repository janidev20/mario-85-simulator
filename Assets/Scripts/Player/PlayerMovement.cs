using System.Collections;
using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Animation Management")]
    [SerializeField] private bool facingRight;
    [SerializeField] private GameObject spriteHolder;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D plyCollider;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask playerMask;

    [Header("Physics")]
    [SerializeField] private float linearDrag;

    [Header("Horizontal Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxRunSpeed;
    [SerializeField] private bool isRunning;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isSliding;
    [SerializeField] private Vector2 direction;

    [Header("Vertical Movement")]
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float jumpTime = 0.65f;
    [SerializeField] private float jumpTimeCounter = 0;
    [SerializeField] private float bumpVelocity;
    [SerializeField] private float jumpCoolDownAmount = 0.85f;
    [SerializeField] private bool onGround;
    [SerializeField] private bool headBumped;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool isJumping;
    [SerializeField] private bool inTheAir => !onGround;
    [SerializeField] private Vector3 groundPosOffset, groundSizeOffset;

    [Header("Head Collider Values [SIZE, POS]")]
    [SerializeField] private Vector3 headColliderPosOffset, headColliderSizeOffset;
    [SerializeField] private Vector3 fhHeadColliderPosOffset, fhHeadColliderSizeOffset;
    [SerializeField] private Vector3 pcHeadColliderPosOffset, pcHeadColliderSizeOffset;
    [SerializeField] private Vector3 mxHeadColliderPosOffset, mxHeadColliderSizeOffset;

    [Header("Crouch")]
    [SerializeField] private bool isCrouching;
    [SerializeField] private Vector2 defColliderSize, defColliderOffset;
    [SerializeField] private Vector2 crouchColliderSize, crouchColliderOffset;

    [Header("Input")]
    [SerializeField] private KeyCode MoveLeft = KeyCode.LeftArrow;
    [SerializeField] private KeyCode MoveRight = KeyCode.RightArrow;
    [SerializeField] private KeyCode Crouch = KeyCode.DownArrow;
    [SerializeField] private KeyCode JumpKey = KeyCode.Y;
    [SerializeField] private KeyCode SprintKey = KeyCode.X;

    [Header("Public Booleans")]
    public bool chkOnGround => onGround;

    [Header("Animator Booleans")]
    public bool chkIsMoving => isMoving;
    public bool chkIsRunning => isRunning;
    public bool chkIsJumping => inTheAir;
    public bool chkIsSliding => isSliding;
    public bool chkIsCrouching => isCrouching;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        plyCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        HeadColliderSizeManage();
        GroundCheck();

        if (!GameManager.instance.playerCanMove)
            return;

        CheckForInput();
        ManagePlayerAttributes();
        Crouching();
        Movement();
    }

    private void FixedUpdate()
    {
        FixedMovement();
    }

    void ManagePlayerAttributes()
    {
        moveSpeed = PlayerAttributes.instance.moveSpeed;
        maxSpeed = PlayerAttributes.instance.maxSpeed;
        maxRunSpeed = PlayerAttributes.instance.maxRunSpeed;
        jumpSpeed = PlayerAttributes.instance.jumpSpeed;
    }

    void Movement()
    {
        if (isCrouching)
            return;

        HeadBumpCheck();
        HeadBumpDown();
        Jump();
        SprintingManage();
        SlidingManage();
    }

    void FixedMovement()
    {
        ModifyPhysics();
        MoveCharacter(direction.x);
    }

    void CheckForInput()
    {
        isCrouching = Input.GetKey(Crouch) && onGround && !isMoving ? true : false;

        if (!isCrouching)
        direction = Input.GetKey(MoveLeft) ? new Vector2(-1, 0) : Input.GetKey(MoveRight) ? new Vector2(1, 0) : new Vector2(0, 0);
    }

    void Crouching()
    {
        if (isCrouching)
        {
            ChangeHeight(isCrouching);
        }
    }

    void ChangeHeight(bool crouch)
    {
        plyCollider.offset = crouch ? defColliderOffset : crouchColliderOffset;
        plyCollider.size = crouch ? defColliderSize : crouchColliderSize;
    }

    void SprintingManage()
    {
        if (Input.GetKey(SprintKey))
        {
            if (!onGround && isRunning || onGround)
            {
                isRunning = true;
            }
        }
        else
        {
            isRunning = false;
        }
    }    

    void MoveCharacter(float horizontal)
    {
        rb.AddForce(Vector2.right * horizontal * moveSpeed);

        // Animation Management
        if ((horizontal > 0 && !facingRight) || (horizontal < 0 && facingRight))
        {
            // Flip() only when on ground.
            if (FormManager.instance.isFH)
            {
                if (!onGround)
                    return;
                Flip();
            } else
            {
                Flip();
            }
        }
    }

    void SlidingManage()
    {
        // Some Animation Debug for Sliding (Prevents slide animation stuttering)
        if (direction.x != 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        bool changingDirections = (direction.x > 0 && rb.linearVelocityX < -0.3f) || (direction.x < 0 && rb.linearVelocityX > 0.3f);

        // Slide Animation Boolean
        if (changingDirections || changingDirections && isMoving == false)
        {
            isSliding = true;
        }
        else
        {
            isSliding = false;
        }

        // If sliding, increase linear drag
        if (isSliding)
        {
            if (isRunning)
            {
                linearDrag = 2f;
            }
            else
                linearDrag = 1.6f;
        }
        else
        {
            linearDrag = 5;
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.rotation = Quaternion.Euler(0, facingRight ? 180 : 0, 0);
    }
    
    void ModifyPhysics()
    {

        // Cap the maxSpeed

        float _maxSpeed = isRunning ? maxRunSpeed : maxSpeed;

        bool changingDirections = (direction.x > 0 && rb.linearVelocityX < 0) || (direction.x < 0 && rb.linearVelocityX > 0);


        if (onGround)
        {
            if (Mathf.Abs(direction.x) < 0.4f || changingDirections)
            {
                rb.linearDamping = linearDrag;
            }
            else
            {
                rb.linearDamping = 2f;
            }
        } else
        {
            rb.linearDamping = linearDrag * 0.5f;
        }

        if (isMoving)
        {
            // FIX THE ACCELERATION ISSUE!! The horizontal movement part of the script is scrapped/empty for now.
        }
    }

    void Jump()
    {
        if (onGround)
        {
            // This basically resets the jumpTimeCounter.
            jumpTimeCounter = 0;
        } 

        if (jumpTimeCounter > jumpTime)
        {
            jumpTimeCounter = jumpTime;
            isJumping = false;
        }

        if (headBumped)
        {
            StartCoroutine(JumpCoolDown());
            return;
        }

        if (!canJump)
            return;

        if (Input.GetKeyDown(JumpKey))
        {
            if (!isJumping && onGround)
            {
                rb.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
                isJumping = true;
            }
        }
        if (Input.GetKey(JumpKey))
        {
            if (!onGround && isJumping)
            {
                if (jumpTimeCounter < jumpTime)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpSpeed);
                    jumpTimeCounter += Time.deltaTime;
                } else
                {
                    isJumping = false;
                }
            }
        }
        if (Input.GetKeyUp(JumpKey))
        {
            jumpTimeCounter = jumpTime;
            isJumping = false;
        }
    }

    // Used after headbump
    IEnumerator JumpCoolDown()
    {
        canJump = false;

        yield return new WaitForSeconds(jumpCoolDownAmount);

        canJump = true;
    }

    void GroundCheck()
    {
        onGround = Physics2D.BoxCast(transform.position + groundPosOffset, groundSizeOffset, transform.rotation.z, Vector2.down, 0, ~playerMask);
    }
    
    void HeadColliderSizeManage()
    {
        headColliderPosOffset = FormManager.instance.isFH ? fhHeadColliderPosOffset : FormManager.instance.isPC ? pcHeadColliderPosOffset : mxHeadColliderPosOffset;
        headColliderSizeOffset = FormManager.instance.isFH ? fhHeadColliderSizeOffset : FormManager.instance.isPC ? pcHeadColliderSizeOffset : mxHeadColliderSizeOffset;
    }

    // If hit a solid object (eg. block), bounce back to the ground 
    void HeadBumpDown()
    {
        if (headBumped)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, -bumpVelocity);
        }
    }

    void HeadBumpCheck()
    {
        headBumped = Physics2D.BoxCast(transform.position + headColliderPosOffset, headColliderSizeOffset, transform.rotation.z, Vector2.down, 0, ~playerMask);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position + groundPosOffset, groundSizeOffset);
        Gizmos.DrawCube(transform.position + headColliderPosOffset, headColliderSizeOffset);
    }
}
