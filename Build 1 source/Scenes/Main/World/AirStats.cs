using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirStats : MonoBehaviour
{
    // public input floats
    /// local wind conditions in {}
    public Vector3 wind;
    /// air temperature in {Kelvin}
    public float aT;
    /// dew point in {Celsius}
    public float dP;
    /// station pressure in {millibars}
    public float sP;



    //https://www.weather.gov/media/epz/wxcalc/densityAltitude.pdf using this as reference
    float vP;                                                                                      // vapour pressure
    float vT;                                                                                      // virtual temperature
    float r;                                                                                       // rankine
    float dA;                                                                                      // density rankine


    ///<summary>Function <c>getDensityAltitude</c> Calculates and gets Density Altitude in {}</summary>
    public float getDensityAltitude(float altitude)
    {
        return altitude;
    }

    float calcVP (float dewPoint)
    {
        return (float)(6.11 * ((7.5 * dewPoint) / (237.7 + dewPoint)));
    }
    float calcVT (float aT, float vP, float sP)
    {
        return (float)(aT / (1-(vP/sP)*(1-0.622)));
    }
    float calcR (float vT)
    {
        return (float)(((9/5) * (vT - 273.16) + 32) + 459.69);
    }
    float calcDA (float R) 
    {
        return 0;
    }
    void Start()
    {
        vP = calcVP(dP);
        vT = calcVT(aT, vP, sP);
        r = calcR(vT);
        //dA

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
