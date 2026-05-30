using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    [SerializeField] private GameObject _src;

    private void Update()
    {
        ManageChaseTheme();
    }

    void ManageChaseTheme()
    {
        if (Transforming.Instance.playerCurrentForm == 0 || Movement.Instance._playerFalling)
        {
                _src.SetActive(false);
            
        }
        
        if (!Movement.Instance._playerFalling)
        {
            if (Transforming.Instance.playerCurrentForm == 2 || (Transforming.Instance.playerCurrentForm == 3 && Movement.Instance._playerWahooJumping))
            {
                _src.SetActive(true);
            }
        }
    }
}
