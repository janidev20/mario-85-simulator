using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAttributes : MonoBehaviour
{
    [Header("Main")]
    public static PlayerAttributes instance;
    public float moveSpeed;
    public float maxSpeed;
    public float maxRunSpeed;
    public float jumpSpeed;

    [Header("False Hero")]
    public float FH_moveSpeed;
    public float FH_maxSpeed;
    public float FH_maxRunSpeed;
    public float FH_jumpSpeed;

    [Header("Pipe Crawler")]
    public float PC_moveSpeed;
    public float PC_maxSpeed;
    public float PC_maxRunSpeed;
    public float PC_jumpSpeed;

    [Header("MX")]
    public float MX_moveSpeed;
    public float MX_maxSpeed;
    public float MX_maxRunSpeed;
    public float MX_jumpSpeed;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Update()
    {
        ManageAttributes();
    }

    void ManageAttributes()
    {
        moveSpeed = FormManager.instance.isFH ? FH_moveSpeed : FormManager.instance.isPC ? PC_moveSpeed : MX_moveSpeed;
        maxSpeed = FormManager.instance.isFH ? FH_maxSpeed : FormManager.instance.isPC ? PC_maxSpeed : MX_maxSpeed;
        maxRunSpeed = FormManager.instance.isFH ? FH_maxRunSpeed : FormManager.instance.isPC ? PC_maxRunSpeed : MX_maxRunSpeed;
        jumpSpeed = FormManager.instance.isFH ? FH_jumpSpeed : FormManager.instance.isPC ? PC_jumpSpeed : MX_jumpSpeed;
    }
}
