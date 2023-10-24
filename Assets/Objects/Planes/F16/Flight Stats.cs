using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class FlightStats : MonoBehaviour
{
    public Vector3 airspeed;
    public Vector3 angular_velocity;
    public float altitude;
    public float vertical_speed;
    public float g_force;
    public Vector3 aoa;
    private Vector3 airspeed_ms;
    private Vector3 aoa_rad;
    private Vector3 previous_position;
    

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = transform.GetComponent<Rigidbody>();
        rb.velocity = transform.forward * 30;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // airspeed
        airspeed_ms = transform.InverseTransformDirection(rb.velocity);
        airspeed = airspeed_ms * 1.94384f;

        // angular speed
        angular_velocity = transform.InverseTransformDirection(rb.angularVelocity) * Mathf.Rad2Deg;

        // aoa
        aoa.x = Vector3.SignedAngle(transform.InverseTransformDirection(transform.forward), airspeed, transform.InverseTransformDirection(transform.right));

        // altitude
        altitude = transform.position.y;

        // vertical speed
        vertical_speed = rb.velocity.y * (float)3.28084;
        
        // Estimate G force with Fg = (V^2/r)/G
        {
            float circumfrence = -360/(transform.InverseTransformDirection(rb.angularVelocity).x * Mathf.Rad2Deg) * rb.velocity.magnitude;
            float radius = circumfrence / (2 * (float)Math.PI);
            g_force = (float)Math.Pow(rb.velocity.magnitude, 2)/radius / 9.81f + transform.TransformDirection(transform.up).y;
        }

        // Estimate G force by calculating
    }
}
