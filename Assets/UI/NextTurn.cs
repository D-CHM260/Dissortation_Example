using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextTurn : MonoBehaviour {

	public void TurnProcessing()
    {
        GameObject.FindObjectOfType<HexGrid>().endTurn();
    }
	
}
