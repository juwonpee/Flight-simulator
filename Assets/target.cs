using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class target : MonoBehaviour
{   
    enum moveAxis_t {
        x, y, z
    };
    public int range = 100;
    public int speed = 10;
    // Start is called before the first frame update

    [SerializeField] Vector3 startPosition;
    [SerializeField] bool reverse = true;
    [SerializeField] moveAxis_t axis;
    Rigidbody rb;
    [SerializeField] Vector3 velocity;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;        
    }

    // Update is called once per frame
    void Update()
    {
        velocity = GetComponent<Rigidbody>().velocity;

        if (axis == moveAxis_t.x) {
            if (!reverse) {
                rb.velocity = Vector3.right * speed;
            }
            else {
                rb.velocity = -Vector3.right * speed;
            }
            // lower Boundary
            if (transform.position.x < startPosition.x - range) {
                reverse = false;
            }
            // upper boundary
            else if (transform.position.x > startPosition.x + range) {
                reverse = true;
            }
        }
        else if (axis == moveAxis_t.y) {
            if (!reverse) {
                rb.velocity = Vector3.up * speed;
            }
            else {
                rb.velocity = -Vector3.up * speed;
            }
            // lower Boundary
            if (transform.position.y < startPosition.y - range) {
                reverse = false;
            }
            // upper boundary
            else if (transform.position.y > startPosition.y + range) {
                reverse = true;
            }
        }
        else if (axis == moveAxis_t.z) {
            if (!reverse) {
                rb.velocity = Vector3.forward * speed;
            }
            else {
                rb.velocity = -Vector3.forward * speed;
            }
            // lower Boundary
            if (transform.position.z < startPosition.z - range) {
                reverse = false;
            }
            // upper boundary
            else if (transform.position.z > startPosition.z + range) {
                reverse = true;
            }
        }
    }
}
