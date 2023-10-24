using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HMD : MonoBehaviour
{
    public Transform cam;
    Transform player;
    private FlightStats stats;
    public Text airspeed;
    public Text altitude;
    public Text aoa;
    public Text g_force;
    public Text weapon_type;
    public Text targeting_mode;
    public Transform compass;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        stats = player.transform.GetComponent<FlightStats>();
    }

    // Update is called once per frame
    void Update()
    {
        string temp = "";
        F16_Weapons playerWeapon = player.GetComponent<F16_Weapons>();
        g_force.text = string.Format($"G: {stats.g_force:0.0}");
        airspeed.text = string.Format($"{(float)1.94384 * stats.airspeed.z:0}");
        altitude.text = string.Format($"{stats.altitude:0}");
        aoa.text = string.Format($"α: {stats.aoa.x:0.0}");

        switch (playerWeapon.targeting_mode) {
            case targeting_mode_t.a2a:
                targeting_mode.text = "A2A";
                switch (playerWeapon.weapon_type_selected) {
                    case weapon_type_t.AIM9:
                        temp = "AIM9";
                        break;
                    case weapon_type_t.AIM120:
                        temp = "AIM120";
                        break;
                    case weapon_type_t.GUN:
                        temp = "GUN";
                        break;
                }
                break;
            case targeting_mode_t.a2g:
                targeting_mode.text = "A2G";
                switch (playerWeapon.weapon_type_selected) {
                    case weapon_type_t.BOMB:
                        temp = "BOMB";
                        break;
                    case weapon_type_t.HARM:
                        temp = "HARM";
                        break;
                    case weapon_type_t.GUN:
                        temp = "GUN";
                        break;
                }
                break;
        }

        // Find number of weapons of type
        weapon_type.text = temp + string.Format($": {playerWeapon.weapon_number:0}"); 


    }
}
