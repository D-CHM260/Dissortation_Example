﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QPath;
using System.IO;

public class HexGrid : MonoBehaviour, IQPathWorld {

	// Use this for initialization
	void Start () {
        GenerateMap();
        //singleUnit();
        //CurrentT = new Team("CurrentT");
        Player = new Team("Player");
        AI = new Team("AI");
        
        spawnareaAI = spawnArea(12, 30);
        spawnareaPlayer = spawnArea(23, 4);

        BuildArmies();
        
        spawnArmy3();

        setCurrent("Player");

        currentPhase = Phase.Movement;

        paintArmy();
        //CurrentT = Player;
        //CurrentUnits = CurrentT.units;
        
    }

    public bool animOn = false;
    public GameObject HexPrefab;
    private Hex[,] hexes;
    private Dictionary<Hex, GameObject> hexToGameObjectMap;
    private Dictionary<GameObject, Hex> gameObjectToHexMap;
    private Dictionary<GameObject, Unit> gameObjectToUnit;
    private Dictionary<Unit, GameObject> UnitToGameObject;
    public Material[] HexMaterials;
    public Material[] PawnMaterial;
    public GameObject TestUnit;
    public Team Player;
    public Team AI;
    public Team CurrentT;
    public int turnNum;
    public List<Hex> spawnareaPlayer = new List<Hex>();
    public List<Hex> spawnareaAI = new List<Hex>();
    public enum Phase { Movement, Fire, Charge, Fight };
    public Phase currentPhase;

