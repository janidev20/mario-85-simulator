using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoombaAudio : MonoBehaviour
{
    [SerializeField] AudioSource _src;
    [SerializeField] AudioClip _goombaDeath;

    [SerializeField] GoombaMovement _movement;

    bool _playedDeath = false;

    private void Update()
    {
        HandleGoombaDeathAudio();
    }

    void HandleGoombaDeathAudio()
    {
        if (_movement._goombaIsDead && !_playedDeath)
        {
            _src.PlayOneShot(_goombaDeath);
            _playedDeath = true;
        }
    }
}
