using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirStats : MonoBehaviour
{
    // public input floats
    /// local wind conditions in {}
    public Vector3 wind;
    /// air temperature in {Kelvin}
    public float seaTemp;
    /// dew point in {Celsius}
    public float dewPoint;
    /// station pressure in {millibars}
    public float seaPress;



    //https://www.weather.gov/media/epz/wxcalc/densityAltitude.pdf using this as reference
    float vP;                                                                                      // vapour pressure
    float vT;                                                                                      // virtual temperature
    float r;                                                                                       // rankine
    float dA;                                                                                      // density rankine


    ///<summary>Function <c>getDensityAltitude</c> Calculates and gets Density Altitude in {}</summary>
    public float getDensity(float altitude)
    {
        return altitude;
    }


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
