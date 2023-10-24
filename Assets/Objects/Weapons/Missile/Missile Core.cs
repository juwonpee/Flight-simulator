using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

enum GuidanceMethod_t {
    DirectGuidance,     // LOS is always towards target
    PNGuidance,         //  proper Proportional navigation
    SPNGuidance,        // Simple PN
    // Augmented proportional navigation, used for radar guided missiles
    APNGuidance
};

public class MissileCore : WeaponCore
{

    [SerializeField] float max_thrust;
    public bool is_launched = false;
    [SerializeField] float thrust_time;
    
    [SerializeField] GameObject explosion;
    [SerializeField] float fuse_distance;
    
    [SerializeField] float fuse_min_time;
    
    [SerializeField] float damage;
    [SerializeField] float seeker_fov;
    [SerializeField] Transform seeker;
    [SerializeField] GuidanceMethod_t guidanceMethod;
    [SerializeField] Transform wing_horizontal;
    [SerializeField] Transform wing_vertical;
    // Some wierd bug workaround
    private Vector3 wing_horizontal_angle;
    private Vector3 wing_vertical_angle;
    [SerializeField] float wing_max_deflection;
    [SerializeField] float wing_sensitivity;

    [SerializeField] float navigation_gain;

    // How long the missile will live
    public float ttl;
    public Vector3 mach;
    Rigidbody rb;
    ParticleSystem particles;
    float launchTime;


    // Start is called before the first frame update


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        particles = transform.GetComponentInChildren<ParticleSystem>();
        particles.Stop();
        // Launch();

