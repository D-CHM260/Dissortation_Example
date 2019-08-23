using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QPath;
using System.IO;

public class HexGrid : MonoBehaviour, IQPathWorld {

	// Use this for initialization
	void Start () {
        GenerateMap();
        Player = new Team("Player");
        AI = new Team("AI");
        BuildArmies();
        spawnArmy();
        //spawnareaAI = spawnArea(12, 30);
        //spawnareaPlayer = spawnArea(23, 4);
    }

    public bool animOn = false;
    public GameObject HexPrefab;
    private Hex[,] hexes;
    private Dictionary<Hex, GameObject> hexToGameObjectMap;
    private Dictionary<GameObject, Hex> gameObjectToHexMap;
    private Dictionary<GameObject, Unit> gameObjectToUnit;
    private Dictionary<Unit, GameObject> UnitToGameObject;
    private HashSet<Unit> CurrentUnits;
    public Material[] HexMaterials;
    public GameObject TestUnit;
    public Team Player;
    public Team AI;
    public Team CurrentT;
    public List<Hex> spawnareaPlayer;
    public List<Hex> spawnareaAI;



    int dimensionsY = 40;
    int dimensionsZ = 80;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(MovementCheck());
        }
        
        if(CurrentT != null)
        {
            CurrentUnits = CurrentT.GetUnitsInTeam();
        }
    }

    public IEnumerator MovementCheck()
    {
        if (CurrentUnits != null)
        {
            foreach (Unit u in CurrentUnits)
            {
                yield return UnitMove(u);
            }
        }

    }

    public IEnumerator UnitMove(Unit u)
    {
        while (u.DoMove())
        {
            Debug.Log("true return");
            while (animOn)
            {
                yield return null;
            }
        }
    }

    public void FinishPhase()
    {

        foreach (Unit u in CurrentUnits)
        {
            u.refreshmovment();
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
            }
        }
    }

    public void creatunit(Unit unit, GameObject prefab, int x, int y)
    {

        if(CurrentUnits == null)
        {
            CurrentUnits = new HashSet<Unit>();
            UnitToGameObject = new Dictionary<Unit, GameObject>();
            gameObjectToUnit = new Dictionary<GameObject, Unit>();
        }
        
        Hex myHex = GetHexAt(x, y);
        GameObject myHexGo = hexToGameObjectMap[myHex];
        unit.SetHex(myHex);


        GameObject unitGo = (GameObject)Instantiate(prefab, myHexGo.transform.position,
                    Quaternion.identity, myHexGo.transform);
        unit.onUnitMoved += unitGo.GetComponent<UnitView>().onUnitMoved;

        unit.onUnitMoved(myHex, myHex);

        //units.Add(unit);
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
                Unit newunit = new Unit(ID, UnitType[i]);

                switch (Team[i])
                {
                    case "Player":
                        Squad a = new Squad(Squad[i]);
                        bool presence = Player.CheckPresence(Squad[i]);
                        if (presence == false)
                        {
                            a.addUnit(newunit);
                            Player.addSquad(a);
                            Debug.Log(a.identifier + a.units[a.units.Count-1].name);

                        }

                        else if (presence == true)
                        {
                            int index = Player.FindIndexOfSquad(Squad[i]); //Player.FindIndexOfSquad(Squad[i]);
                            Player.squads[index].addUnit(newunit);
                            int count = Player.squads[index].units.Count - 1;
                            Debug.Log(Player.squads[index].identifier + Player.squads[index].units[count].name);
                        }




                        ; break;
                    case "AI":
                        Squad b = new Squad(Squad[i]);
                        presence = AI.CheckPresence(Squad[i]);
                        if (presence == false)
                        {
                            b.addUnit(newunit);
                            AI.addSquad(b);
                            Debug.Log(b.identifier + b.units[b.units.Count - 1].name);

                        }

                        else if (presence == true)
                        {
                            int index = AI.FindIndexOfSquad(Squad[i]); //Player.FindIndexOfSquad(Squad[i]);
                            AI.squads[index].addUnit(newunit);
                            int count = AI.squads[index].units.Count - 1;
                            Debug.Log(AI.squads[index].identifier + AI.squads[index].units[count].name);
                        }

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
        foreach (Unit u in Player.GetUnitsInTeam())
        {
            creatunit(u, TestUnit, o + 20, i + 3);
            i++;


        }

        o = 1;
        foreach (Unit u in AI.GetUnitsInTeam())
        {
            creatunit(u, TestUnit, o + 20, i + 3);
            i--;
 
        }

    }

    public List<Hex> spawnArea(int StartPointX, int StartPointY )
    {

        int checkX = StartPointX;
        int checkY = StartPointY;

        List<Hex> list = new List<Hex>();
        while (StartPointX < checkX + 3)
        {


            while (StartPointY < checkY + 20)
            {
                Hex hexx = GetHexAt(StartPointX, StartPointY);
                list.Add(hexx);

                StartPointX++;
            }

            StartPointY++;
        }
        return list;

        

    }

}


