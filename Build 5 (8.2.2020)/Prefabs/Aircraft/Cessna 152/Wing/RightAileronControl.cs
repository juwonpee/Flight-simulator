using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightAileronControl : MonoBehaviour
{
    // global stuff
    public Transform fusalage;
    FusalageControl fusalageScript;
    // global variables import
    float area;
    float liftM;
    float liftC;
    float stallM;
    float stallC;
    float dragM;
    float dragC;
    float stallAlpha;
    float maxUpDefAlpha;
    float maxDownDefAlpha;
    float smoothing;

    //private stuff
    float defAlpha;
    float zInputAxis;
    public float zVelocity;
    public float yVelocity;
    public float alpha;
    public float yForce;
    bool isStalling;
    HingeJoint hinge;
    JointSpring hingeSpring;
    Rigidbody rb;

    /*******************ANIMATION*******************/
    /*******************ANIMATION*******************/


    /********************PHYSICS********************/
    float calcAlpha(float yVelocity, float zVelocity)
    {
        float alpha = -Mathf.Tan(yVelocity / zVelocity) * Mathf.Rad2Deg;
        if (alpha < -300)
        {
            return 360 + alpha;
        }
        return alpha;
    }
    float calcLift()
    {
        return calcCL(alpha) * ((fusalageScript.worldScript.getDensity(transform.position.y) * Mathf.Pow(zVelocity, 2)) / 2) * area;
    }

    float calcCL(float alpha)
    {
        if (alpha < stallAlpha)
        {
            return liftM * alpha + liftC;
        }
        else
        {
            return stallM * alpha + stallC;
        }
    }
    /********************PHYSICS********************/

    void Start()
    {
        //init
        fusalageScript = fusalage.GetComponent<FusalageControl>();
        hinge = GetComponent<HingeJoint>();
        hingeSpring = hinge.spring;
        rb = GetComponent<Rigidbody>();

        // get global parameters
        area = fusalageScript.aileronArea;
        liftM = fusalageScript.aileronLiftM;
        liftC = fusalageScript.aileronLiftC;
        stallM = fusalageScript.aileronStallM;
        stallC = fusalageScript.aileronStallC;
        dragM = fusalageScript.aileronDragM;
        dragC = fusalageScript.aileronDragC;
        stallAlpha = fusalageScript.aileronStallAlpha;
        maxUpDefAlpha = fusalageScript.aileronMaxUpDefAlpha;
        maxDownDefAlpha = fusalageScript.aileronMaxDownDefAlpha;
    }

    void FixedUpdate()
    {
        // general housekeeping 
        zInputAxis = fusalageScript.getAileron();                                                                // get axis values
        defAlpha = fusalageScript.calcDefAlpha(zInputAxis, maxUpDefAlpha, maxDownDefAlpha);

        /*******************ANIMATION*******************/
        // rotate aileron to target axis
        hingeSpring.targetPosition = defAlpha;
        hinge.spring = hingeSpring;
        /*******************ANIMATION*******************/

        /********************PHYSICS********************/
        // calculate values
        yVelocity = transform.InverseTransformDirection(rb.velocity).y;
        zVelocity = transform.InverseTransformDirection(rb.velocity).z;
        alpha = calcAlpha(yVelocity, zVelocity);
        yForce = calcLift();

        // apply forces
        rb.AddForce(yForce * transform.up);

        /********************PHYSICS********************/
    }
}
