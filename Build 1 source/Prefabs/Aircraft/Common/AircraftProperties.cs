using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftProperties: MonoBehaviour
{
    // variables for local use
    public Vector3 centerOfMass;

    //global variables for reference
    public float wingLiftCoeff;
    public float wingAlphaLiftCoeff;
    public float wingDragCoeff;
    public float wingStallAlpha;

    public float aileronLiftCoeff;
    public float aileronAlphaLiftCoeff;
    public float aileronDragCoeff;
    public float aileronStallAlpha;

    public float aileronMaxUpDefAlpha;
    public float aileronMaxDownDefAlpha;
    [Range(1.0f, 10.0f)]
    public float aileronSensitivity;
    [Range(1.0f, 10.0f)]
    public float aileronSmoothing;

    public float tailXLiftCoeff;
    public float tailYLiftCoeff;
    public float tailXAlphaLiftCoeff;
    public float tailYAlphaLiftCoeff;
    public float tailDragCoeff;

    public float elevatorLiftCoeff;
    public float elevatorAlphaLiftCoeff;
    public float elevatorDragCoeff;
    public float elevatorStallAlpha;

    public float elevatorMaxUpDefAlpha;
    public float elevatorMaxDownDefAlpha;
    [Range(1.0f, 10.0f)]
    public float elevatorSensitivity;
    [Range(1.0f, 10.0f)]
    public float elevatorSmoothing;

    public float rudderLiftCoeff;
    public float rudderAlphaLiftCoeff;
    public float rudderDragCoeff;
    public float rudderStallAlpha;

    public float rudderMaxUpDefAlpha;
    public float rudderMaxDownDefAlpha;
    [Range(1.0f, 10.0f)]
    public float rudderSensitivity;
    [Range(1.0f, 10.0f)]
    public float rudderSmoothing;

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
