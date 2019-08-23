using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraKeyboardController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
    float PanSpeed = 3;
	// Update is called once per frame
	void Update () {
        Vector3 MovementVector = new Vector3
            (
            Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")
            );
        this.transform.Translate(MovementVector * PanSpeed * Time.deltaTime, Space.World);
    }
}
