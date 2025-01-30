using UnityEngine;
using UnityEngine.Pool;

public class PlayerColliderSizeManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private BoxCollider2D plyCollider;

    [Header("Check Character")]
    [SerializeField] private bool isFH => FormManager.instance.isFH;
    [SerializeField] private bool isPC => FormManager.instance.isPC;
    [SerializeField] private bool isMX => FormManager.instance.isMX;
    
    [Header("Main Collider Values [SIZE, POSITION]")]
    [SerializeField] private Vector2 positionOffset;
    [SerializeField] private Vector2 sizeOffset;

    [Header("FH Collider Values [SIZE, POSITION]")]
    [SerializeField] private Vector2 FHPositionOffset;
    [SerializeField] private Vector2 fhSizeOffset;

    [Header("PC Collider Values [SIZE, POSITION]")]
    [SerializeField] private Vector2 pcPositionOffset;
    [SerializeField] private Vector2 pcSizeOffset;

    [Header("MX Collider Values [SIZE, POSITION]")]
    [SerializeField] private Vector2 mxPositionOffset;
    [SerializeField] private Vector2 mxSizeOffset;

    private void Start()
    {
        plyCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        ManageCollider();
    }

    void ManageCollider()
    {
        positionOffset = isFH ? FHPositionOffset : isPC ? pcPositionOffset : mxPositionOffset;
        sizeOffset = isFH ? fhSizeOffset : isPC ? pcSizeOffset : mxSizeOffset;

        plyCollider.offset = positionOffset;
        plyCollider.size = sizeOffset;
    }
}
