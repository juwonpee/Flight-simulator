using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instruments : MonoBehaviour
{
    // Start is called before the first frame update
    public Quaternion attitude;
    public Vector3 attitude2;
    public float indicatedAirSpeed;
    public float trueAirSpeed;
    public float groundSpeed;
    public float altimeter;
    public float verticalSpeed;
    public float turnIndicator;
    public uint rpm;
    public float meterToKnots;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // attitude
        // x axis: up&down (attitude indicator)
        // y axis: left&right (heading indicator)
        // z azis: left&right roll (attitude indicator)
        attitude = transform.rotation;

        // indicatedairspeed
        indicatedAirSpeed = Vector3.Dot(GetComponent<Rigidbody>().velocity, transform.forward) * meterToKnots;

        // trueairspeed
        trueAirSpeed = indicatedAirSpeed;                                                          // not implemented now; need to complete world script

        // altimeter
        altimeter = transform.position.y * meterToKnots;

        // verticalSpeed
        verticalSpeed = GetComponent<Rigidbody>().velocity.y * 3.28f;

        // turnIndicator
       //turnIndicator = 0f;                                                                        // need more info
    }
}
