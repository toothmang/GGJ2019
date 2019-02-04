using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBehavior : MonoBehaviour {

    public static List<TurretBehavior> Instances = new List<TurretBehavior>();

    public int HitPoints = 1;
    public float MinVelocityForHit = 1.0f;

    public enum FireMode {constant, random, burst, rhythm};
    public enum FireArc {high, low, none};
    public enum MovementMode {wander, patrol, stationary};

    public float projectileScale;
    public float projectileSpeed;
    public GameObject projectilePrefab;
    public Transform FireAt;
    public float FireOffset = 1.2f;
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

    private Vector3 firePos;

    private bool chargingUp = false;
    // Allows you to set the value in the Editor Inspector while maintaining in-code privacy
    [SerializeField]
    private float animTime = 1.0f;

    public Vector3 velocity;
    public Vector3 position;
    public Vector3 targetPosition;
    public Vector3 attack;
    public Quaternion attack_q;

    // Use this for initialization
    public void Start () {
        refireWait = refireDelay + Random.value;
        burstCount = 3;

        origin = transform.position;

        float angle = Random.value;
        float distance = (Random.value * 0.5f + 0.5f) * stepLength;
        waypoint = new Vector3(Mathf.Cos(angle)*distance, origin.y, Mathf.Sin(angle)*distance);

        if (fireMode == FireMode.rhythm)
        {
            RhythmRecorder.Instance.OnHit += () =>
            {
                Fire();
            };
        }
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

    IEnumerator ChargeUp()
    {
        // Block against starting multiple coroutines
        if (chargingUp) yield break;
        chargingUp = true;

        // Can declare your wait amount as a variable and reuse it
        // See also
        // var eof = new WaitForEndOfFrame(); // called once per Update() loop
        var eoff = new WaitForFixedUpdate(); // called once per FixedUpdate() loop. Good for setting transforms/rotations/rigibodies
        //var eoff = new WaitForSeconds(); // called once per x seconds, but is affected by time scale
        //var eoff = new WaitForSecondsRealtime(); // called once per x seconds without time scale 

        Quaternion startRot = transform.rotation;
        Vector3 startScale = transform.localScale;

        for (float t = 0; t < animTime; t += Time.fixedDeltaTime)    // Docs say you can use Time.deltaTime since it picks the right delta per update type
        {
            float it = Mathf.Clamp01(t / 1.0f);
            transform.rotation = Quaternion.Slerp(startRot, Random.rotationUniform, it * it * 0.5f);
            transform.localScale = Vector3.Lerp(startScale * 1.5f, startScale, 1.0f - (it * it));
            yield return eoff;
        }

        // No long-term harm done
        transform.rotation = startRot;
        transform.localScale = startScale;
        
        // Since we set the transform, wait for one more fixed update
        yield return eoff;
        chargingUp = false;
        

        // Can also call yield return null, or simply nothing at the end of a coroutine function and it should terminate just the same.
        yield break;
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

        //refireWait -= Time.deltaTime;
        refireWait -= Time.unscaledDeltaTime;

        bool ready = false;

        // Coroutine should set the flag so this is only called once per shot
        //if (refireWait <= animTime + 1.0e-5 && !chargingUp)
        //{
        //    StartCoroutine(ChargeUp());
        //}

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

        if (ready)
        {
            Fire();
        }
    }

    private void Fire()
    {
        firePos = Vector3.MoveTowards(transform.position, FireAt.position, FireOffset);

        bool safe;

        Quaternion atk_arc = ballistic(out safe, FireAt.position - firePos, projectileSpeed);

        if (safe)
        {
            if (fireAccuracyPercent < 100f)
            {
                atk_arc = Quaternion.Slerp(atk_arc, Random.rotationUniform, 1f - 0.01f * fireAccuracyPercent);
            }

            GameObject clone = SpawnBank.Instance.BulletSpawner.FromPool(firePos, Quaternion.identity, Vector3.one * projectileScale);
            //GameObject clone = Instantiate(projectilePrefab, firePos, atk_arc);

            //Rigidbody rb = (Rigidbody) clone;
            //rb.velocity = new Vector3(projectileSpeed, 0.f, 0.f);
            //clone.GetComponent<Rigidbody>().velocity = atk_arc * new Vector3(projectileSpeed, 0f, 0f);
            //var p = clone.AddComponent<Projectile>();

            //clone.transform.parent = null;

            var p = clone.GetComponent<Projectile>();
            p.StartTime = Time.unscaledTime;
            if (fireArc != FireArc.none)
            {
                p.rigBod.velocity = atk_arc * new Vector3(0f, 0f, -projectileSpeed);
            }
            else
            {
                p.rigBod.velocity = (FireAt.position - firePos).normalized * projectileSpeed;
            }

            var trail = clone.GetComponent<TrailRenderer>();
            if (trail)
            {
                trail.Clear();
            }

            velocity = p.rigBod.velocity;
            position = firePos;
            attack = atk_arc.eulerAngles;
            attack_q = atk_arc;
            targetPosition = FireAt.position;
        }
    }

    private void OnDrawGizmos()
    {
        // Firing position
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(firePos, 0.05f);
        // Fire position
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(position, 0.05f);
        // Target position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPosition, 0.05f);

        // Show axes of attack
        Gizmos.color = Color.red;
        Gizmos.DrawLine(position, position + (attack_q * Vector3.right));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(position, position + (attack_q * Vector3.up));
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(position, position + (attack_q * Vector3.forward));

        Vector3 lastPos = position;
        Vector3 lastVel = velocity;
        Vector3 g = Physics.gravity;
        float dt = 0.1f;

        Gizmos.color = Color.magenta;
        for(float t = dt; t < 3.0f; t += dt)
        {
            Vector3 newPos = position + lastVel * t + (0.5f * g * t * t);
            Gizmos.DrawLine(lastPos, newPos);
            lastPos = newPos;
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
        float atk_incline = 0.0f;
        if (fireArc == FireArc.high)
            atk_incline = Mathf.Max(first, second);
        else if (fireArc == FireArc.low)
            atk_incline = Mathf.Min(first, second);

        //return Quaternion.Euler(atk_incline * Mathf.Rad2Deg, atk_heading * Mathf.Rad2Deg, 0f);
        return Quaternion.Euler(0f, atk_heading * Mathf.Rad2Deg, 0f) * Quaternion.Euler(atk_incline * Mathf.Rad2Deg, 0f, 0f);
        //return Quaternion.Euler(atk_incline * Mathf.Rad2Deg, 0f, 0f) * Quaternion.Euler(0f, atk_heading * Mathf.Rad2Deg, 0f);
        //return Quaternion.Euler(atk_incline * Mathf.Rad2Deg, 0f, 0f);
    }

}
