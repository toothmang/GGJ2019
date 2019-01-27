using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBehavior : MonoBehaviour {

    public static List<TurretBehavior> Instances = new List<TurretBehavior>();

    public int HitPoints = 1;
    public float MinVelocityForHit = 1.0f;

    public enum FireMode {constant, random, burst};
    public enum FireArc {high, low};
    public enum MovementMode {wander, patrol, stationary};

    public float projectileScale;
    public float projectileSpeed;
    public GameObject projectilePrefab;
    public Transform FireAt;
    public Transform FireOffset;
    public float refireDelay;
    public FireMode fireMode;
    public FireArc fireArc;

    public float fireAccuracyPercent = 100f;

    public ParticleSystem impactExploder;
    public ParticleSystem glancingExploder;

    public MovementMode movementMode;
    public Vector3 waypoint;
    private Vector3 origin;
    public float stepLength;
    public float speedLimit;
    public float thrust;

    private float refireWait;
    private int burstCount;

    // Use this for initialization
    public void Start () {
        refireWait = refireDelay + Random.value;
        burstCount = 3;

        origin = transform.position;

        float angle = Random.value;
        float distance = (Random.value * 0.5f + 0.5f) * stepLength;
        waypoint = new Vector3(Mathf.Cos(angle)*distance, origin.y, Mathf.Sin(angle)*distance);
    }

    // Update is called once per frame
    void Update () {
        if (HitPoints <= 0 && SpawnBank.Instance.TurretSpawner)
        {
            Instances.Remove(this);
            SpawnBank.Instance.TurretSpawner.UnSpawnObject(gameObject);
            Console.Instance.TurretsDestroyed++;
        }
    }

    void OnCollisionEnter(Collision collision) {
        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;
        Vector3 forward = GetComponent<Rigidbody>().velocity;

        // Takes particle system and aligns UP with the impact normal
        Quaternion impact_align = Quaternion.FromToRotation(Vector3.up, normal);
        // Takes particle system and aligns UP with the reflection of the forward vector,
        //     as if glancing off of the collided surface.
        Quaternion impact_offset = Quaternion.FromToRotation(-forward, normal);
        Quaternion impact_oblique = impact_align * impact_offset;
        Vector3 position = contact.point;

        float angle = Mathf.Acos( Vector3.Dot(normal.normalized, forward.normalized) );

        if (angle > Mathf.PI/4) {
            ParticleSystem PartiSys = (ParticleSystem)Instantiate(glancingExploder, position, impact_oblique);
            var shape = PartiSys.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.radius = 0.1f;
        } else {
            ParticleSystem PartiSys = (ParticleSystem)Instantiate(impactExploder, position, impact_align);
            var shape = PartiSys.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
        }

        if (collision.relativeVelocity.magnitude >= MinVelocityForHit)
        {
            HitPoints -= 1;
        }
        var p = collision.gameObject.GetComponent<Projectile>();
        if (p)
        {
            SpawnBank.Instance.BulletSpawner.UnSpawnObject(p.gameObject);
        }
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
        Quaternion atk_arc = ballistic(out safe, FireAt.position - transform.position, projectileSpeed);

        if (fireAccuracyPercent < 100f)
        {
            atk_arc = Quaternion.Slerp(atk_arc, Random.rotationUniform, 1f - 0.01f * fireAccuracyPercent);
        }

        if (safe) {
            GameObject clone = SpawnBank.Instance.BulletSpawner.FromPool(FireOffset.position, atk_arc, Vector3.one * projectileScale);

            //Rigidbody rb = (Rigidbody) clone;
            //rb.velocity = new Vector3(projectileSpeed, 0.f, 0.f);
            //clone.GetComponent<Rigidbody>().velocity = atk_arc * new Vector3(projectileSpeed, 0f, 0f);
            //var p = clone.AddComponent<Projectile>();
            var p = clone.GetComponent<Projectile>();
            p.StartTime = Time.unscaledTime;
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
