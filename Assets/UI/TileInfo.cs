using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileInfo : MonoBehaviour {

    MouseController mouseController;
    void Start()
    {
        mouseController = GameObject.FindObjectOfType<MouseController>();
    }

    public Text TileType;
    public Text TileModifier;
    public Text Unit;
    public Text UnitHP;
	
	// Update is called once per frame
	void Update () {

        if(mouseController.hexUnderMouse != null)
        {
            TileType.text = "Terrain type: " + mouseController.hexUnderMouse.terrain.ToString();
            TileModifier.text ="Damage Passed x " + mouseController.hexUnderMouse.terrainModifier().ToString();
            Unit[] list = mouseController.hexUnderMouse.Units();
            if (list.Length > 0)
            {
                Unit.text = "Unit: " + list[0].name.ToString();
                UnitHP.text = "HP: " + list[0].WoundsRem.ToString();
            }

            else
            {
                Unit.text = "Empty tile";
                UnitHP.text = "Empty tile";
            }
        }
		
	}
}
