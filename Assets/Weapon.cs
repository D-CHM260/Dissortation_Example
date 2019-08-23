using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon {

    public string name;
    public string type;
    public int attacks;
    public int range;
    public int strength;
    public int ap;
    public int damage;

    public Weapon()
    {
        name = null;
        type = null;
        attacks = 0;
        range = 0;
        strength = 0;
        ap = 0;
        damage = 0;
    }

    public void Reaper()
    {
        this.name = "Reaper";
        this.type = "Melee";
        this.attacks = 1;
        this.range = 1;
        this.strength = 2;
        this.ap = 3;
        this.damage = 3;
    }

    public void Bolter()
    {
        this.name = "Bolter";
        this.type = "Rapid Fire 1";
        this.attacks = 1;
        this.range = 24;
        this.strength = 2;
        this.ap = 3;
        this.damage = 3;
    }

}
