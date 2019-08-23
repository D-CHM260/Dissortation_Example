using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QPath;
using System;
using System.Linq;


//Class to store unit instances
public class Unit : IQPathUnit
{

    HexGrid hexG = GameObject.FindObjectOfType<HexGrid>();

    public string name;
    public int Wounds;
    public int WoundsRem;
    public int Ballistic_Skill;
    public int Weapon_Skill;
    public int Strength;
    public int Toughness;
    public int initiative;
    public int attacks;
    public int leadership;
    public int moves;
    public int movesRem;
    public int save;
    public string squad;
    public bool melee;
    public bool ranged;
    public Weapon[] weapons = new Weapon[2] { new Weapon(), new Weapon() };

    public Hex Hex { get; protected set; }

    public delegate void UnitMovedDel(Hex oldhex, Hex newhex);
    public UnitMovedDel onUnitMoved;

    List<Hex> hexPath;


    public Unit(string NAME, string Type, string Squad)
    {

        switch (Type)
        {
            case "Marine": this.newMarine(NAME, Squad); break;

        }

    }

    public void newMarine(string NAME, string Squad)
    {
        this.name = NAME;
        this.Wounds = 6;
        this.WoundsRem = 6;
        this.Ballistic_Skill = 2;
        this.Weapon_Skill = 2;
        this.Strength = 4;
        this.Toughness = 5;
        this.initiative = 3;
        this.attacks = 4;
        this.leadership = 9;
        this.moves = 4;
        this.movesRem = 4;
        this.save = 2;
        this.squad = Squad;
        this.melee = false;
        this.ranged = false;
        this.weapons[0].Reaper();
        this.weapons[1].Bolter();
    }



    public void SetHex(Hex newhex)
    {
        Hex oldhex = Hex;
        if (Hex != null)
        {
            Hex.RemoveUnit(this);
        }

        Hex = newhex;

        Hex.AddUnit(this);

        if (onUnitMoved != null)
        {
            onUnitMoved(oldhex, newhex);
        }
    }

    public void dummy_pathing_function()
    {
        /*QPath.CostEstimate ced = (IQPathTile a, IQPathTile b) => 
        (return Hex.Distance(a, b););*/


        Hex[] hs = QPath.QPath.FindPath<Hex>(
            Hex.HexGrid,
            this,
            Hex,
            Hex.HexGrid.GetHexAt(Hex.Col + 4, Hex.Row + 10),
            Hex.CostEstimate
            );

        //Hex[] hs = System.Array.ConvertAll(p, a => (Hex)a);
        Debug.Log("Got PathFidning Path of Length" + hs.Length);
        setHexPath(hs);

    }

    public void ClearOutHexPath()
    {
        this.hexPath = new List<Hex>();
    }

    public void setHexPath(Hex[] hexArray)
    {
        this.hexPath = new List<Hex>(hexArray);

        //if(hexPath.Count > 0)
        //{
        //this.hexPath.Dequeue();
        //}
    }

    public bool WaitingForOrder()
    {
        if (movesRem > 0 && (hexPath == null || hexPath.Count == 0))
        {
            return false;
        }

        else
        {
            return true;
        }
    }

    public bool DoMove()
    {

        if (movesRem <= 0)
        {
            return false;
        }

        if (hexPath == null || hexPath.Count == 0)
        {
            return false;
        }

        Hex currentHex = hexPath[0];
        Hex newHex = hexPath[1];
        Hex LastHex = hexPath[hexPath.Count - 1];
        Unit[] unitCheck = newHex.Units();
        bool check = true;
        if (unitCheck.Length > 0 /*&& (hexPath.Count)>movesRem && FinalHexUnits.Length>0*/)
        {
            check = false;
            //Debug.Log("Unit Already Present");
        }



        if (check)
        {
            if (1 > movesRem && movesRem < moves)
            {
                return false;
            }

            hexPath.RemoveAt(0);

            if (hexPath.Count == 1)
            {
                hexPath = null;
            }
            SetHex(newHex);
            movesRem = Mathf.Max(movesRem - 1, 0);
        }

        else
        {
            hexPath = QPath.QPath.FindPath<Hex>(hexG, this, this.Hex, LastHex, Hex.CostEstimate).ToList();
            unitCheck = hexPath[1].Units();
            if (unitCheck.Length > 0)
            {
                hexPath = null;
            }
        }




        return hexPath != null && movesRem > 0;


    }

    public int MovementCostToEnterHex(Hex hex)
    {
        return hex.BaseMovementCost();
    }

    public float AggregateTurnsToEnterHex(Hex hex, float turnsToDate)
    {
        float BaseTurnsToEnterHex = MovementCostToEnterHex(hex) / moves;

        if (BaseTurnsToEnterHex < 0)
        {

            return -99999;
        }

        if (BaseTurnsToEnterHex > 1)
        {

            BaseTurnsToEnterHex = 1;
        }

        float TurnsToDateWole = Mathf.Floor(turnsToDate);
        float TurnsToDateFraction = turnsToDate - TurnsToDateWole;

        float turnsUsedAfterThismove = TurnsToDateFraction + BaseTurnsToEnterHex;

        if (turnsUsedAfterThismove > 1)
        {


            turnsUsedAfterThismove = 1;

        }



        return TurnsToDateWole + turnsUsedAfterThismove;

    }

