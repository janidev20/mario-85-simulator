using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // components
    [Description("Components")]
    [SerializeField] Transform _playerTransform;
    [SerializeField] Transform _cameraTransform;

    private void Start()
    {
        _cameraTransform = GetComponent<Transform>();
    }

    private void Update()
    {
        FollowPlayerX();
    }

    void FollowPlayerX()
    {
        _cameraTransform.position = new Vector3(_playerTransform.position.x, _cameraTransform.position.y, _cameraTransform.position.z);
    }
}
