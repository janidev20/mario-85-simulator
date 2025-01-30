using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class SoundManager : MonoBehaviour
{
    [Header("Movement Script")]
    [SerializeField] PlayerMovement movementScript;
    
    [Header("Audio Source")]
    [SerializeField] AudioSource source;

    [Header("Music")]
    [SerializeField] private AudioSource crawlerTransforming;
    [SerializeField] private AudioSource chaseMusic;

    [Header("Player Sounds")]
    [SerializeField] AudioClip fhJumpSound, mxJumpSound;
    bool didJumpPlay = false;

    [Header("Voice")]
    [SerializeField] AudioClip crawlerLaugh;
    bool playedLaugh = false;

    [Header("Environment")]
    [SerializeField] AudioClip blockBreak;
    bool playedBreak = false;

    [Header("Meme")]
    private bool saidIt = false;
    [SerializeField] AudioClip imAnOrange;
    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    private void Update()
    {
        SFXManage();
        MusicManage();
    }

    void SFXManage()
    {
        IntroTransformation();
        PlayerSounds();
        MemeSounds();
    }

    void IntroTransformation()
    {
        TransformStepSound();
        CrawlerTransformingSounds();
    }

    void MusicManage()
    {
        ManageChaseMusic();
    }

    void PlayerSounds()
    {
        JumpSound();
    }

    void MemeSounds()
    {
        if (!FormManager.instance.memeAllowed)
            return;

            SayImAnOrange();
        
    }

    void TransformStepSound()
    {
        if (FormManager.instance.tookStep && !playedBreak)
        {
            source.PlayOneShot(blockBreak);
            playedBreak = true;
        }

        if (!FormManager.instance.tookStep)
        {
            playedBreak = false;
        }
    }

    void ManageChaseMusic()
    {
        chaseMusic.enabled = FormManager.instance.isFH && FormManager.instance.step <= 3 ? false : true;
    }

    void CrawlerTransformingSounds()
    {
        if (FormManager.instance.step >= 1 && FormManager.instance.step <= 3)
        {
            playedLaugh = false;
            crawlerTransforming.enabled = true;
        }
        else if (FormManager.instance.step == 4)
        {
            crawlerTransforming.enabled = false;
            if (!playedLaugh)
            {
                source.PlayOneShot(crawlerLaugh);
                playedLaugh = true;
            }
        }
    }

    void JumpSound()
    {
        if (movementScript.chkIsJumping)
        {
            if (!didJumpPlay)
            {
                source.PlayOneShot(FormManager.instance.isFH ? fhJumpSound : mxJumpSound);
                didJumpPlay = true;
            }
        }

        else
        {
            didJumpPlay = false;
        }
    }

    void SayImAnOrange()
    {
        if (FormManager.instance.isOrange)
        {
            if (!saidIt)
            {
                source.PlayOneShot(imAnOrange);
                saidIt = true;
            }
        }
    }
}
