using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TurnInfo : MonoBehaviour {

    void Start()
    {
        grid = GameObject.FindObjectOfType<HexGrid>();
    }

    public Text Turn;
    public Text Team;
    public Text Phase;

    HexGrid grid;

    // Update is called once per frame
    void Update()
    {
        Turn.text = grid.turnNum.ToString();
        Team.text = grid.CurrentT.Name;
        Phase.text = grid.currentPhase.ToString();

    }
}
