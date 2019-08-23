using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitView : MonoBehaviour {

    void Start()
    {
        oldposition = newposition = this.transform.position;
    }

    Vector3 oldposition;
    Vector3 newposition;

    Vector3 currentVelocity;
    float movement = 0.5f;

	public void onUnitMoved(Hex oldHex, Hex newHex)
    { 
        //Animation
        oldposition = oldHex.PositionFromCamera();
        newposition = newHex.PositionFromCamera();
        GameObject.FindObjectOfType<HexGrid>().animOn = true;

    }
    

    void Update()
    {
        this.transform.position = Vector3.SmoothDamp(this.transform.position, newposition, ref currentVelocity, movement);
        if(Vector3.Distance(this.transform.position, newposition) < 0.1f)
        {
            GameObject.FindObjectOfType<HexGrid>().animOn = false;
        }
    }
}
