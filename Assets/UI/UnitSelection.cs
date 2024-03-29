﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelection : MonoBehaviour {

	// Use this for initialization
	void Start () {
        mouseController = GameObject.FindObjectOfType<MouseController>();
	}

    public Text Title;
    public Text Movement;
    public Text Wounds;

    MouseController mouseController;
	
	// Update is called once per frame
	void Update () {
		if (mouseController.Selectedunit != null)
        {
            Title.text = mouseController.Selectedunit.name;
            Movement.text = string.Format("{0} {1}/{2}", "Movement", mouseController.Selectedunit.movesRem, mouseController.Selectedunit.moves);
            Wounds.text = string.Format("{0} {1}/{2}" ,"Health", mouseController.Selectedunit.WoundsRem, mouseController.Selectedunit.Wounds);
        }
	}

    public void set(Unit unit)
    {
        Title.text = unit.name;
        Movement.text = string.Format("{0}/{1}", unit.movesRem, unit.moves);
        Wounds.text = string.Format("{0}/{1}", unit.WoundsRem, unit.Wounds);
    }
}
