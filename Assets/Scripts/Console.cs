using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Console : MonoBehaviour {

    public UnityEngine.UI.Text Text;

    public static Console Instance;

	// Use this for initialization
	void Start () {
        Instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
