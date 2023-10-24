using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum target_iff_type_t {
    friendly, foe, neutral
};

public static class Utility {
    // Get angles on all three major axis
    public static Vector3 GetAngles(Vector3 from, Vector3 to) {
        Vector3[] axis = {
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1)
        };

        Vector3 angle = new Vector3();
        
        foreach(var singleAxis in axis) {
            Vector3 perpendicularFrom = Vector3.Cross(singleAxis, from);
            Vector3 perpendicularTo = Vector3.Cross(singleAxis, to);
            angle += singleAxis * Vector3.SignedAngle(perpendicularFrom, perpendicularTo, singleAxis);
        }
        return angle;
    }
    public static float GetAngle(Vector3 from, Vector3 to, Vector3 axis) {
        // Vector3 perpendicularStart = Vector3.Cross(axis, from);
        // Vector3 perpendicularTo = Vector3.Cross(axis, to);
        // float angle = Vector3.SignedAngle(perpendicularStart, perpendicularTo, axis);
        // return angle;

        Vector3 planeFrom = Vector3.ProjectOnPlane(from, axis);
        Vector3 planeTo = Vector3.ProjectOnPlane(to, axis);
        return Vector3.SignedAngle(planeFrom, planeTo, axis);
    }

};

public struct tracking_t {
    public Transform target;
    public Vector3 direction;
    public Vector3 velocity;
    public target_iff_type_t iff_type;


    public tracking_t(Transform _target) : this(){
        this.target = _target;
        this.direction = new Vector3();
        this.velocity = new Vector3();
        this.iff_type = target_iff_type_t.neutral;
    }

    public tracking_t(Transform _target, Vector3 _direciton, Vector3 _velocity, target_iff_type_t _iff_type) : this() {
        this.target = _target;
        this.direction = _direciton;
        this.velocity = _velocity;
        this.iff_type = _iff_type;
    }
}; 
