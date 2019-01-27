using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Saber : MonoBehaviour {

    public static List<Saber> Instances = new List<Saber>();

    public float ReflectWeight = 20.0f;
    public float GradualTime = 2.0f;

    public enum ReflectMode
    {
        None,
        Direct,
        Gradual
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
                        .OrderBy(t => (t.position - collision.contacts[0].point).sqrMagnitude)
                        .ToList();
                    break;
                }
            case ReflectTarget.OtherHand:
                {
                    toCheck = Instances
                        .Where(sb => sb != this)
                        .Select(sb => sb.transform)
                        .OrderBy(t => (t.position - collision.contacts[0].point).sqrMagnitude)
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
                StartCoroutine(GuideProjectile(p, toCheck.First()));
                break;
            case ReflectMode.None:
            default:
                break;
        }
    }

    IEnumerator GuideProjectile(Projectile p, Transform target)
    {
        var eoff = new WaitForFixedUpdate();

        var startVel = p.rigBod.velocity;
        //var endVel = (target.position - p.rigBod.position).normalized * startVel.magnitude;

        p.rigBod.useGravity = false;
        for (float t = 0.0f; t < GradualTime; t += Time.fixedDeltaTime)
        {
            var endVel = (target.position - p.rigBod.position).normalized * startVel.magnitude;

            float it = t / GradualTime;
            p.rigBod.velocity = Vector3.Slerp(p.rigBod.velocity, endVel, it);

            yield return eoff;
        }
        p.rigBod.useGravity = true;

        yield break;
    }
}
