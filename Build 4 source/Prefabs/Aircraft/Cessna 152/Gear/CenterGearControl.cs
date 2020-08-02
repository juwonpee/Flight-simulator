using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterGearControl : MonoBehaviour
{
    // global variables
    public GameObject fusalage;
    public FusalageControl fusalageScript;
    public WheelCollider centerGear;

    public float maxSteerAlpha;
    public float steerAlpha;
    float maxBrakeForce;
    float brakeForce;
    float smoothing;

    // private
    float yAxis;

    float calcDefAlpha (float yAxis, float maxSteerAlpha)
    {
        return (yAxis / 100f) * maxSteerAlpha;
    }

    // Start is called before the first frame update
    void Start()
    {
        // get global values
        fusalageScript = fusalage.GetComponent<FusalageControl>();
        centerGear = fusalageScript.centerGear;
        maxSteerAlpha = fusalageScript.gearMaxSteerAlpha;
        maxBrakeForce = fusalageScript.gearMaxBrakeForce;
        smoothing = fusalageScript.gearSmoothing;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // general housekeeping
        yAxis = fusalageScript.getRudder();

        // rotate z axis of gear
        steerAlpha = calcDefAlpha(yAxis, maxSteerAlpha);
        centerGear.steerAngle = Mathf.SmoothStep(centerGear.steerAngle,steerAlpha, smoothing * Time.deltaTime);
    }
}
