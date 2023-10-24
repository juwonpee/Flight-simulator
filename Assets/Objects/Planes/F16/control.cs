using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class control : MonoBehaviour
{
    public GameObject aileron_left;
    public GameObject aileron_right;
    public GameObject rudder;
    public GameObject elevator_left;
    public GameObject elevator_right;
    public GameObject engine;
    //  ---------------------------------------------------------------------------
    [Range(-1f, 1f)]
    float aileron_left_position;
    [Range(-1f, 1f)]
    float aileron_right_position;
    [Range(-1f, 1f)]
    float elevator_left_position;
    [Range(-1f, 1f)]
    float elevator_right_position;
    [Range(-1f, 1f)]
    float rudder_position;
    [Range(0, 1f)]
    float engine_position;
    //  ---------------------------------------------------------------------------
    [Range(0f, 0.1f)]
    public float aileron_sensitivity;
    [Range(0f, 0.1f)]
    public float elevator_sensitivity;
    [Range(0f, 0.1f)]
    public float rudder_sensitivity;
    [Range(0f, 0.1f)]
    public float engine_sensitivity;
    //  ---------------------------------------------------------------------------
    [Range(0f, 20f)]
    public float aileron_max_deflection;
    [Range(0f, 20f)]
    public float elevator_max_deflection;
    [Range(0f, 20f)]
    public float rudder_max_deflection;
    [Range(0f, 1f)]
    public float engine_max_deflection;
    //  ---------------------------------------------------------------------------
    float aileron_left_deflection;
    float aileron_right_deflection;
    float rudder_deflection;
    float elevator_left_deflection;
    float elevator_right_deflection;
    float engine_deflection;
    // ----------------------------------------------------------------------------
    public float flybywire_sensitivity;
    public Vector3 angular_max_velocity;
    public float aoa_max;
    public float g_force_max = 9;

    FlightStats stats;


    // Fly by wire system
    bool isFlyByWire = true;


    // Start is called before the first frame update
    void Start()
    {
        stats = transform.GetComponent<FlightStats>();
    }

    void Update() {
        elevator_left_deflection = elevator_left_position * elevator_max_deflection;
        elevator_right_deflection = elevator_right_position * elevator_max_deflection;
        aileron_left_deflection = aileron_left_position * aileron_max_deflection;
        aileron_right_deflection = aileron_right_position * aileron_max_deflection;
        rudder_deflection = rudder_position * rudder_max_deflection;
        engine_deflection = engine_position * engine_max_deflection;

        elevator_left.transform.localRotation = Quaternion.Slerp(elevator_left.transform.localRotation, Quaternion.Euler(elevator_left_deflection + aileron_left_deflection, 0, 0), Time.fixedDeltaTime*2);
        elevator_right.transform.localRotation = Quaternion.Slerp(elevator_right.transform.localRotation, Quaternion.Euler(elevator_right_deflection + aileron_right_deflection, 0, 0), Time.fixedDeltaTime*2);
        aileron_left.transform.localRotation = Quaternion.Slerp(aileron_left.transform.localRotation, Quaternion.Euler(aileron_left_deflection, 0, 0), Time.fixedDeltaTime*2);
        aileron_right.transform.localRotation = Quaternion.Slerp(aileron_right.transform.localRotation, Quaternion.Euler(aileron_right_deflection, 0, 0), Time.fixedDeltaTime*2);
        rudder.transform.localRotation = Quaternion.Slerp(rudder.transform.localRotation, Quaternion.AngleAxis(rudder_deflection, new Vector3(0, 1, -0.6f)), Time.fixedDeltaTime*2);

        transform.GetComponent<AircraftPhysics>().SetThrustPercent(engine_deflection);
    }

    // Update is called once per frame
    void FixedUpdate() {
        // Elevator
        if (Input.GetKey(KeyCode.S)) {
            // If under limit
            if (elevator_left_position < 1f) elevator_left_position += elevator_sensitivity;
            if (elevator_right_position < 1f) elevator_right_position += elevator_sensitivity;
        }
        else if (Input.GetKey(KeyCode.W)) {
            if (elevator_left_position > -1f) elevator_left_position -= elevator_sensitivity;
            if (elevator_right_position > -1f) elevator_right_position -= elevator_sensitivity;
        }
        else {
            if (elevator_left_position > 0f) elevator_left_position -= elevator_sensitivity;
            else elevator_left_position += elevator_sensitivity;
            if (elevator_right_position > 0f) elevator_right_position -= elevator_sensitivity;
            else elevator_right_position += elevator_sensitivity;
        }

        // Aileron
        if (Input.GetKey(KeyCode.A)) {
            // If under limit
            if (aileron_left_position < 1f) aileron_left_position += aileron_sensitivity;
            if (aileron_right_position > -1f) aileron_right_position -= aileron_sensitivity;
        }
        else if (Input.GetKey(KeyCode.D)) {
            if (aileron_left_position > -1f) aileron_left_position -= aileron_sensitivity;
            if (aileron_right_position < 1f) aileron_right_position += aileron_sensitivity;
        }
        else {
            if (aileron_left_position > 0f) aileron_left_position -= aileron_sensitivity;
            else aileron_left_position += aileron_sensitivity;
            if (aileron_right_position > 0f) aileron_right_position -= aileron_sensitivity;
            else aileron_right_position += aileron_sensitivity;
        }

        // Rudder
        if (Input.GetKey(KeyCode.Q)) {
            if (rudder_position < 1f) rudder_position += rudder_sensitivity;
        }
        else if (Input.GetKey(KeyCode.E)) {
            if (rudder_position > -1f) rudder_position -= rudder_sensitivity;
        }
        else {
            if (rudder_position > 0f) rudder_position -= aileron_sensitivity;
            else rudder_position += aileron_sensitivity;
        }  



        // // Fly by wire only available when airspeed > 140
        // if (isFlyByWire && stats.airspeed.z > 140f) {
        //     // Elevator
        //     // Only obstruct when G force or aoa exceeds
        //     if (stats.g_force > g_force_max | stats.aoa.x > aoa_max) {
        //         elevator_left_position -= flybywire_sensitivity * elevator_sensitivity;
        //         elevator_right_position -= flybywire_sensitivity * elevator_sensitivity;
        //     }
                
        // }
        

        // Engine
        if (Input.GetKey(KeyCode.LeftShift)) {
            if (engine_position < 1f) engine_position += engine_sensitivity;
        }
        else if (Input.GetKey(KeyCode.LeftControl)) {
            if (engine_position > 0) engine_position -= engine_sensitivity;
        }

        // Gear
        if (Input.GetKeyDown(KeyCode.G)) {

        }
    }
}
