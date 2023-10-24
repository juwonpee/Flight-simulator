using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum weapon_type_t {
    AIM9,
    AIM120,
    BOMB,
    HARM,
    GUN
}


public class WeaponCore : MonoBehaviour {
    
    public weapon_type_t type;
    public Transform target;
    // To figure out who killed who
    public Transform player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void Launch() {
    }
    public virtual void Drop() {}

    protected virtual void Kill() { }
}
