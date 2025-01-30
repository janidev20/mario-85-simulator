using System.Collections;
using UnityEngine;

public class UICanvasManager : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private GameObject fadeIn;
    [SerializeField] private bool canExecute = true;
    [SerializeField] private float executeCoolDown = 0.5f;


    private void Update()
    {
        ManageFade();
    }

    void ManageFade()
    {
        FadeIn();
    }

    void FadeIn()
    {
        CheckForExecution();
    }

    void CheckForExecution()
    {
        if (canExecute && FormManager.instance.changedForm)
        {
           StartCoroutine(ExecuteFadeIn());
        }
    }

    IEnumerator ExecuteFadeIn()
    {
        canExecute = false;
        fadeIn.SetActive(true);

        yield return new WaitForSeconds(executeCoolDown);

        fadeIn.SetActive(false);
        canExecute = true;
    }
}
