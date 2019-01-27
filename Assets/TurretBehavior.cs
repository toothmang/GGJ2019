using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBehavior : MonoBehaviour {

    public static List<TurretBehavior> Instances = new List<TurretBehavior>();

    public enum FireMode {constant, random, burst};
    public enum FireArc {high, low};
    public enum MovementMode {wander, patrol, stationary};

    public float projectileScale;
    public float projectileSpeed;
    public GameObject projectilePrefab;
    public GameObject target;
    public Transform FireOffset;
    public float refireDelay;
    public FireMode fireMode;
    public FireArc fireArc;

    public float fireAccuracyPercent = 100f;

    public MovementMode movementMode;
    public Vector3 waypoint;
    private Vector3 origin;
    public float stepLength;
    public float speedLimit;
    public float thrust;

    private float refireWait;
    private int burstCount;

    // Use this for initialization
    void Start () {
        refireWait = refireDelay + Random.value;
        burstCount = 3;

        origin = transform.position;

        float angle = Random.value;
        float distance = (Random.value * 0.5f + 0.5f) * stepLength;
        waypoint = new Vector3(Mathf.Cos(angle)*distance, origin.y, Mathf.Sin(angle)*distance);

        Instances.Add(this);
    }

    // Update is called once per frame
    void Update () {

    }

    // FixedUpdate is called once per physics update (50Hz default)
    void FixedUpdate () {
        Vector3 vel = GetComponent<Rigidbody>().velocity;

        if (movementMode != MovementMode.stationary) {
            vel *= 0.9f;

            Vector3 direction = waypoint - transform.position;

            if (direction.magnitude > 1f) {
                vel += thrust * direction.normalized;
            }
            else if (movementMode == MovementMode.patrol) {
                Vector3 temp = origin;
                origin = waypoint;
                waypoint = temp;
            }
            else /* movementMode == wander */
            {
                float angle = Random.value * Mathf.PI * 2.0f;
                float distance = (Random.value * 0.5f + 0.5f) * stepLength;
                waypoint = transform.position + new Vector3(Mathf.Cos(angle)*distance, 0.0f, Mathf.Sin(angle)*distance);
            }
        }

        if (vel.magnitude > speedLimit)
            vel *= speedLimit / vel.magnitude;

        // Remember to actually apply the changes we've made to the velocity!
        GetComponent<Rigidbody>().velocity = vel;

        refireWait -= Time.deltaTime;

        bool ready = false;

        if (fireMode == FireMode.constant) {
            if (refireWait <= 0) {
                refireWait += refireDelay;
                ready = true;
            }
        }
        if (fireMode == FireMode.random) {
            if (refireWait <= 0) {
                refireWait += refireDelay * 2 * Random.value;
                ready = true;
            }
        }
        if (fireMode == FireMode.burst) {
            if (refireWait <= 0 && burstCount > 0) {
                refireWait += refireDelay * 0.05f;
                burstCount -= 1;
                ready = true;
            }
            if (refireWait <= 0 && burstCount <= 0) {
                refireWait += refireDelay;
                burstCount = 2;
                ready = true;
            }
        }

        if (!ready) return;

        bool safe;
        Quaternion atk_arc = ballistic(out safe, target.transform.position - transform.position, projectileSpeed);

        if (fireAccuracyPercent < 100f)
        {
            atk_arc = Quaternion.Slerp(atk_arc, Random.rotationUniform, 1f - 0.01f * fireAccuracyPercent);
        }

        if (safe) {
            GameObject clone = (GameObject) Instantiate(projectilePrefab, FireOffset.position, atk_arc);
            clone.transform.localScale = new Vector3(projectileScale, projectileScale, projectileScale);

            //Rigidbody rb = (Rigidbody) clone;
            //rb.velocity = new Vector3(projectileSpeed, 0.f, 0.f);
            //clone.GetComponent<Rigidbody>().velocity = atk_arc * new Vector3(projectileSpeed, 0f, 0f);
            var p = clone.AddComponent<Projectile>();
            p.rigBod = clone.GetComponent<Rigidbody>();
            p.rigBod.velocity = atk_arc * new Vector3(0f, 0f, -projectileSpeed);

            //clone.GetComponent<Rigidbody>().velocity = atk_arc * (target.transform.position - transform.position);
            //clone.GetComponent<Rigidbody>().velocity = clone.transform.TransformVector(new Vector3(0f, 0f, projectileSpeed));
            //clone.GetComponent<Rigidbody>().velocity = clone.transform.TransformVector(new Vector3(projectileSpeed, 0f, 0f));
        }

    }

    Quaternion ballistic(out bool safe, Vector3 target, float speed) {
        float G = -Physics.gravity.y;

        Vector3 atk_proj_horiz = target;
        atk_proj_horiz.y = 0f;
        float atk_horiz = atk_proj_horiz.magnitude;

        float atk_elev = target.y;

        float det = speed*speed*speed*speed - G * (G*atk_horiz*atk_horiz + 2f*atk_elev*speed*speed);
        if (det < 0f) {
            safe = false;
            return Quaternion.identity;
        } else {
            safe = true;
        }

        det = Mathf.Sqrt(det);

        float first  = Mathf.Atan((speed*speed + det) / (G * atk_horiz));
        float second = Mathf.Atan((speed*speed - det) / (G * atk_horiz));

        float atk_heading = Mathf.Atan2( -target.z, target.x ) - Mathf.PI/2;
        float atk_incline;
        if (fireArc == FireArc.high)
            atk_incline = Mathf.Max(first, second);
        else
            atk_incline = Mathf.Min(first, second);


        //return Quaternion.Euler(atk_incline * Mathf.Rad2Deg, atk_heading * Mathf.Rad2Deg, 0f);
        return Quaternion.Euler(0f, atk_heading * Mathf.Rad2Deg, 0f) * Quaternion.Euler(atk_incline * Mathf.Rad2Deg, 0f, 0f);
        //return Quaternion.Euler(atk_incline * Mathf.Rad2Deg, 0f, 0f) * Quaternion.Euler(0f, atk_heading * Mathf.Rad2Deg, 0f);
        //return Quaternion.Euler(atk_incline * Mathf.Rad2Deg, 0f, 0f);
    }

}
