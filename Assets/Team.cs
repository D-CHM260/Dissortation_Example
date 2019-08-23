using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Stores which units belong in which team and squad
public class Team {




    public string Name;
    public HashSet<Unit> units;
    public List<List<Unit>> squads;



    public Team(string NAME)
    {
        Name = NAME;
        units = new HashSet<Unit>();
        squads = new List<List<Unit>>();
    }


    public void AddUnit(Unit u)
    {
        units.Add(u);
    }

    public List<Unit> squad(Unit u)
    {
        List<Unit> list = new List<Unit>();
        foreach (Unit unit in this.units)
        {
            if (unit.squad == u.squad && unit.WoundsRem>0)
            {
                list.Add(unit);
            }

        }

        if(list.Count<= 0)
        {
            Debug.Log("squad not found or out of units");
        }
        return list;
    }

    public bool Teamfinder(Unit u)
    {
        bool check = false;
        foreach (Unit unit in this.units)
        {
            if (unit.name == u.name)
            {
                check = true;
            }
        }
        return check;
    }

    public Unit squadLead(Unit unit)
    {
        Unit IT = null;
        if(unit != null)
        {
            foreach (Unit u in squad(unit))
            {
                if (u.WoundsRem > 0)
                {
                    IT = u;
                    break;
                }
            }

        }

        else
        {
            Debug.Log("no Unit passed");
        }
        

        return IT;
    }

}
