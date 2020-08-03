using MapMagic.Terrains;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightWingControl : MonoBehaviour
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

    //private stuff
    public float zVelocity;
    public float yVelocity;
    public float alpha;
    public float yForce;
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
        rb = GetComponent<Rigidbody>();

        // get global parameters
        area = fusalageScript.wingArea;
        liftM = fusalageScript.wingLiftM;
        liftC = fusalageScript.wingLiftC;
        stallM = fusalageScript.wingStallM;
        stallC = fusalageScript.wingStallC;
        dragM = fusalageScript.wingDragM;
        dragC = fusalageScript.wingDragC;
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
        yForce = calcLift();

        // apply forces
        // lift
        rb.AddForce(yForce * transform.up);

        /********************PHYSICS********************/
    }
}
