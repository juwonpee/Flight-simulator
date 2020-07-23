using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftAileronControl : MonoBehaviour
{
    // global stuff
    public Transform fusalage;
    FusalageControl fusalageScript;
    // global variables import
    float liftCoeff;
    public float alphaLiftCoeff;
    float dragCoeff;
    float alphaDragCoeff;
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

        // check if stalling
        if (Mathf.Abs(alpha) > stallAlpha)
        {
            isStalling = true;
        }
        else
        {
            isStalling = false;
        }
        return alpha;
    }
    float calcLift(float zVelocity, float alpha, float liftCoeff, float alphaLiftCoeff, float stallAlpha)
    {
        float lift = 0;
        lift = zVelocity * liftCoeff;
        lift = lift + zVelocity * alphaLiftCoeff * alpha;
        if (isStalling)                                                         // if stalling
        {
            lift = lift + zVelocity * alphaLiftCoeff * (stallAlpha - alpha);
        }
        return lift;
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
        liftCoeff = fusalageScript.aileronLiftCoeff;
        alphaLiftCoeff = fusalageScript.aileronAlphaLiftCoeff;
        dragCoeff = fusalageScript.aileronDragCoeff;
        stallAlpha = fusalageScript.aileronStallAlpha;
        maxUpDefAlpha = fusalageScript.aileronMaxUpDefAlpha;
        maxDownDefAlpha = fusalageScript.aileronMaxDownDefAlpha;
        smoothing = fusalageScript.aileronSmoothing;
    }

    void FixedUpdate()
    {
        // general housekeeping 
        zInputAxis = -fusalageScript.getAileron();                                                                // get axis values
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
        yForce = calcLift(zVelocity, alpha, liftCoeff, alphaLiftCoeff, stallAlpha);

        // apply forces
        rb.AddForce(yForce * transform.up);

        /********************PHYSICS********************/
    }
}
