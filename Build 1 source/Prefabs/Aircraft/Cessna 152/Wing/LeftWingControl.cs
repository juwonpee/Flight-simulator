using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftWingControl : MonoBehaviour
{
    // global stuff
    public Transform fusalage;
    FusalageControl fusalageScript;
    // global variables import
    float liftCoeff;
    float alphaLiftCoeff;
    float dragCoeff;
    float alphaDragCoeff;
    float stallAlpha;

    //private stuff
    float defAlpha;
    public float zVelocity;
    public float yVelocity;
    public float alpha;
    public float yForce;
    bool isStalling;
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
            //Debug.Log("Elevator Stall");
        }
        return lift;
    }
    /********************PHYSICS********************/

    void Start()
    {
        //init
        fusalageScript = fusalage.GetComponent<FusalageControl>();
        rb = GetComponent<Rigidbody>();

        // get global parameters
        liftCoeff = fusalageScript.wingLiftCoeff;
        alphaLiftCoeff = fusalageScript.wingAlphaLiftCoeff;
        dragCoeff = fusalageScript.wingDragCoeff;
        stallAlpha = fusalageScript.wingStallAlpha;
    }

    void FixedUpdate()
    {
        // general housekeeping 

        /*******************ANIMATION*******************/
        /*******************ANIMATION*******************/

        /********************PHYSICS********************/
        // calculate values
        yVelocity = transform.InverseTransformVector(rb.velocity).y;
        zVelocity = transform.InverseTransformVector(rb.velocity).z;
        alpha = calcAlpha(yVelocity, zVelocity);
        yForce = calcLift(zVelocity, alpha, liftCoeff, alphaLiftCoeff, stallAlpha);

        // apply forces
        // lift
        rb.AddForce(yForce * transform.up);

        /********************PHYSICS********************/
    }
}
