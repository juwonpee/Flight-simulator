using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class AircraftProperties: MonoBehaviour
{
    // variables for local use
    public Transform world;
    public AirStats worldScript;
    float seaPress;
    float seaTemp;
    float dewPoint;
    public Vector3 centerOfMass;

    //global variables for reference
    public float wingArea;
    public float wingLiftM;
    public float wingLiftC;
    public float wingStallAlpha;
    public float wingStallM;
    public float wingStallC;
    public float wingDragM;
    public float wingDragC;

    public float aileronArea;
    public float aileronLiftM;
    public float aileronLiftC;
    public float aileronStallAlpha;
    public float aileronStallM;
    public float aileronStallC;
    public float aileronDragM;
    public float aileronDragC;

    public float aileronMaxUpDefAlpha;
    public float aileronMaxDownDefAlpha;
    [Range(1.0f, 10.0f)]
    public float aileronSensitivity;

    public float tailArea;
    public float tailLiftM;
    public float tailLiftC;
    public float tailStallAlpha;
    public float tailStallM;
    public float tailStallC;
    public float tailDragM;
    public float tailDragC;

    public float elevatorArea;
    public float elevatorLiftM;
    public float elevatorLiftC;
    public float elevatorStallAlpha;
    public float elevatorStallM;
    public float elevatorStallC;
    public float elevatorDragM;
    public float elevatorDragC;

    public float elevatorMaxUpDefAlpha;
    public float elevatorMaxDownDefAlpha;
    [Range(1.0f, 10.0f)]
    public float elevatorSensitivity;

    public float rudderArea;
    public float rudderLiftM;
    public float rudderLiftC;
    public float rudderStallAlpha;
    public float rudderStallM;
    public float rudderStallC;
    public float rudderDragM;
    public float rudderDragC;

    public float rudderMaxUpDefAlpha;
    public float rudderMaxDownDefAlpha;
    [Range(1.0f, 10.0f)]
    public float rudderSensitivity;

    public float propRotateCoeff;
    public float propThrustCoeff;
    public float propThrustSensitivity;
    public float propIdleRPM;

    public float gearMaxSteerAlpha;
    public float gearMaxBrakeForce;
    [Range(1.0f, 10.0f)]
    public float gearSmoothing;

    public Rigidbody fusalage;
    public Transform propeller;
    public Transform leftWing;
    public Transform rightWing;
    public Transform leftAileron;
    public Transform rightAileron;
    public Transform leftElevator;
    public Transform rightElevator;
    public Transform rudder;
    public WheelCollider leftGear;
    public WheelCollider rightGear;
    public WheelCollider centerGear;

    // aileron axis

    public virtual void Start()
    {
        worldScript = world.GetComponent<AirStats>();
    }

    public float calcAileron(float zAxis)
    {
        zAxis += Input.GetAxis("Aileron") * aileronSensitivity;
        if (zAxis > 100)
        {
            zAxis = 100;
        }
        else if (zAxis < -100)
        {
            zAxis = -100;
        }
        return zAxis;
    }

    // elevator axis
    public float calcElevator(float xAxis)
    {
        xAxis += Input.GetAxis("Elevator") * elevatorSensitivity;
        if (xAxis > 100)
        {
            xAxis = 100;
        }
        else if (xAxis < -100)
        {
            xAxis = -100;
        }
        return xAxis;
    }


    // rudder axis
    public float calcRudder(float yAxis)
    {
        yAxis += Input.GetAxis("Rudder") * elevatorSensitivity;
        if (yAxis > 100)
        {
            yAxis = 100;
        }
        else if (yAxis < -100)
        {
            yAxis = -100;
        }
        return yAxis;
    }

    // throttle axis
    public float calcThrottle(float throttle)
    {
        throttle += Input.GetAxis("Throttle");
        if (throttle > 100)  // set limit to 100 and -10
        {
            throttle = 100f;
        }
        else if (throttle < -10)
        {
            throttle = -10f;
        }
        return throttle;
    }
    public Vector3 getVelocity()
    {
        return fusalage.velocity;
    }
    public float getAlpha()
    {
        float alpha = -Mathf.Tan(getVelocity().y / getVelocity().z) * Mathf.Rad2Deg;
        if (alpha < -300)
        {
            return 360 + alpha;
        }
        return alpha;
    }

    public float calcDefAlpha(float zAxis, float maxUpDefAlpha, float maxDownDefAlpha) {
        float maxDefAlpha = 0f;
        if (zAxis > 0)                                                                             // right bank
        {
            maxDefAlpha = Mathf.Abs(zAxis) * (maxUpDefAlpha / 100f);
        }
        else if (zAxis < 0)                                                                        // left bank
        {
            maxDefAlpha = Mathf.Abs(zAxis) * (maxDownDefAlpha / 100f);
        }
        return maxDefAlpha;
    }


}
