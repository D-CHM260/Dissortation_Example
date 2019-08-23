using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//single damage log entries
public class DamageLogEntry {

    public string Defender;
    public string AttackedBy;
    public string ForXDamage;
    public string OnTurn;
    public HexGrid.Phase OnPhase;
    public bool Overwatch;
    public string Type;

    public DamageLogEntry(string def, string att, string dmg, string turn, HexGrid.Phase p, bool Overwatch, string type)
    {
        this.Defender = def;
        this.AttackedBy = att;
        this.ForXDamage = dmg;
        this.OnTurn = turn;
        this.OnPhase = p;
        this.Overwatch = Overwatch;
        this.Type = type;
    }
}
