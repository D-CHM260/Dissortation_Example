using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            if (unit.squad == u.squad)
            {
                list.Add(unit);
            }

        }
        return list;
    }

}
