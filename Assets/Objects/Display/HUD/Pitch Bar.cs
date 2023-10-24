

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

using Vector3 = UnityEngine.Vector3;

public class PitchBar : MonoBehaviour
{
    public Transform cam;
    public Transform player;
    public float Angle;
    public Sprite horizon;
    public Sprite other;
    public Text text;
    public Image bar;
    // Start is called before the first frame update
    void Start()
    {
        if (Angle == 0) bar.sprite = horizon;
        else bar.sprite = other;
        
        text.text = string.Format($"{-Angle}");

        // Change name
        transform.name = string.Format($"Horizon: {-Angle}");
        
        transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));
        transform.localPosition = new Vector3(0,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        // Get pitch of plane
        float pitch = player.eulerAngles.x;

        // Calculate how high/low object should be
        float fov = cam.GetComponent<Camera>().fieldOfView;
        float pixelHeight = cam.GetComponent<Camera>().pixelHeight;
        float angleDiff = Mathf.DeltaAngle(Angle, pitch);

        float position = Mathf.Tan(angleDiff * Mathf.Deg2Rad) / Mathf.Tan(fov / 2 * Mathf.Deg2Rad) * pixelHeight;

        // transform.GetComponent<RectTransform>().position = new Vector3(0,position,0);
        transform.localPosition = new Vector3(0,position,0);

        // float roll = player.eulerAngles.z;
        // float pitch = player.eulerAngles.x;

        // // transform.localEulerAngles = new Vector3(0,0,-roll);

        // float fov = cam.GetComponent<Camera>().fieldOfView;
        // float pixelHeight = cam.GetComponent<Camera>().pixelHeight;
        // float angleDiff = Mathf.DeltaAngle(pitch, Angle);

        // // Y distance relative to camera
        // float position = (Mathf.Tan(angleDiff * Mathf.Deg2Rad) / Mathf.Tan(fov / 2 * Mathf.Deg2Rad)) * pixelHeight / 2;

        // transform.position = cam.transform.GetComponent<Camera>().WorldToScreenPoint(cam.position + player.forward);


        // transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y -position, transform.localPosition.z);

        
    }
}
