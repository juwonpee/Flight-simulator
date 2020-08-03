using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirspeedController : MonoBehaviour
{
    public GameObject longHand;
    public Vector3 longRotate;
    public GameObject shortHand;
    public Vector3 shortRotate;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        longHand.transform.Rotate(longRotate);
        shortHand.transform.Rotate(shortRotate);
    }
}
