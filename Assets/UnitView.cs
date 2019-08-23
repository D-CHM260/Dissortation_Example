using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitView : MonoBehaviour {

    void Start()
    {
        newposition = this.transform.position;
    }
    Vector3 newposition;

    Vector3 currentVelocity;
    float movement = 0.5f;

	public void onUnitMoved(Hex oldHex, Hex newHex)
    {
        Vector3 oldposition = oldHex.PositionFromCamera();
        newposition = newHex.PositionFromCamera();
        currentVelocity = Vector3.zero;


        this.transform.position = oldposition;

        if (Vector3.Distance(this.transform.position, newposition) > 2)
        {
            this.transform.position = newposition;
        }
        else
        {
            GameObject.FindObjectOfType<HexGrid>().animOn = true;
        }

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
