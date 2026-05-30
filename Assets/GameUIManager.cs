using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private Animator _cameraAnimator;
    [SerializeField] private GameObject _blinkRed;
    [SerializeField] private GameObject _blinkBlack;
    private bool _transformPlayed = false;

    private void Awake()
    {
        _blinkRed.SetActive(false);
        _blinkBlack.SetActive(false);
    }

    private void Update()
    {
        ManageTransformUIAnimations();
    }

    void ManageTransformUIAnimations()
    {
        if (Transforming.Instance.playerIsTransforming && !_transformPlayed)
        {
            // Determine which sound to play based on the original form
            if (Transforming.Instance.OriginalForm == 0)
            {
                // Transforming from FH to PF
                _cameraAnimator.SetTrigger("camerashake");
                StartCoroutine("Blink");
            }
            else if (Transforming.Instance.OriginalForm == 2 && !_transformPlayed)
            {
                
                _cameraAnimator.SetTrigger("camerashake");

                if (Movement.Instance._playerIsGrounded)
                {
                    StartCoroutine("BlinkBlack");
                }
                else
                {
                    StartCoroutine("Blink");
                }
            }
            _transformPlayed = true;
        }

        // Reset flag when transformation is complete
        if (!Transforming.Instance.playerIsTransforming)
        {
            _transformPlayed = false;
        }
    }

    IEnumerator Blink()
    {
        _blinkRed.SetActive(true);

        yield return new WaitForSeconds(3f);

        _blinkRed.SetActive(false);
    }

    IEnumerator BlinkBlack()
    {
        _blinkBlack.SetActive(true);

        yield return new WaitForSeconds(3f);

        _blinkBlack.SetActive(false);
    }
}
