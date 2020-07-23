using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FusalageControl : AircraftProperties
{
    public Vector3 velocity;
    float xAxis;
    float zAxis;
    float yAxis;
    float throttle;

    // ****************************BUG WORKAROUND****************************
    // https://fogbugz.unity3d.com/default.asp?747895_gmsqj94tlfnrf6q3&_ga=2.213926231.190394458.1581507745-1198437751.1573100689
    // when wheelcollider has velocity of 0, goes into sleep mode: infinite friction
    void wheelColliderBug()
    {
        leftGear.motorTorque = 0.001f;
        rightGear.motorTorque = 0.001f;
        centerGear.motorTorque = 0.001f;
    }
    // **********************************************************************
    public float getElevator () { return xAxis; }
    public float getAileron () { return zAxis; }
    public float getRudder () { return yAxis; }
    public float getThrottle () { return throttle; }
    // Start is called before the first frame update
    void Start()
    {
        // set center of mass
        fusalage.centerOfMass = centerOfMass;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        wheelColliderBug();
        velocity = getVelocity();
        
        xAxis = calcElevator(xAxis);
        zAxis = calcAileron(zAxis);
        yAxis = calcRudder(yAxis);
        throttle = calcThrottle(throttle);
        // general housekeeping
    }
}
