using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] public Transform cam;
    [SerializeField] Transform player;
    private FlightStats stats;
    [SerializeField] Transform boresight;
    [SerializeField] Transform velocity_vector;

    [SerializeField] GameObject horizon;
    [SerializeField] List<Transform> horizon_bars;
    [SerializeField] float horizon_bars_fov;
    [SerializeField] GameObject tracking_prefab;
    [SerializeField] List<Transform> targetable_list;
    [SerializeField] GameObject targeting_object;
    [SerializeField] Color green;
    [SerializeField] Color red;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        stats = player.transform.GetComponent<FlightStats>();

        for (int x = 0; x < 36; x++) {
            horizon_bars.Add(Instantiate(horizon).transform);
            horizon_bars[x].SetParent(boresight);
            // horizon_bars[x].parent = boresight;
            horizon_bars[x].GetComponent<PitchBar>().Angle = x*10-180;
            horizon_bars[x].GetComponent<PitchBar>().cam = cam;
            horizon_bars[x].GetComponent<PitchBar>().player = player;
            horizon_bars[x].gameObject.SetActive(true);
        }



    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Boresight shold be in front of the plane
        boresight.position = cam.transform.GetComponent<Camera>().WorldToScreenPoint(cam.position + player.forward.normalized);
        boresight.localEulerAngles = new Vector3(0, 0, -player.localEulerAngles.z);

        // Velocity vector
        velocity_vector.position = cam.transform.GetComponent<Camera>().WorldToScreenPoint(cam.position + player.GetComponent<Rigidbody>().velocity.normalized);
        velocity_vector.localEulerAngles = new Vector3(0, 0, -player.localEulerAngles.z);
        
        // Horizon
        float angle = player.rotation.eulerAngles.x;

        // Only activate bars that are within HUD fov of player
        for (int x = 0; x < 36; x++) {
            if (Mathf.Abs(Mathf.DeltaAngle(horizon_bars[x].GetComponent<PitchBar>().Angle, player.eulerAngles.x)) < horizon_bars_fov/2) {
                horizon_bars[x].gameObject.SetActive(true);
            }
            else
                horizon_bars[x].gameObject.SetActive(false);
        }

        // Put overlay on radar targets
        // Clear previous squares
        foreach (var target in targetable_list) {
            Destroy(target.gameObject);
        }
        targetable_list.Clear();
        // New ones
        List<tracking_t> tracking = player.GetComponent<FCR>().tracking;
        int index = 1;
        foreach(var track in tracking ?? Enumerable.Empty<tracking_t>()) {
            // Create object in front of target
            targetable_list.Add(Instantiate(tracking_prefab, cam.transform.GetComponent<Camera>().WorldToScreenPoint(cam.position + (track.target.position - player.position).normalized), Quaternion.identity).transform);
            targetable_list.LastOrDefault().transform.localEulerAngles = new Vector3(0, 0, -player.localEulerAngles.z);
            targetable_list.LastOrDefault().Find("Index").GetComponent<Text>().text = string.Format($"{index:0}");
            targetable_list.LastOrDefault().Find("Velocity").GetComponent<Text>().text = string.Format($"{track.velocity.z:0}");
            targetable_list.LastOrDefault().SetParent(transform);
            index++;
        }

        // Targeting cursor
        Transform targeting = player.GetComponent<FCR>().targeting;
        bool targeting_launchable = player.GetComponent<FCR>().targeting_launchable;
        if (targeting == null) {
            targeting_object.gameObject.SetActive(false);
        }
        else {
            targeting_object.gameObject.SetActive(true);
            targeting_object.transform.position = cam.transform.GetComponent<Camera>().WorldToScreenPoint(cam.position + (targeting.position - player.position).normalized);
            targeting_object.transform.localEulerAngles = new Vector3(0, 0, -player.localEulerAngles.z);
            if (!targeting_launchable) {
                targeting_object.GetComponentInChildren<Image>().color = green;
            }
            else {
                targeting_object.GetComponentInChildren<Image>().color = red;
            }
        }

    }
}
