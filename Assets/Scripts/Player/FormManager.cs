using Mono.Cecil.Cil;
using System.Collections;
using System.Linq;
using UnityEngine;

public class FormManager : MonoBehaviour
{
    public static FormManager instance;

    [Header("Progressive Transform")]
    // If the transform mode is progressive, player needs to hit the (Transform) key each time to complete the transform state.
    [SerializeField] private float stepCoolDownAmount = 1f;
    [SerializeField] bool isProgressive;
    // Bunch of variables for progressive transform at the beginning of the intro (FH transforms for the first time in front of Lucas.)
    public bool manageProgress;
    public bool canProceed;
    public bool tookStep;
    public int step = 0;
    public bool finishedProgressiveTransform = false; // Once this is true, you cannot transform back to FH

    [Header("CharacterStatus")]
    // These are booleans to manage transforming state (ex. FH -> PC, PC -> MX)
    public bool isTransforming;
    public bool isFH;
    public bool isPC;
    public bool isMX;
    public bool changedForm;

    [Header("MemeState")]
    // Just made this for showcasing on Discord. Keep it in or not someway, up to you.
    public bool isOrange;
    public bool memeAllowed;

    [Header("Input")]
    [SerializeField] private KeyCode Transform = KeyCode.C;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Update()
    {
        ManageForm();
    }

    void ManageForm()
    {
        if (!isProgressive)
        {
            ChangeForm();
        }

        else if (!finishedProgressiveTransform)
        {
            CheckForProgressiveTransform();
            if (!manageProgress)
            {
                ChangeForm();
            }
            else
            {
                ManageProgressiveTransforming();
            }
        }
    }
    void ChangeForm()
    {
        if (Input.GetKeyDown(Transform))
        {
            // This (isFH) most likely won't be used in the actual Story Mode.
            if (isFH)
            {
                ChangeToPC();
                StartCoroutine(SendFormChanged());
            }

            else if (isPC)
            {
                // Change to MX
                ChangeToMX();
                StartCoroutine(SendFormChanged());
            }

            else if (isMX)
            {
                if (!memeAllowed)
                {
                    // Change back to PC
                    ChangeToPC();
                } else
                {
                    // Change to OrangeMX
                    ChangeToOrangeMX();
                }
                StartCoroutine(SendFormChanged());
            }

            
        }
    }

    // If the player is in False Hero form, manage the progressive transform.
    void CheckForProgressiveTransform()
    {
        manageProgress = isFH ? true : false;
    }

    void ManageProgressiveTransforming()
    {
            if (!isTransforming)
            {
                // The very first step to transforming into PC after pressing the Transform key.
                StartTransformingToPC();
            }
            else if (isTransforming)
            {
                
                TransformToPC();
            }
    }
   
    void StartTransformingToPC()
    {
        if (!Input.GetKeyDown(Transform))
            return;

        step += 1;
        StartCoroutine(StepCoolDown());
        isTransforming = true;
    }

    void TransformToPC()
    {
        // As the Transform key gets pressed continously, the transform progresses step by step.

        if (step >= 1 && step <= 3)
        {
            if (Input.GetKeyDown(Transform))
            {
                if (canProceed)
                {
                    step += 1;
                    StartCoroutine(StepCoolDown());
                }
            }
        }

        // On the last step, reset a bunch of variables, this way we don't track the progressive transform unnecessarily (it would cause problems.)
        else if (step == 4)
        {
            StartCoroutine(StopProgressiveTransform());
        }
    }

    // Currently not used, though WILL be used in the PRE-INTRO (CASTLE -> FORMS TESTING)
    void ChangeToFH()
    {
        isFH = true;
        isPC = false;
        isMX = false;
        isOrange = false;
    }

    void ChangeToPC()
    {
        isFH = false;
        isPC = true;
        isMX = false;
        isOrange = false;
    }

    void ChangeToMX()
    {
        isFH = false;
        isPC = false;
        isMX = true;
        isOrange = false;
    }

    void ChangeToOrangeMX()
    {
        isFH = false;
        isPC = false;
        isMX = false;
        isOrange = true;
    }

    // Used in the UICanvasManager.cs for Fade Effect
    IEnumerator SendFormChanged()
    {
        changedForm = true;

        yield return new WaitForSeconds(0.25f);

        changedForm = false;
    }

    IEnumerator StopProgressiveTransform()
    {
        // Why 0.35f? It's basically around the same value (in seconds) as the length of the last transformation animation clip to sync the movement correctly.
        yield return new WaitForSeconds(0.35f);

        step = 0;
        ChangeToPC();
        finishedProgressiveTransform = true;
        isTransforming = false;
        isProgressive = false;
        manageProgress = false;

        yield return new WaitForEndOfFrame();
    }

    IEnumerator StepCoolDown()
    {
        tookStep = true;
        canProceed = false;

        yield return new WaitForSeconds(stepCoolDownAmount);

        tookStep = false;
        canProceed = true;
    }
}
