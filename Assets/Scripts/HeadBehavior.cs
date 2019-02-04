using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBehavior : MonoBehaviour {

    public int hitPoints = 20;
    public float minHitSpeed = 3.0f;

    public static HeadBehavior Instance;

	// Use this for initialization
	void Start () {
        Instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //IEnumerator HeadHit()
    //{
    //    var cam = WebVRManager.Instance.mainCamera;

    //    var camParent = cam.transform.parent;
    //    var parentRot = camParent.rotation;

    //    for(float t = 0.0f; t < 0.2f; t += Time.unscaledDeltaTime)
    //    {
    //        float it = t / 0.2f;

    //        Quaternion random = Quaternion.Slerp(Random.rotationUniform, parentRot, it);

    //        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
    //    }

    //    camParent.rotation = parentRot;
    //}

    private void OnCollisionEnter(Collision collision)
    {
        var p = collision.gameObject.GetComponent<Projectile>();
        if (p)
        {
            if (collision.relativeVelocity.magnitude >= minHitSpeed)
            {
                hitPoints--;
                //StartCoroutine(HeadHit());
            }
            
        }
    }
}
