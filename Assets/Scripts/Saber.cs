using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Saber : MonoBehaviour {

    public static List<Saber> Instances = new List<Saber>();

    public Transform FireOffset;

    public float ReflectWeight = 20.0f;
    public float GradualTime = 2.0f;

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
	}
	
	// Update is called once per frame
	void Update () {
		if (clip.Any() && Time.time - lastFired > fireRate)
        {
            Fire();
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
    }

    // On exit, check if we can redirect any projectiles
    private void OnCollisionExit(Collision collision)
    {
        var p = collision.gameObject.GetComponent<Projectile>();
        if (!p) return;

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
                p.StartCoroutine(p.GuideTowards(toCheck.First(), GradualTime));
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
