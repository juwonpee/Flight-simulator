using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform player;
    public GameObject playerCamera;

    void Start()
    {
        player = GameObject.Find("Cessna 152").transform;
        playerCamera = GameObject.Find("Camera");
    }

    // Update is called once per frame
    void Update()
    {
        // Camera Controller
        //playerCamera.CameraController.target = player;
    }
}
