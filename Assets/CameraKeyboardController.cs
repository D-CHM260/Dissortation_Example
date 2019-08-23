using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Controls camera movement
public class CameraKeyboardController : MonoBehaviour {

    float PanSpeed = 10;
	void Update () {
        Vector3 MovementVector = new Vector3
            (
            Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")
            );
        this.transform.Translate(MovementVector * PanSpeed * Time.deltaTime, Space.World);
    }
}
