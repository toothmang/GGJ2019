using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetButton : MonoBehaviour {

    public Resettable resettable;
    public float resetTime = 2.0f;

    Collider touching = null;
    float touchedAt = 0f;

    Vector3 readyPos = new Vector3(), pressedPos = new Vector3();
    float pressSpeed = 0.25f;

    bool hasReset = false;

	// Use this for initialization
	void Start ()
    {
        readyPos = transform.position;
        pressedPos = transform.position - new Vector3(0, 0.1f, 0.0f);
	}

    //IEnumerator Press(bool _down)
    //{
    //    isDown = _down;

    //    //if (isDown && resettable)
    //    //{
    //    //    resettable.StartCoroutine(resettable.RealReset(resetTime));
    //    //}

    //    var eoff = new WaitForFixedUpdate();

    //    Vector3 a = transform.position;
    //    Vector3 b = pressedPos;
    //    if (!isDown)
    //    {
    //        b = readyPos;
    //    }

    //    float dist = (transform.position - b).magnitude;
    //    float timeToMove = pressSpeed / dist;
    //    float speed = Time.fixedDeltaTime * pressSpeed;


    //    for(float t = 0; t < resetTime; t += Time.fixedDeltaTime)
    //    {
    //        transform.position = Vector3.Lerp(a, b, t);
    //        yield return eoff;
    //    }

    //    transform.position = b;

    //    moving = false;

    //    if (isDown)
    //    {
    //        StartCoroutine(Press(false));
    //        if (resettable)
    //        {
    //            resettable.ResetNow();
    //        }
    //    }
    //    else
    //    {
    //        touching = null;
    //    }

    //    yield break;
    //}


    //private void OnTriggerEnter(Collision collision)
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(string.Format("Hit by {0}, tag {1}, layer {2}", other.name,
            other.tag, other.gameObject.layer));
        if (!touching && other.tag == "Hands")
        {
            touching = other;
            touchedAt = Time.time;
            hasReset = false;
            //StartCoroutine(Press(true));
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (touching && other == touching && !hasReset)
        {
            float dt = Time.time - touchedAt;

            if (dt >= this.resetTime)
            {
                resettable.StartCoroutine(resettable.RealReset(resetTime));
                //resettable.ResetNow();
                hasReset = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (touching && other == touching)
        {
            touching = null;
        }
    }
}