    int dimensionsY = 40;
    int dimensionsZ = 80;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(MovementCheck());
        }
               
    }

    public IEnumerator MovementCheck()
    {
        foreach (Unit u in CurrentT.units)
        {
            yield return UnitMove(u);
        }
        
    }

    public IEnumerator UnitMove(Unit u)
    {
        while (u.DoMove())
        {
            Debug.Log("true return");
            yield return new WaitForSeconds(0.5f);
            
        }
    }

    public void FinishPhase()
    {

        foreach (Unit u in CurrentT.units)
        {
            u.refreshmovment();
        }

    }

    public void setCurrent(string team)
    {
        switch (team)
        {
            case "Player":
                CurrentT = Player;

                ; break;
            case "AI":
                CurrentT = AI;

                ; break;
        }
    }

    public void GenerateMap()
    {
        hexes = new Hex[dimensionsZ, dimensionsY];
        hexToGameObjectMap = new Dictionary<Hex, GameObject>();
        gameObjectToHexMap = new Dictionary<GameObject, Hex>();

        for (int column = 0; column < dimensionsZ; column++)
        {
            for(int row = 0; row < dimensionsY; row++)
            {
                int r = Random.Range(0, HexMaterials.Length);
                Hex h = new Hex(this, column, row, r);
               
                GameObject hexGO = (GameObject)Instantiate(
                    HexPrefab, h.position(), 
                    Quaternion.identity, 
                    this.transform);

                MeshRenderer mr =hexGO.GetComponentInChildren<MeshRenderer>();
                mr.material = HexMaterials[r];

                

                hexGO.GetComponentInChildren<TextMesh>().text = string.Format("{0},{1}", column, row);
                AppendHexes(h, column, row);
                hexToGameObjectMap[h] = hexGO;
                gameObjectToHexMap[hexGO] = h;

                //Unit u = new Unit("Marine", "Marine", "Squad 1");
                //creatunit(u, TestUnit, 23, 3);
                //CurrentT.AddUnit(u);
                
            }
        }
    }

    public void singleUnit()
    {
        Unit u = new Unit("Marine1", "Marine", "Squad 1");
        creatunit(u, TestUnit, 23, 3);
        Unit i = new Unit("Marine2", "Marine", "Squad 2");
        creatunit(i, TestUnit, 25, 3);
        Unit o = new Unit("Marine3", "Marine", "Squad 3");
        creatunit(o, TestUnit, 24, 3);
    }

    public void creatunit(Unit unit, GameObject prefab, int x, int y)
    {

        if(UnitToGameObject == null || gameObjectToUnit == null)
        {
            UnitToGameObject = new Dictionary<Unit, GameObject>();
            gameObjectToUnit = new Dictionary<GameObject, Unit>();
        }

        if(CurrentT == null)
        {
            CurrentT = new Team("CurrentT");
        }
        
        Hex myHex = GetHexAt(x, y);
        GameObject myHexGo = hexToGameObjectMap[myHex];
        unit.SetHex(myHex);


        GameObject unitGo = (GameObject)Instantiate(prefab, myHexGo.transform.position,
                    Quaternion.identity, myHexGo.transform);


        unit.onUnitMoved += unitGo.GetComponent<UnitView>().onUnitMoved;

        //unit.onUnitMoved(myHex, myHex);

        CurrentT.AddUnit(unit);

        UnitToGameObject.Add(unit, unitGo);
        gameObjectToUnit.Add(unitGo, unit);

    }

    public Hex GetHexAt(int x, int y)
    {
        return hexes[x, y];
    }

    public Vector3 GetHexPosition(int q, int r)
    {
        Hex hex = GetHexAt(q, r);

        return GetHexPosition(hex);
    }

    public Vector3 GetHexPosition(Hex hex)
    {
        return hex.position();//Camera.main.transform.position, dimensions, dimensions
    }


    public void AppendHexes(Hex hex, int col, int row)
    {
        hexes[col, row] = hex;
    }

    public GameObject GetHexGO(Hex h)
    {
        if (hexToGameObjectMap.ContainsKey(h))
        {
            return hexToGameObjectMap[h];
        }

        return null;
    }
    public Hex GetHexFromGameObject(GameObject hexGO)
    {
        if (gameObjectToHexMap.ContainsKey(hexGO))
        {
            return gameObjectToHexMap[hexGO];
        }

        return null;
    }

    public GameObject GetUnitGO(Unit u)
    {
        if (UnitToGameObject.ContainsKey(u))
        {
            return UnitToGameObject[u];
        }

        return null;
    }
    public Unit GetUnitFromGameObject(GameObject unitGO)
    {
        if (gameObjectToUnit.ContainsKey(unitGO))
        {
            return gameObjectToUnit[unitGO];
        }

        return null;
    }

    public void BuildArmies()
    {
        using (var reader = new StreamReader(@"assets\units.csv"))
        {
            List<string> UnitID = new List<string>();
            List<string> UnitType = new List<string>();
            List<string> Team = new List<string>();
            List<string> Squad = new List<string>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                UnitID.Add(values[0]);
                UnitType.Add(values[1]);
                Team.Add(values[2]);
                Squad.Add(values[3]);

            }
            int i = 0;
            foreach (string ID in UnitID)
            {
                Unit newunit = new Unit(ID, UnitType[i], Squad[i]);

                switch (Team[i])
                {
                    case "Player":
                        Player.AddUnit(newunit);
                        //Debug.Log(newunit.name + newunit.squad);
                        ; break;
                    case "AI":

                        AI.AddUnit(newunit);
                        //Debug.Log(newunit.name + newunit.squad);
                        ; break;

                }


                i++;
            }

        }
    }

    public void spawnArmy()
    {
        int i = 0;
        int o = 0;
        foreach (Unit u in Player.units)
        {
            creatunit(u, TestUnit, o + 20, i + 3);
            i++;


        }

        //Player = CurrentT;
        //CurrentT = new Team("CurrenT");
        o = 1;
        foreach (Unit u in AI.units)
        {
            creatunit(u, TestUnit, o + 20, i + 3);
            i--;
 
        }
        //AI = CurrentT;
        //CurrentT = new Team("CurrenT");

    }

    public void spawnArmy2()
    {

        int i = 0;      
        foreach (Unit u in Player.units)
        {
            creatunit(u, TestUnit, spawnareaPlayer[i].Col, spawnareaPlayer[i].Row);
            i = i + (spawnareaPlayer.Count/Player.units.Count);

        }

        //Player = CurrentT;
        //CurrentT = new Team("CurrenT");
        i = 0;
        foreach (Unit u in AI.units)
        {
            creatunit(u, TestUnit, spawnareaAI[i].Col, spawnareaAI[i].Row);
            i++;

        }
        //AI = CurrentT;
        //CurrentT = new Team("CurrenT");

    }

    public void spawnArmy3()
    {

        int i = 0;
        List<Hex> NextDoor = new List<Hex>();

        foreach (Unit u in Player.units)
        {
            if (u == squad(Player, u)[0])
            {
                Player.squads.Add(squad(Player, u));

            }
        }

        foreach (Unit u in AI.units)
        {
            if (u == squad(AI, u)[0])
            {
                AI.squads.Add(squad(AI, u));
            }
        }

        foreach (List<Unit> list in Player.squads)
        {
            creatunit(list[0], TestUnit, spawnareaPlayer[i].Col, spawnareaPlayer[i].Row);
            NextDoor = list[0].Hex.GetNeighboursList();
            
            i = i + (spawnareaPlayer.Count / Player.squads.Count);
            int o = 0;
            foreach(Unit u in list)
            {
                if(u != list[0])
                {
                    creatunit(u, TestUnit, NextDoor[o].Col, NextDoor[o].Row);
                    o++;
                }
            }


        }

        i = 0;
        foreach (List<Unit> list in AI.squads)
        {
            creatunit(list[0], TestUnit, spawnareaAI[i].Col, spawnareaAI[i].Row);
            NextDoor = list[0].Hex.GetNeighboursList();
            
            i = i + (spawnareaAI.Count / AI.squads.Count);
            int o = 0;
            foreach (Unit u in list)
            {
                if (u != list[0])
                {
                    creatunit(u, TestUnit, NextDoor[o].Col, NextDoor[o].Row);
                    o++;
                }
            }


        }

    }

    public void paintArmy()
    {

        List<Unit> Squad = new List<Unit>();

        foreach (Unit u in Player.units)
        {
            
            if(u == squad(Player, u)[0])
            {
                MeshRenderer mr = GetUnitGO(u).GetComponentInChildren<MeshRenderer>();
                mr.material = PawnMaterial[1];

            }
            else 
            {
                MeshRenderer mr = GetUnitGO(u).GetComponentInChildren<MeshRenderer>();

                mr.material = PawnMaterial[0];

            }

        }

        

        foreach (Unit u in AI.units)
        {
            if (u == squad(AI, u)[0])
            {
                MeshRenderer mr = GetUnitGO(u).GetComponentInChildren<MeshRenderer>();
                mr.material = PawnMaterial[2];

            }
            else
            {
                MeshRenderer mr = GetUnitGO(u).GetComponentInChildren<MeshRenderer>();

                mr.material = PawnMaterial[3];

            }


        }
    }

    public List<Unit> squad(Team team, Unit u)
    {
        List<Unit> list = new List<Unit>();
        foreach(Unit unit in team.units)
        {
            if(unit.squad == u.squad)
            {
                list.Add(unit);
            }

        }
        return list;
    }

    public List<Hex> spawnArea(int col, int row )
    {

        int checkCol = col + 10;
        int checkRow = row + 2;

        List<Hex> list = new List<Hex>();
        while (col < checkCol)
        {


            while (row < checkRow )
            {
                Hex hexx = GetHexAt(col, row);
                list.Add(hexx);

                row++;
            }
            row = checkRow - 2;
            col++;
        }
        return list;

        

    }

    public void setPlayer(Team team)
    {
        CurrentT = team;
    }

    public void endTurn()
    {
        if(CurrentT.Name == Player.Name)
        {
            switch (currentPhase)
            {
                case Phase.Movement:
                    currentPhase = Phase.Fire;
                    break;
                case Phase.Fire:
                    currentPhase = Phase.Charge;
                    break;
                case Phase.Charge:
                    currentPhase = Phase.Fight;
                    break;
                case Phase.Fight:
                    turnNum++;
                    Player = CurrentT;
                    ResetMoves();
                    setCurrent("AI");
                    currentPhase = Phase.Movement;
                    break;
            }
        }
        else if (CurrentT.Name == AI.Name)
        {
            switch (currentPhase)
            {
                case Phase.Movement:
                    currentPhase = Phase.Fire;
                    break;
                case Phase.Fire:
                    currentPhase = Phase.Charge;
                    break;
                case Phase.Charge:
                    currentPhase = Phase.Fight;
                    break;
                case Phase.Fight:
                    turnNum++;
                    AI = CurrentT;
                    ResetMoves();
                    setCurrent("Player");
                    currentPhase = Phase.Movement;
                    break;
            }

        }
    }

    public void ResetMoves()
    {
        foreach (Unit u in Player.units)
        {
            u.refreshmovment();
        }

        foreach (Unit u in AI.units)
        {
            u.refreshmovment();
        }
    }

}


