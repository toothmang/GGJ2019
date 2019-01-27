using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saber : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter(Collision collision)
    {
        AudioEvents.PlayAt(collision.contacts[0].point);
    }
}
