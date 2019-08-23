using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QPath;


//Class for storing Hex information
public class Hex : IQPathTile {

    public readonly int Col;  //column
    public readonly int Row;  //Row
    public readonly int Aggregate;  //aggreg
    float radius = 1f;


    
    public enum Terrain { Cover, Elevation, Normal};
    public Terrain terrain;

    public readonly HexGrid HexGrid;

    //Type determination needed

    public Hex(HexGrid hexGrid, int c, int r, int t)
    {
        this.HexGrid = hexGrid;
        this.Col = c;
        this.Row = r;
        this.Aggregate = -(c + r);
        switch (t)
        {
            case 1:
                this.terrain = Terrain.Normal;
                break;
            case 2:
                this.terrain = Terrain.Elevation;
                break;
            case 3:
                this.terrain = Terrain.Cover;
                break;           
        }

        units = new HashSet<Unit>();
    }

    public Vector3 position()
    {
        return new Vector3(
            horiz() * (this.Col + this.Row / 2f),
            0,
            vert()* this.Row);
    }

    public float height()
    {
        return radius * 2;
    }

    public float width()
    {
        return height() * Mathf.Sqrt(3) / 2;
    }

    public float vert()
    {
       return height() * 0.75f;
    }

   public float horiz()
    {
        return width();
    }

    HashSet<Unit> units;

    public void AddUnit(Unit unit)
    {
        if(units == null)
        {
            units = new HashSet<Unit>();
        }

        units.Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        if(units != null)
        {
            units.Remove(unit);
        }
        
    }

    public Unit [] Units()
    {
        return units.ToArray();
    }

	//Gets position of hex
    public Vector3 PositionFromCamera()
    {
        return HexGrid.GetHexPosition(this);
    }

	//Hex movment cost used in pathfinding
    public int BaseMovementCost()
    {

        Unit[] u = Units();
        int i = 1;
        if (u.Length > 0)
        {
            i = 100;
        }

        return i;
       
    }

	//Function creats neighbours list
    Hex[] neighbours;

    public IQPathTile[] GetNeighbours()
    {
        if (this.neighbours != null) return this.neighbours;

        List<Hex> neighbours = new List<Hex>();

        neighbours.Add(HexGrid.GetHexAt(Col +1, Row+0));
        neighbours.Add(HexGrid.GetHexAt(Col -1, Row+0));
        neighbours.Add(HexGrid.GetHexAt(Col +0, Row+1));
        neighbours.Add(HexGrid.GetHexAt(Col +0, Row-1));
        neighbours.Add(HexGrid.GetHexAt(Col +1, Row-1));
        neighbours.Add(HexGrid.GetHexAt(Col -1, Row+1));

        List<Hex> neighbours2 = new List<Hex>();

        foreach(Hex h in neighbours)
        {
            if(h != null)
            {
                neighbours2.Add(h);
            }
        }

        this.neighbours = neighbours2.ToArray();

        return this.neighbours;

    }

	//Functiont to get the neighbour list
    public List<Hex> GetNeighboursList()
    {;
        List<Hex> neighbours = new List<Hex>();

        neighbours.Add(HexGrid.GetHexAt(Col + 1, Row + 0));
        neighbours.Add(HexGrid.GetHexAt(Col - 1, Row + 0));
        neighbours.Add(HexGrid.GetHexAt(Col + 0, Row + 1));
        neighbours.Add(HexGrid.GetHexAt(Col + 0, Row - 1));
        neighbours.Add(HexGrid.GetHexAt(Col + 1, Row - 1));
        neighbours.Add(HexGrid.GetHexAt(Col - 1, Row + 1));        

        return neighbours;

    }

	//Cost to enter after modifiers
    public float AggregateCostToEnter(float costSoFar, IQPathTile sourceTile, IQPathUnit theUnit)
    {
        return ((Unit)theUnit).AggregateTurnsToEnterHex(this, costSoFar);
    }

	//Cost estimate, Simple distance measurment right now
    public static float CostEstimate(IQPathTile aa, IQPathTile bb)
    {

        return Distance((Hex)aa, (Hex)bb);



    }

	//Distance measurment function
    public static float Distance(Hex a, Hex b)
    {
        // WARNING: Probably Wrong for wrapping
        int dCOL = Mathf.Abs(a.Col - b.Col);
        

        int dROW = Mathf.Abs(a.Row - b.Row);

        int Dis = Mathf.Max
            (
                dCOL,
                dROW,
                Mathf.Abs(a.Aggregate - b.Aggregate)
            );
        return Dis ;
    }

	//Movement values for different terrains
    public float terrainModifier()
    {
        float modifier = 0;      
        switch (terrain)
        {
            case Terrain.Normal:
                modifier = 1;
                break;
            case Terrain.Cover:
                modifier = 0.8f;
                break;
            case Terrain.Elevation:
                modifier = 1.2f;
                break;
        }
        return modifier;
    }


}
