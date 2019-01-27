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
            Destroy(gameObject);
        }
	}
}