        wing_horizontal_angle = wing_horizontal.localEulerAngles;
        wing_vertical_angle = wing_vertical.localEulerAngles;
    }

    void Update() {
        if (is_launched) launchTime += Time.deltaTime;

        if (is_launched) {
            mach = transform.InverseTransformDirection(rb.velocity) / 343f;
            // Check if launch happened
            if (is_launched) {
                // set thrust
                transform.GetComponent<MissileAero>().thrust = max_thrust;
                if (launchTime > thrust_time) {
                    particles.Stop();
                    transform.GetComponent<MissileAero>().thrust = 0;
                }
                if (launchTime > ttl) {
                    Debug.Log("Destroy due to ttl");
                    Kill();
                    return;
                }

                // Find targetable objects within range 
                // LayerMask mask = LayerMask.GetMask("Player") + LayerMask.GetMask("UI");
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, fuse_distance);
                foreach (var hitCollider in hitColliders) {
                    // If targetable is within range
                    if (hitCollider.transform.TryGetComponent<Targetable>(out Targetable target) && launchTime > fuse_min_time) {
                        // if collider isnt this missile
                        if (target.transform != transform && launchTime > fuse_min_time) {
                            Debug.Log("Destroy due to proxmimity");
                            Kill();
                            return;
                        }
                        
                    }
                }

                if (target != null) {
                    // Check if object is in FOV
                    Vector3 directionToTarget = target.position - seeker.position;
                    if (seeker_fov/2 <  Vector3.Angle(seeker.forward, directionToTarget)) {
                        target = null;
                        Debug.Log("Kill due to target not in FOV");
                        Kill();
                        return;
                    }
                    // Check if target is in LOS
                    // TODO: LOS CHECK NOT WORKING
                    // LayerMask mask = LayerMask.GetMask("Player");
                    // if (Physics.Linecast(seeker.position, target.transform.position, out RaycastHit hit,mask)) target = null;
                    Debug.DrawLine(seeker.position, target.transform.position, Color.red);

                    Vector2 deflection = new Vector2();
                    switch (guidanceMethod) {
                        case GuidanceMethod_t.DirectGuidance:
                            deflection = DirectGuidance();
                            break;
                        case GuidanceMethod_t.SPNGuidance:
                            deflection = SPNGuidance();
                            break;
                        case GuidanceMethod_t.PNGuidance:
                            deflection = PNGuidance();
                            Debug.Log(deflection);
                            // Dampen inverse to speed as higher speed will make missile controls more sensitive
                            wing_vertical_angle.z = Mathf.Clamp(-deflection.y * wing_sensitivity, -wing_max_deflection * (1/mach.magnitude), wing_max_deflection * (1/mach.magnitude));
                            wing_vertical.localEulerAngles = wing_vertical_angle;
                            wing_horizontal_angle.z = Mathf.Clamp(-deflection.x * wing_sensitivity, -wing_max_deflection * (1/mach.magnitude), wing_max_deflection * (1/mach.magnitude));
                            wing_horizontal.localEulerAngles = wing_horizontal_angle;
                            break;
                    }
                }
                else  {
                    Kill();
                    return;
                }
            }
        }
    }

    // Guidance functions will return desired direction to move
    Vector2 DirectGuidance() {
        // Calculate using local positions cuz global position is hard
        Vector3 directionToTarget = seeker.InverseTransformPoint(target.position);
        return new Vector2(directionToTarget.x, directionToTarget.y).normalized; 
    }

    Vector3 lastVelocity;
    Vector2 SPNGuidance() {
        // Not following any conventional proportional guidance systems (that comes maybe later to implement)
        // Just predicts where the target will be using velocities
        Vector3 relativePosition = transform.InverseTransformPoint(target.position);
        Vector3 relativeVelocity = transform.InverseTransformDirection(rb.velocity - target.GetComponent<Rigidbody>().velocity);
        // estimated time to target accounting for velocity
        // TODO: this is only finding the time to target, not time to intercept point, which would make this more accurate
        float time = relativePosition.magnitude / relativeVelocity.magnitude;
        // estimated point where the missile will intercept
        // TODO: make this point several degrees higher to compensate for gravity
        Vector3 estimatedPoint = relativePosition - relativeVelocity * time;
        // Calculate distance
        float planeDistance = new Vector2(transform.TransformDirection(estimatedPoint).x, transform.TransformDirection(estimatedPoint).z).magnitude;
        float adjustedHeight = Mathf.Tan(2 * Mathf.Deg2Rad) * planeDistance; // Calculate height just at 2 degrees above the target
        estimatedPoint = transform.InverseTransformPoint(transform.TransformPoint(estimatedPoint) + new Vector3(0,0,adjustedHeight));
        
        Debug.DrawLine(target.position, transform.TransformPoint(estimatedPoint), Color.green);
        Debug.DrawLine(transform.position, transform.TransformPoint(estimatedPoint), Color.green);

        
        return new Vector2(estimatedPoint.x, estimatedPoint.y).normalized;

    }

    Vector2 PNGuidance() {
        // https://en.wikipedia.org/wiki/Proportional_navigation
        // Use velocity to calculate Line of sight rate
        Vector3 forwardDirection = transform.InverseTransformDirection(rb.velocity);
        Vector3 relativeVelocity = transform.InverseTransformDirection(rb.velocity - target.GetComponent<Rigidbody>().velocity * Time.deltaTime);
        Vector3 directionToTarget = transform.InverseTransformDirection(target.position - transform.position);
        Vector3 LOS = Utility.GetAngles(forwardDirection, directionToTarget);

        Vector3 predictedDirectionToTarget = transform.InverseTransformDirection(target.position + target.GetComponent<Rigidbody>().velocity * Time.deltaTime - (transform.position + rb.velocity * Time.deltaTime));
        Vector3 predictedLOS = Utility.GetAngles(forwardDirection, predictedDirectionToTarget);

        Vector2 LOSRate = predictedLOS - LOS;
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(LOSRate.y, 0, 0)), Color.green);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(0, LOSRate.x, 0)), Color.green);
        // return navigation_gain * LOSRate * relativeVelocity.z;
        return LOSRate;
    }

    public bool Launchable(Transform target) {
        // If within FOV
        Vector3 directionToTarget = target.position - transform.position;
        if (seeker_fov/2 <  Vector3.Angle(transform.forward, directionToTarget)) return false;
        else return true;
    }

    public override void Launch() {
        Debug.Log("Missile Launch");
        particles.Play();
        is_launched = true;
        Destroy(transform.GetComponent<FixedJoint>());
    }

    protected override void Kill() {
        particles.Stop();
        GameObject tempExplosion = Instantiate(explosion, transform.position, Quaternion.identity);
        Transform smoke = transform.Find("Particle System");
        smoke.parent = null;
        Destroy(smoke.gameObject, 20);
        Destroy(tempExplosion, 2);
        Destroy(gameObject);
    }
}
