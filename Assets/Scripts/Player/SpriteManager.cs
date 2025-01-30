using UnityEditor.Animations;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class SpriteManager : MonoBehaviour
{
    [Header("Movement Script")]
    public PlayerMovement movementScript;

    [Header("Animation Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private AnimatorController falseHeroAC, crawlerAC, mxAC, orangeAC;
    [SerializeField] private AnimatorController heroToCrawler;

    [Header("Transform - [False Hero to Pipe Crawler]")]
    [SerializeField] private string step2 = "2", step3 = "3", step4 = "4"; 

    [Header("Parameters")]
    public bool isMoving => movementScript.chkIsMoving;
    public bool isRunning => movementScript.chkIsRunning;
    public bool isJumping => movementScript.chkIsJumping;
    public bool isSliding => movementScript.chkIsSliding;
    public bool isCrouching => movementScript.chkIsCrouching;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        ManageAnimationSpeed();
        CharacterChange();
        AnimationParameters();
    }

    void AnimationParameters()
    {
        Movement();
    }

    void CharacterChange()
    {
        if (!FormManager.instance.manageProgress)
        {
            InstantChange();
        } else
        {
            ProgressiveChange();
        }
    }

    void ProgressiveChange()
    {

        if (FormManager.instance.isFH)
        {
            ProgressToPC();
        }
    }

    void InstantChange()
    {
        InstantChangeAC();
    }

    void ProgressToPC()
    {
        // Manage the Animation Controller from False Hero -> to the Transforming AC -> to Pipe Crawler AC
        animator.runtimeAnimatorController = FormManager.instance.step == 0 ? falseHeroAC : FormManager.instance.step >= 1 && FormManager.instance.step <= 4 ? heroToCrawler : crawlerAC;

        if (FormManager.instance.isTransforming)
        {
            // While having the Transforming AC, set the step triggers to change the transform animation clip.
            animator.SetTrigger(FormManager.instance.step == 2 ? step2 : FormManager.instance.step == 3 ? step3 : FormManager.instance.step == 4 ? step4 : "");
        }
    }

    void InstantChangeAC()
    {
        if (!FormManager.instance.memeAllowed)
        {
            animator.runtimeAnimatorController = FormManager.instance.isFH ? falseHeroAC : FormManager.instance.isPC ? crawlerAC : mxAC;
        }
        else
        {
            animator.runtimeAnimatorController = FormManager.instance.isFH ? falseHeroAC : FormManager.instance.isPC ? crawlerAC : FormManager.instance.isMX ? mxAC : orangeAC;
        }

    }

    void Movement()
    {
        WalkAnimation();
        JumpAnimation();
        SlideAnimation();
        CrouchAnimation();
    }

    void WalkAnimation()
    {
        if (isMoving)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }

    void ManageAnimationSpeed()
    {
        // Smoothly transition from walking to sprinting.
        // Basically the animation speeds up and slows down based on checking whether the player is running or not.
        
        if (isMoving && isRunning)
        {
            if (animator.speed < 2.65f)
            animator.speed += 0.01f;
        }

        else if (isMoving && !isRunning)
        {
            if (animator.speed > 1)
            animator.speed -= 0.01f;
        } 
        
        else
        {
            animator.speed = 1;
        }
    }

    void JumpAnimation()
    {
        if (isJumping)
        {
            animator.SetBool("isJumping", true);
        } else
        {
            animator.SetBool("isJumping", false);
        }
    }

    void SlideAnimation()
    {
        if (isSliding)
        {
            animator.SetBool("isSliding", true);
        }
        else
        {
            animator.SetBool("isSliding", false);
        }
    }

    void CrouchAnimation()
    {
        if (isCrouching)
        {
            animator.SetBool("isCrouching", true);
        }
        else
        {
            animator.SetBool("isCrouching", false);
        }
    }
}
