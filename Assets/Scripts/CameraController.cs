using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    // Update is called once per frame

    void Update()
    {
        transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);

    }

}

