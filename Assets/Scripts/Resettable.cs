using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Resettable : MonoBehaviour {

    public bool resetChildren = true;

    public Collider resetter;

    public bool resetting = false;

    float resetStartedAt = 0.0f;

    struct TTF
    {
        public Transform transform;
        public TF tf;
        public Rigidbody rigBod;
    }

    TTF[] initial, last;

    TTF[] MakeTTFs()
    {
        if (resetChildren)
        {
            List<TTF> ttfs = new List<TTF>();

            Stack<Transform> stack = new Stack<Transform>();

            stack.Push(transform);

            while (stack.Any())
            {
                var s = stack.Pop();

                if (s.GetComponent<ResetButton>() == null)
                {
                    ttfs.Add(new TTF
                    {
                        transform = s,
                        tf = new TF(s),
                        rigBod = s.gameObject.GetComponent<Rigidbody>()
                    });
                }

                foreach (Transform c in s)
                {
                    stack.Push(c);
                }
            }

            return ttfs.ToArray();
        }
        else
        {
            return new TTF[]{new TTF{
                transform = transform,
                tf = new TF(transform),
                rigBod = gameObject.GetComponent<Rigidbody>()
            } };
        }
    }

	// Use this for initialization
	void Start () {
        initial = MakeTTFs();

        foreach(var x in initial)
        {
            Debug.Log("Tracked " + x.transform.name + " for resetting");
        }
	}

    private void ResetTF()
    {
        foreach(var it in initial)
        {
            it.tf.Apply(it.transform);
        }
    }

    public void ResetNow()
    {
        // Clear velocities and anything else on rigid bodies
        for (int i = 0; i < initial.Length; i++)
        {
            initial[i].tf.Apply(initial[i].transform);

            var rb = initial[i].rigBod;
            if (rb)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // Clear projectiles
        var projectiles = FindObjectsOfType<Projectile>();
        foreach (var p in projectiles)
        {
            Destroy(p.gameObject);
        }

        var trails = FindObjectsOfType<TrailRenderer>();
        foreach(var t in trails)
        {
            t.Clear();
        }
    }
    
    public IEnumerator RealReset(float resetTime)
    {
        if (resetting) yield break;
        resetting = true;
        resetStartedAt = Time.fixedTime;
        last = MakeTTFs();
        Debug.Log("Starting reset!");

        var eoff = new WaitForFixedUpdate();

        for (float t  = 0.0f; t < resetTime; t += Time.unscaledDeltaTime)
        {
            float it = Mathf.Clamp01(t / resetTime);
            it *= it;

            for (int i = 0; i < initial.Length; i++)
            {
                var newTF = TF.Lerp(last[i].tf, initial[i].tf, it);
                newTF.Apply(initial[i].transform);
            }
            yield return eoff;
        }

        ResetNow();

        resetting = false;
        yield break;
    }
}
