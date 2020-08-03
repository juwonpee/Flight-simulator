using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform target;
    public float smoothSpeed;
    public Vector3 offset;
    public Vector3 lookatOffset;

    void Start()
    {

    }

    // Update after frame is called
    void FixedUpdate()
    {
        Vector3 desiredPosition = transform.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(target.TransformPoint(offset), desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(target);
    }
}
