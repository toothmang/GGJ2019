using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public float ExpireTime = 20.0f;
    public float StartTime = 0.0f;
    public Rigidbody rigBod;
	// Use this for initialization
	void Start () {
        StartTime = Time.unscaledTime;
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.unscaledTime - StartTime > ExpireTime)
        {
            StopAllCoroutines();
            Destroy(gameObject);
        }
	}

    public IEnumerator GuideTowards(Transform target, float guideTime)
    {
        var eoff = new WaitForFixedUpdate();

        var startVel = rigBod.velocity;

        rigBod.useGravity = false;
        for (float t = 0.0f; t < guideTime; t += Time.unscaledDeltaTime)
        {
            var endVel = (target.position - rigBod.position).normalized * startVel.magnitude;

            float it = t / guideTime;
            rigBod.velocity = Vector3.Slerp(rigBod.velocity, endVel, it);

            yield return eoff;
        }
        rigBod.useGravity = true;

        yield break;
    }
}
