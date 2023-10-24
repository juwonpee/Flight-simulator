using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using UnityEngine;
using UnityEngine.AI;

// Radar target tracking information



public class FCR : MonoBehaviour {
    [SerializeField] float radar_min_range;
    [SerializeField] float radar_max_range;
    [SerializeField] float radar_sensitivity;
    [SerializeField] int radar_fov;
    [SerializeField] int radar_boresight_fov;
    [SerializeField] int radar_speed;
    [SerializeField] Transform radar;
    public Vector2 radar_direction;

    public Transform targeting;
    public bool targeting_launchable = false;
    public List<tracking_t> tracking;

    // Start is called before the first frame update
    void Start() {
        radar_direction = new Vector2(-radar_fov / 2, -radar_fov / 2);
        tracking = new List<tracking_t>();
    }

    // Update is called once per frame
    void FixedUpdate() {
        // Separated by 10 degrees horizontal sectors
        // Scanning one second takes one second
        // Start from Vertical(X axis)
        radar_direction.y += radar_fov * Time.fixedDeltaTime;
        if (radar_direction.y > radar_fov / 2) {
            radar_direction.y = -radar_fov / 2;
            radar_direction.x += radar_boresight_fov;
            if (radar_direction.x > radar_fov / 2) {
                radar_direction.x = -radar_fov / 2;
            }
        }
    }

    void Update() {

        /*
        1. Find Trackable
        2. Add trackable into ordered list of tracking time
        3. Remove any non trackable objects from list
        4. When tracking, select first object of list
        5. When tracking another object, circular shift list and select first object
        6. Update values of tracking list
        */

        // 1. Find all trackable units
        List<Transform> trackable = Search();

        // 2. Add trackable into list ordered by found time
        foreach (var track in trackable ?? Enumerable.Empty<Transform>()) {
            // if (!tracking.Contains(track)) tracking.Add(track);
            if (tracking == null) {     // For some reason tracking becomes null when recompiling
                tracking = new List<tracking_t>();
                tracking.Add(new tracking_t(track));
            }// Throws exception if not null checked
            else if (!tracking.Any(tracking_t => tracking_t.target == track)) {
                tracking.Add(new tracking_t(track));
            }
        }

        // 3. Remove any non trackable objects from list
        for (int x = tracking.Count - 1; x >= 0; x--) {
            if (!trackable.Contains(tracking[x].target)) {
                // Check if tracking object is also targeted
                if (tracking[x].target == targeting) targeting = null;
                tracking.RemoveAt(x);
            }
        }

        if (tracking.Count > 0) {
            if (Input.GetKeyDown(KeyCode.C)) {
                // 4. When selecting to target, select first object of list
                if (targeting == null) {
                    targeting = tracking[0].target;
                }
                // 5. When targeting another object, Rotate list and select first object
                else {
                    Transform temp = tracking[0].target;
                    tracking.RemoveAt(0);
                    tracking.Add(new tracking_t(temp));
                    targeting = tracking[0].target;
                }
            }
        }

        // 6. Update tracking list
        for (int x = 0; x < tracking.Count; x++) {
            Vector3 direction = transform.InverseTransformPoint(tracking[x].target.position);
            Vector3 velocity = transform.InverseTransformDirection(tracking[x].target.GetComponent<Rigidbody>().velocity);
            target_iff_type_t iff_type;
            if (tracking[x].target.GetComponent<Targetable>().team == 0) iff_type = target_iff_type_t.neutral;
            else if (tracking[x].target.GetComponent<Targetable>().team == GetComponent<Targetable>().team) iff_type = target_iff_type_t.friendly;
            else iff_type = target_iff_type_t.foe;

            tracking[x] = new tracking_t(tracking[x].target, direction, velocity, iff_type);
        }

        // if targeting check if launchable
        if (targeting != null) {
            targeting_launchable = transform.GetComponent<F16_Weapons>().TargetWeapon(targeting.transform);
        }

        // If firing
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (targeting_launchable) transform.GetComponent<F16_Weapons>().Fire(targeting);
            else transform.GetComponent<F16_Weapons>().Fire(null);
        }

    }


    List<Transform> Search() {
        // Find targetable objects within range then filter only ones within FOV
        Collider[] hitColliders = Physics.OverlapSphere(radar.position, radar_max_range);
        List<Transform> trackable = new List<Transform>();

        foreach (var hitCollider in hitColliders) {
            if (hitCollider.transform.TryGetComponent<Targetable>(out Targetable target)) {

                // Remove minimum range;
                if (radar_min_range > Vector3.Distance(radar.position, target.transform.position)) continue;

                // Check if within LOS
                LayerMask mask = LayerMask.GetMask("Player");
                // TODO: Does not linecast when target is above player
                if (Physics.Linecast(radar.position, target.transform.position, out RaycastHit hit,mask)) continue;
                
                // Check if within FOV
                Vector3 directionToTarget = target.transform.position - radar.position;
                Vector2 deflection = new Vector2(Vector3.SignedAngle(transform.forward, directionToTarget, transform.TransformDirection(transform.right)), Vector3.SignedAngle(transform.forward, directionToTarget, transform.TransformDirection(transform.up)));

                // If within FOV
                if (Mathf.Abs(Vector3.Angle(transform.forward, directionToTarget)) > radar_fov/2) continue;
                // if (Vector3.Angle(deflection, radar_direction) > radar_boresight_fov) continue;

                // Add to targetable
                trackable.Add(target.transform);

                Debug.DrawLine(radar.position, target.transform.position, Color.black);
            }
        }
        return trackable;
    }
}
