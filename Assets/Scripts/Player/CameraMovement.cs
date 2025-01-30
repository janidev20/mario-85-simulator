using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform player;

    private void Update()
    {
        // Camera follows the X position of the player. 
        transform.position = new Vector3(player.position.x, transform.position.y, transform.position.z);
    }
}
