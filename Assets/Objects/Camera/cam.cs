using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cam : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 offset;
    public Vector3 look_angle_max;
    public float camera_sensitivity;
    public float lag_sensitivity;
    Vector3 mouse_position;

    Transform main_cam;
    void Start()
    {
        main_cam = GameObject.Find("Main Camera").GetComponent<Transform>();

    }

    // Update is called once per frame
    void Update() {
        // Get inputs for X and Y axis
        if (Input.GetMouseButton(0)) mouse_position = Vector3.Lerp(mouse_position, new Vector3((Input.mousePosition.x - Screen.width / 2) / Screen.width, (Input.mousePosition.y - Screen.height / 2) / Screen.height, 0), Time.deltaTime * camera_sensitivity);
        else mouse_position = Vector3.Lerp(mouse_position, new Vector3(0, 0, 0), Time.deltaTime * camera_sensitivity);

        // Use angular lag velocity
        Vector3 lagRotation = transform.InverseTransformDirection(transform.parent.GetComponent<Rigidbody>().angularVelocity) * lag_sensitivity;

        // Position
        var targetLookAngle = Vector2.Scale(mouse_position, look_angle_max);

        // X and Y axis Z axis
        var targetRotation = Quaternion.Euler(-targetLookAngle.y + lagRotation.x, targetLookAngle.x + lagRotation.y, lagRotation.z);
        // Set positon/rotation
        transform.localPosition = targetRotation * offset;
        transform.localRotation = targetRotation;

        // Set main cameras rotation
        main_cam.position = transform.position;
        main_cam.rotation = transform.rotation;
    }
}