    public float MovmementValue(IQPathTile SourceTile, IQPathTile destinationTile)
    {
        return 1;
    }

    public Hex[] HexQueue()
    {
        return (this.hexPath == null) ? null : this.hexPath.ToArray();
    }

    public void refreshmovment()
    {
        movesRem = moves;
    }


    public int Fire(Unit Defender, bool overwatch, Weapon weap)
    {
        Weapon RangedWeap = weap;
        int attackNumber = RangedWeap.attacks;
        float distance = Hex.Distance(Hex, Defender.Hex);

        /*Debug.Log(
            "Weapon: " + 
            RangedWeap.name +
            " Attacks: " +
            attackNumber +
            " Distance: " +
            distance);*/

        if ((RangedWeap.range / 2) > distance)
        {
            attackNumber = attackNumber * 2;
            //Debug.Log("Attack Doubled: " + attackNumber);
        }



        int WoundRolls = 10;

        while (attackNumber > 0 && !overwatch)
        {
            int roll = UnityEngine.Random.Range(1, 6);
            if (roll < this.Ballistic_Skill)
            {
                attackNumber = 0;
            }
            else
            {
                WoundRolls++;
            }
            attackNumber--;
        }



        while (attackNumber > 0 && overwatch)
        {
            //Debug.Log("should not fire");
            int roll = UnityEngine.Random.Range(1, 6);
            if (roll < 6)
            {
                attackNumber = 0;
            }
            else
            {
                WoundRolls++;
            }
            attackNumber--;
        }

        //Debug.Log("WoundRolls: " + WoundRolls);


        int WoundsPassed = 0;

        while (WoundRolls > 0)
        {
            int roll = UnityEngine.Random.Range(1, 6) + 2;
            //Debug.Log(roll);
            if (RangedWeap.strength > Defender.Toughness)
            {
                //Debug.Log("(RangedWeap.strength > Defender.Toughness)");

                if (RangedWeap.strength > (Defender.Toughness * 2))
                {
                    //Debug.Log("(RangedWeap.strength > (Defender.Toughness * 2))");
                    if (roll > 2)
                    {
                        WoundsPassed++;
                        WoundRolls--;
                    }

                    else
                    {
                        WoundRolls--;
                    }
                }
                else if (roll > 3)
                {
                    WoundsPassed++;
                    WoundRolls--;
                }
                else
                {
                    WoundRolls--;
                }
            }
            else if (RangedWeap.strength == Defender.Toughness)
            {
                //Debug.Log("(RangedWeap.strength == Defender.Toughness)");
                if (roll > 4)
                {
                    WoundsPassed++;
                    WoundRolls--;
                }
                else
                {
                    WoundRolls--;
                }
            }
            else if (RangedWeap.strength < Defender.Toughness)
            {
                //Debug.Log("(RangedWeap.strength < Defender.Toughness)");
                if ((RangedWeap.strength * 2) < Defender.Toughness)
                {
                    //Debug.Log("((RangedWeap.strength * 2) < Defender.Toughness)");

                    if (roll >= 6)
                    {
                        WoundsPassed++;
                        WoundRolls--;
                        //Debug.Log("Rolled: " + roll);
                    }
                    else
                    {
                        WoundRolls--;
                    }
                }
                else if (roll > 5)
                {
                    WoundsPassed++;
                    WoundRolls--;
                }
            }
            else if ((RangedWeap.strength * 2) < Defender.Toughness)
            {
                //Debug.Log("((RangedWeap.strength * 2) < Defender.Toughness)");

                if (roll >= 6)
                {
                    WoundsPassed++;
                    WoundRolls--;
                    //Debug.Log("Rolled: " + roll);
                }
                else
                {
                    WoundRolls--;
                }
            }

        }

        //Debug.Log("Wounds Passed: " + WoundsPassed);

        int DamageingShots = 0;
        while (WoundsPassed > 0)
        {
            //Debug.Log(Defender.Hex.terrainModifier());
            int roll = (UnityEngine.Random.Range(1, 6)); /*+ Defender.Hex.terrainModifier())*/
            if ((roll - RangedWeap.ap) >= (Defender.save))
            {
                WoundsPassed = 0;
            }
            else
            {
                DamageingShots++;
            }
        }
        //Debug.Log("Damage Passed: " + DamageingShots);

        float damage = (float)DamageingShots * Defender.Hex.terrainModifier();
        return (int)Math.Round(damage);


    }


    public void damageTaken(int damage)
    {
        GameObject unitThis = hexG.GetUnitGO(this);
        Transform t = unitThis.transform;
        string dmgTXT = damage.ToString();
        PopUpController.CreateTXT(dmgTXT, t);
        this.WoundsRem = this.WoundsRem - damage;    
    }
    public bool checkCharge(HashSet<Unit> EnemyUnits)
    {
        bool CanUnitChargeAnybody = false;

        foreach (Unit u in EnemyUnits)
        {
            if (Hex.Distance(this.Hex, u.Hex) < 12)
            {
                CanUnitChargeAnybody = true;
            }
        }

        return CanUnitChargeAnybody;

    }
}   
