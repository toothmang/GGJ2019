using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Console : MonoBehaviour {

    public UnityEngine.UI.Text Text;

    public static Console Instance;

    public int TurretsDestroyed = 0;
    public int ShotsDeflected = 0;
    public float leftTrigger = 0;
    public float rightTrigger = 0;
    public float totalTrigger = 0;
    public Vector3 weightedCenter = Vector3.zero;
	// Use this for initialization
	void Start () {
        Instance = this;
	}
	
	// Update is called once per frame
	void Update () {
        leftTrigger = 0;
        rightTrigger = 0;
        if (WebVRController.Left && WebVRController.Right)
        {
            leftTrigger = WebVRController.Left.GetAxis("Trigger");
            rightTrigger = WebVRController.Right.GetAxis("Trigger");
        }

        if (leftTrigger > 0 || rightTrigger > 0)
        {
            float totalStrength = leftTrigger + rightTrigger;

            weightedCenter = ((leftTrigger / totalStrength) * WebVRController.Left.transform.position)
                + ((rightTrigger / totalStrength) * WebVRController.Right.transform.position);
            totalTrigger = (leftTrigger + rightTrigger) * 0.5f;
        }
        else
        {
            totalTrigger = 0f;
        }

        Text.text = string.Format("Turrets destroyed: {0}\nShots deflected: {1}\nLeft trigger: {2}\nRight trigger: {3}\nTotal: {4}",
                TurretsDestroyed, ShotsDeflected, leftTrigger, rightTrigger, totalTrigger);
    }
}
