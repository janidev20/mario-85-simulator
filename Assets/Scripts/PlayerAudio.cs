using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    // components
    [Header("Components")]
    [SerializeField] private AudioSource _source;

    // player audio clips
    [Header("Player Audio Clips")]
    [SerializeField] private AudioClip _fhJump;
    [SerializeField] private AudioClip _pfJump;
    [SerializeField] private AudioClip _fastTransformToPF; // Sound when transforming from FH to PF
    [SerializeField] private AudioClip _standTransformToPF; // Sound when transforming from FH to PF
    [SerializeField] private AudioClip _transformToFH; // Sound when transforming from PF to FH
    [SerializeField] private AudioClip _transformLaugh; // Sound when transforming from PF to FH
    [SerializeField] private AudioClip _wahooJump; // Sound when transforming from PF to FH
    [SerializeField] private AudioClip _fallScream; // Sound when transforming from PF to FH

    private bool _jumpPlayed = false;
    private bool _transformPlayed = false;
    private bool _fallPlayed = false;
    private int _lastForm = 0; // Track the last form to detect transform direction

    private void Update()
    {
        JumpAudio();
        TransformAudio();
        FallAudio();
    }

    private void JumpAudio()
    {
        // Play jump sound exactly once when jump starts
        

        if (Movement.Instance._playerWahooJumping && !_jumpPlayed)
        {
            if (Transforming.Instance.playerCurrentForm == 0)
            {
                _source.PlayOneShot(_fhJump);
            }
            else
            {
                _source.PlayOneShot(_pfJump);
                _source.PlayOneShot(_wahooJump);
            }
            _jumpPlayed = true;
        } else if (Movement.Instance._playerJumping && !_jumpPlayed)
        {
            if (Transforming.Instance.playerCurrentForm == 0)
            {
                _source.PlayOneShot(_fhJump);
            }
            else
            {
                _source.PlayOneShot(_pfJump);
            }
            _jumpPlayed = true;
        }

        // Reset flag when no longer jumping (grounded or falling)
        if (!Movement.Instance._playerJumping)
        {
            _jumpPlayed = false;
        }
    }

    private void FallAudio()
    {
        if (Movement.Instance._playerFalling && !_fallPlayed)
        {
                _source.PlayOneShot(_fallScream);
            _fallPlayed = true;
        }
       

        // Reset flag when no longer jumping (grounded or falling)
        if (!Movement.Instance._playerFalling)
        {
            _fallPlayed = false;
        }
    }

    private void TransformAudio()
    {
        // Check if currently transforming
        if (Transforming.Instance.playerIsTransforming && !_transformPlayed)
        {
            // Determine which sound to play based on the original form
            if (Transforming.Instance.OriginalForm == 0)
            {
                // Transforming from FH to PF
                if (_fastTransformToPF!= null)
                {

                    if (Movement.Instance._playerIsGrounded)
                    {
                        _source.PlayOneShot(_transformLaugh);
                        _source.PlayOneShot(_standTransformToPF);
                    } else
                    {
                        _source.PlayOneShot(_fastTransformToPF);
                    }

                        Debug.Log("Playing FH -> PF transform sound");
                }
            }

            else if (Transforming.Instance.OriginalForm == 2 && !_transformPlayed)
            {
                // Transforming from PF to FH
                if (_transformToFH != null)
                {
                    _source.PlayOneShot(_transformToFH);
                    
                    Debug.Log("Playing PF -> FH transform sound");
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
}