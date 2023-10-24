using JetBrains.Annotations;
using System.Collections;
using System.Runtime;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

public enum targeting_mode_t {
    a2a, a2g
}
public class F16_Weapons : MonoBehaviour
{   

    public GameObject aim9;
    public GameObject aim120;
    public Transform target;
    public List<WeaponCore> weapons;
    public weapon_type_t weapon_type_selected;
    [SerializeField] WeaponCore weapon_selected;
    public targeting_mode_t targeting_mode;
    public int weapon_number;
    // Start is called before the first frame update
    void Start()
    {
        Vector3[] pylon = {
             new Vector3(-4.977844f, 0.2750854f, -0.1530762f),
             new Vector3(-3.404785f, -0.06567383f, 0.4956055f),
             new Vector3(-2.253296f, -0.09094238f, 0.973877f),
             new Vector3(2.253296f, -0.09094238f, 0.973877f),
             new Vector3(3.404785f, -0.06567383f, 0.4956055f),
             new Vector3(4.977844f, 0.2750854f, -0.1530762f),
         };

        // Create and initialize weapons
        for (int x = 0; x < pylon.Length; x++) {
            weapons.Add(Instantiate(aim9, transform.TransformPoint(pylon[x]), transform.rotation).GetComponent<WeaponCore>());
            weapons[x].player = transform;
            weapons[x].transform.GetComponent<FixedJoint>().connectedBody = transform.GetComponent<Rigidbody>();
        }

        SelectWeapon();
        
    }

    // Update is called once per frame
    void Update()
    { 
        // Targeting Mode Change
        if (Input.GetKeyDown(KeyCode.R)) {
            Debug.Log("Targeting Mode Change");
            switch (targeting_mode) {
                case targeting_mode_t.a2a:
                    targeting_mode = targeting_mode_t.a2g;
                    weapon_type_selected = weapon_type_t.BOMB;
                    break;
                case targeting_mode_t.a2g:
                    targeting_mode = targeting_mode_t.a2a;
                    weapon_type_selected = weapon_type_t.AIM9;
                    break;
            }
            SelectWeapon();
        }

        // Weapon change
        if (Input.GetKeyDown(KeyCode.F)) {
            Debug.Log("Weapon Change");
            switch (targeting_mode) {
                case targeting_mode_t.a2a:
                    switch (weapon_type_selected) {
                        case weapon_type_t.AIM9:
                            weapon_type_selected = weapon_type_t.AIM120;
                            break;
                        case weapon_type_t.AIM120:
                            weapon_type_selected = weapon_type_t.GUN;
                            break;
                        case weapon_type_t.GUN:
                            weapon_type_selected = weapon_type_t.AIM9;
                            break;
                    }
                    break;
                case targeting_mode_t.a2g:
                    switch (weapon_type_selected) {
                        case weapon_type_t.BOMB:
                            weapon_type_selected = weapon_type_t.HARM;
                            break;
                        case weapon_type_t.HARM:
                            weapon_type_selected = weapon_type_t.GUN;
                            break;
                        case weapon_type_t.GUN:
                            weapon_type_selected = weapon_type_t.BOMB;
                            break;
                    }
                    break;
            }
            SelectWeapon();
        }


        

    }

    /*
    Weapon firing mechanism
    1. Select targeting mode and weapon type
    2. Select weapon of type
    3. 
    */

    // Select current weapon type
    void SelectWeapon() {
        // Get valid weapons
        List<WeaponCore> validWeapons = new List<WeaponCore>();
        foreach (var weapon in weapons) {
            if (weapon.type == weapon_type_selected) {
                validWeapons.Add(weapon);
            }
        }
        weapon_number = validWeapons.Count;
        // If empty
        if (validWeapons.Count == 0) {
            weapon_selected = null;
            return;
        }
        // Find outermost weapon
        int index = 0;
        for (int x = 0; x < validWeapons.Count; x++) {
            if (Mathf.Abs(transform.InverseTransformPoint(validWeapons[index].transform.position).x) <= Mathf.Abs(transform.InverseTransformPoint(validWeapons[x].transform.position).x)) {
                index = x;
            }
        }
        // Designate selected weapon
        weapon_selected = validWeapons[index];

    }



    public bool TargetWeapon(Transform target) {
        // if no weapons
        if (weapon_selected == null) return false;

        // if a2a missiles
        if (targeting_mode == targeting_mode_t.a2a) {
            MissileCore weapon = weapon_selected.transform.GetComponent<MissileCore>();
            return weapon.Launchable(target);
        }
        return false;
    }

    public void Fire(Transform target) {
        if (weapon_selected == null) {
            Debug.Log("No more weapons of selected type");
            return;
        }
        weapon_selected.target = target;
        weapon_selected.Launch();
        weapons.Remove(weapon_selected);
        SelectWeapon();
    }

}