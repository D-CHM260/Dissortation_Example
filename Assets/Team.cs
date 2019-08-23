using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team {

    public string Type;
    public List<Squad> squads;

    public Team(string TYPE)
    {
        Type = TYPE;
        squads = new List<Squad>();
    }

    public Squad FindSquadByUnit(Unit unit)
    {

        Squad b = new Squad(null);

        foreach (Squad s in squads)
        {
            if (s.MatchUnit(unit))
            {
                b = s;
            }
        }

        

        return b;

    }
    public bool CheckPresence(string Type)
    {

        bool Exists = false;

        foreach (Squad s in squads)
        {
            if (Type == s.identifier)
            {
                Exists = true;
            }
        }

        return Exists;

    }

    public Squad ReturnSquadByType(string Type)
    {
        Squad retrunSquad = new Squad(Type);
        foreach (Squad s in squads)
        {
            if (Type == s.identifier)
            {
                retrunSquad = s;  
                return s;
            }
        }

        return retrunSquad;

    }

    public HashSet<Unit> GetUnitsInTeam()
    {
        HashSet<Unit> units = new HashSet<Unit>();
        foreach (Squad s in squads)
        {
            foreach (Unit b in s.units)
            {
                units.Add(b); 
            }


        }
        return units;
    }

    public void addSquad(Squad s)
    {
        squads.Add(s);
    }
    public int FindIndexOfSquad(string Type)
    {
        int i = 0;
        foreach (Squad s in squads)
        {
            if (Type == s.identifier)
            {
                return i; 
            }
         i++;
        }

        return i;
    }


}
