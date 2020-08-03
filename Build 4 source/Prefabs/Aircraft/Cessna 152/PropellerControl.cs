using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerControl : MonoBehaviour
{
    public GameObject fusalage;
    FusalageControl fusalageScript;
    Rigidbody hull;
    Transform propeller;


    float rotateCoeff;
    float thrustCoeff;
    public float throttle;
    float idleRPM;
    float thrust;


    float calcRotate (float throttle)
    {
        return idleRPM + throttle * rotateCoeff;
    }


    void Start()
    {

        // get parameter values
        fusalageScript = fusalage.GetComponent<FusalageControl>();
        hull = fusalageScript.fusalage;
        propeller = fusalageScript.propeller;
        rotateCoeff = fusalageScript.propRotateCoeff;
        thrustCoeff = fusalageScript.propThrustCoeff;
        idleRPM = fusalageScript.propIdleRPM;
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        // general housekeepeing
        throttle = fusalageScript.getThrottle();                                                         // get throttle axis
        thrust = thrustCoeff * throttle;



        // rotate prop
        propeller.Rotate(0, calcRotate(throttle) * Time.deltaTime, 0, Space.Self);
        // apply force
        hull.AddForceAtPosition(-transform.up * thrust, transform.position);


    }
}
