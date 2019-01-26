﻿using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class ControllerInteraction : MonoBehaviour
{
    public WebVRController controller;
    private FixedJoint attachJoint = null;
    private Rigidbody currentRigidBody = null;
    private List<Rigidbody> contactRigidBodies = new List<Rigidbody> ();

    private Animator anim;

    bool isGrabbing = false;

    AverageOverFrames currentVelocity = new AverageOverFrames(10);
    AverageOverFrames currentAngularVelocity = new AverageOverFrames(10);
    Vector3 lastTractorPos;
    Quaternion lastTractorRot;


    void Awake()
    {
        attachJoint = GetComponent<FixedJoint> ();
    }

    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        controller = gameObject.GetComponent<WebVRController>();
    }

    void Update()
    {
        float normalizedTime = controller.GetAxis("Grip");

        bool lastGrabbing = isGrabbing;

        isGrabbing = normalizedTime > 0.2f;
        //if (controller.GetButtonDown("Trigger") || controller.GetButtonDown("Grip"))
        //{
        //    isGrabbing = true;
        //}

        //if (controller.GetButtonUp("Trigger") || controller.GetButtonUp("Grip"))
        //{
        //    isGrabbing = false;
        //}

        // Handle state changes for buttons
        if (isGrabbing)
        {
            if (!currentRigidBody)
            {
                Pickup();
            }
            else
            {
                Vector3 currentTractorPos = currentRigidBody.position;
                Quaternion currentTractorRot = currentRigidBody.rotation;
                Vector3 tv = (currentTractorPos - lastTractorPos) / Time.deltaTime;
                lastTractorPos = currentTractorPos;


                currentVelocity.Update(tv);

                // Update angular velocity
                Quaternion av = currentTractorRot * Quaternion.Inverse(lastTractorRot);
                Vector3 ave = av.eulerAngles;
                Vector3 avd = new Vector3(Mathf.DeltaAngle(0f, ave.x), Mathf.DeltaAngle(0f, ave.y), Mathf.DeltaAngle(0f, ave.z));
                lastTractorRot = currentTractorRot;
                currentAngularVelocity.Update(avd / Time.deltaTime);
            }
            
        }
        else if (lastGrabbing)
        {
            Drop();
        }

        // Use the controller button or axis position to manipulate the playback time for hand model.
        anim.Play("Take", -1, normalizedTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Interactable")
            return;

        contactRigidBodies.Add(other.gameObject.GetComponent<Rigidbody> ());
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "Interactable")
            return;

        contactRigidBodies.Remove(other.gameObject.GetComponent<Rigidbody> ());
    }

    public void Pickup() {
        currentRigidBody = GetNearestRigidBody ();

        if (!currentRigidBody)
            return;

        currentRigidBody.MovePosition(transform.position);
        attachJoint.connectedBody = currentRigidBody;
        //attachJoint.anchor = currentRigidBody.gameObject.transform.InverseTransformPoint(debrisHit.point);
    }

    public void Drop() {
        if (!currentRigidBody)
            return;

        var cb = attachJoint.connectedBody;
        attachJoint.connectedBody = null;
        if (cb)
        {
            cb.velocity = currentVelocity.value;
            cb.angularVelocity = currentAngularVelocity.value * Mathf.Deg2Rad;
        }

        currentVelocity.Clear();
        currentAngularVelocity.Clear();
        currentRigidBody = null;
    }

    private Rigidbody GetNearestRigidBody() {
        Rigidbody nearestRigidBody = null;
        float minDistance = float.MaxValue;
        float distance = 0.0f;

        foreach (Rigidbody contactBody in contactRigidBodies) {
            distance = (contactBody.gameObject.transform.position - transform.position).sqrMagnitude;

            if (distance < minDistance) {
                minDistance = distance;
                nearestRigidBody = contactBody;
            }
        }

        return nearestRigidBody;
    }
}
