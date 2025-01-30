using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game Intro")]
    public bool playerCanMove;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Update()
    {
        IntroEvents();
    }

    void IntroEvents()
    {
        EventPlayerMovementManage();
    }

    void EventPlayerMovementManage()
    {
        if (FormManager.instance.finishedProgressiveTransform)
        {
            playerCanMove = true;
        } else
        {
            playerCanMove = false;
        }
    }
}
