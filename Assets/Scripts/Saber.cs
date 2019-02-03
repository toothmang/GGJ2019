using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Saber : MonoBehaviour {

    public static List<Saber> Instances = new List<Saber>();

    public Transform FireOffset;

    public float ReflectWeight = 20.0f;
    public float GradualTime = 2.0f;

    public bool Boomerang = true;
    public float BoomerangScale = 100.0f;

    Rigidbody rigBod;

    public struct Bullet
    {
        public float speed;
        public Projectile p;
    };

    Queue<Bullet> clip = new Queue<Bullet>();

    public float fireRate = 0.25f;
    public float lastFired = 0f;

    public enum ReflectMode
    {
        None,
        Direct,
        Gradual,
        Fire
    }

    public enum ReflectTarget
    {
        None,
        NearestGaze,
        Turret,
        OtherHand
    }

    public ReflectMode reflectMode = ReflectMode.None;
    public ReflectTarget reflectTarget = ReflectTarget.Turret;

	// Use this for initialization
	void Start () {
        Instances.Add(this);
        rigBod = GetComponent<Rigidbody>();
        if (!rigBod)
        {
            foreach(Transform child in transform)
            {
                var cb = child.GetComponent<Rigidbody>();
                if (cb)
                {
                    rigBod = cb;
                    break;
                }
            }
        }

        // Give material unique color
        var mrs = GetComponentsInChildren<MeshRenderer>();
        Color c = new Color(Random.value, Random.value, Random.value);
        foreach(var mr in mrs)
        {
            mr.material.color = c;
        }
	}
	
	// Update is called once per frame
	void Update () {
		if (clip.Any() && Time.time - lastFired > fireRate)
        {
            Fire();
        }
    }

    private void FixedUpdate()
    {
        if (Boomerang && Console.Instance.totalTrigger > 0f)
        {
            rigBod.AddForce((Console.Instance.weightedCenter - rigBod.position).normalized * BoomerangScale * Console.Instance.totalTrigger);
        }
    }

    void AddToClip(Projectile p)
    {
        p.rigBod.isKinematic = true;
        p.rigBod.position = new Vector3(0, -100f, 0);
        clip.Enqueue(new Bullet
        {
            speed = Mathf.Max(10.0f, p.rigBod.velocity.magnitude),
            p = p
        });
    }

    void Fire()
    {
        var p = clip.Dequeue();

        if (p.p)
        {
            p.p.rigBod.isKinematic = false;
            p.p.rigBod.position = FireOffset.position;
            p.p.rigBod.velocity = p.speed * FireOffset.up;
            lastFired = Time.time;
        }
        


    }

    private void OnCollisionEnter(Collision collision)
    {
        AudioEvents.PlayAt(collision.contacts[0].point);

        var turret = collision.gameObject.GetComponent<TurretBehavior>();
        if (turret)
        {
            StopAllCoroutines();
        }
    }

    // On exit, check if we can redirect any projectiles
    private void OnCollisionExit(Collision collision)
    {
        var p = collision.gameObject.GetComponent<Projectile>();
        if (!p) return;

        Console.Instance.ShotsDeflected++;

        List<Transform> toCheck = new List<Transform>();
        switch (reflectTarget)
        {
            case ReflectTarget.Turret:
                {
                    toCheck = TurretBehavior.Instances
                        .Select(tb => tb.transform)
                        .OrderBy(t => (t.position - (collision.contacts.Length > 0 ? collision.contacts[0].point : collision.transform.position)).sqrMagnitude)
                        .ToList();
                    break;
                }
            case ReflectTarget.OtherHand:
                {
                    toCheck = Instances
                        .Where(sb => sb != this)
                        .Select(sb => sb.transform)
                        .OrderBy(t => (t.position - (collision.contacts.Length > 0 ? collision.contacts[0].point : collision.transform.position)).sqrMagnitude)
                        .ToList();
                    break;
                }
            case ReflectTarget.None:
            default:
                break;
        }
        switch (reflectMode)
        {
            case ReflectMode.Direct:
                p.rigBod.velocity = p.rigBod.velocity.magnitude * (toCheck.First().position - p.transform.position).normalized;
                break;
            case ReflectMode.Gradual:
                p.StartCoroutine(Projectile.GuideTowards(p.rigBod, toCheck.First(), GradualTime));
                break;
            case ReflectMode.Fire:
            {
                var otherOffset = Instances
                    .Where(sb => sb != this)
                    .Select(sb => sb)
                    .ToList();
                if (otherOffset.Any())
                {
                    otherOffset.First().AddToClip(p);
                }
                break;
            }
                
            case ReflectMode.None:
            default:
                break;
        }
    }

    
}
