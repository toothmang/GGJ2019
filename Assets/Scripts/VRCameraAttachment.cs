using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRCameraAttachment : MonoBehaviour {

    /// <summary>
    /// The target anchor point for the VR camera
    /// </summary>
    public Transform AttachTo;

    /// <summary>
    /// The necessary manufactured evil child of AttachTo and parent of VRCamera. 
    /// Exists solely to apply the VR Camera's local transform in reverse so it can
    /// be centered at the anchor point
    /// </summary>
    public Transform Counterweight;

    /// <summary>
    /// The actual VR Camera
    /// </summary>
    public Transform VRCamera;

    public Transform LookTarget;

    public bool LookAtTarget = false;

    public float LookRange = 20f;

    public float LookAtTheTime = 0f;

    public float LookDuration = 1f;

    public Quaternion InitialCounterweightRot;

    //public Ship ship;

    public struct TransformOffset
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    TransformOffset offset;

    //bool buttonState = false, lastButtonState = false;


    public void Attach(Transform tf)
    {
        AttachTo = tf;

        Counterweight.position = Vector3.zero;
        Counterweight.rotation = Quaternion.identity;

        Counterweight.parent = AttachTo;

        Counterweight.localPosition = Vector3.zero;
        Counterweight.localRotation = Quaternion.identity;
        //VRCamera.parent = Counterweight;


        //Resync();
    }

    // We want to compute a transform that will transform the camera position to the
    // AttachTo transform (negate the camera's local transform and rotation)
    void Resync()
    {
        //offset.position = -VRCamera.localPosition;
        //offset.rotation = Quaternion.Inverse(VRCamera.localRotation);
        if (Counterweight && VRCamera)
        {
            // Compute the difference in orientation between the headset 
            Quaternion invQ = Quaternion.Inverse(VRCamera.localRotation);

            Counterweight.localRotation = invQ;
            Counterweight.localPosition = -(invQ * VRCamera.localPosition);

            //if (WebVRManager.Instance.vrState == WebVRState.ENABLED)
            //{
            //    Debug.Log("WebVR only yess??");
            //    Counterweight.localPosition = -VRCamera.localPosition;
            //}
            //else
            //{

            //}

            //Vector3 newScaleAdjust = VectorExtensions.Divide(Vector3.one, ship.transform.localScale);
            //Counterweight.localPosition = Counterweight.localRotation * -(VectorExtensions.Multiply(VRCamera.localPosition, newScaleAdjust));



            Debug.Log(string.Format("Setting counterweight rotation to {0}, position to {1}", Counterweight.localRotation.ToString(),
                Counterweight.localPosition.ToString()));
        }
    }

    public void AttachPlayerCamera(Transform tf)
    {
        if (tf && VRCamera)
        {
            Attach(tf);
        }
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Space))// || ship.leftHand.GetButtonDown("Submit") || ship.rightHand.GetButtonDown("Submit"))
        {
            Debug.Log("Resyncing");
            Resync();
        }

        //       //if (VRContext.Instance == null || !VRContext.Instance.initialized) return;

        //       // Compute the transforms between the VR headset and the attachment


        //       lastButtonState = buttonState;
        //       //buttonState = VRContext.Left.input.direction.pressed;

        //       if (buttonState && !lastButtonState)
        //       {
        //           LookAtTarget = !LookAtTarget;
        //           LookAtTheTime = Time.time;

        //           if (LookAtTarget)
        //           {
        //               InitialCounterweightRot = Counterweight.transform.rotation;
        //               Resync();
        //           }
        //       }

        //       var t = Mathf.Clamp01(Time.time - LookAtTheTime) / LookDuration;
        //       if (LookAtTarget)
        //       {
        //           var targetRot = Quaternion.LookRotation((LookTarget.transform.position - transform.position).normalized, transform.up);

        //           if (t < 1f)
        //           {
        //               Counterweight.transform.rotation = Quaternion.Slerp(InitialCounterweightRot, targetRot, t);
        //           }
        //           else
        //           {
        //               Counterweight.transform.rotation = targetRot;
        //           }

        //       }
    }
}