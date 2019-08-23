using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squad {

    public string identifier;
    public List<Unit> units;

    public Squad(string i)
    {
        identifier = i;
        units = new List<Unit>();
    }

    public void addUnit(Unit unit)
    {
        units.Add(unit);
    }

    public void removeUnit(Unit unit)
    {
        int i = 0;
        foreach (Unit u in units)
        {           
            if (unit == u)
            {
                units.RemoveAt(i);
            }
            i++;
        }
    }

    public bool MatchUnit(Unit unit)
    {
        bool present = false;
        foreach (Unit u in units)
        {
            if(u == unit)
            {
                present = true;
            }

            else
            {
                present = false;
            }
        }

        return present;

    }

}
