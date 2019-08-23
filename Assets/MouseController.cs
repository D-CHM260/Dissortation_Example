using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Linq;

public class MouseController : MonoBehaviour
{


    public GameObject UnitPanel;
    public GameObject DamageLog;
    public GameObject GameOverScreen;
    public GameObject SelectionQuad;
    public GameObject RangeQuad;
    public GameObject HitMessage;

    //General Variables
    HexGrid hexGrid;
    public Hex hexUnderMouse;
    Hex hexPrevious;
    Vector3 lastMousePos;
    int turn;
    string phase;
    bool MoveHappening = false;
    bool Fireing = false;
    bool Chargeing = false;
    bool Fighting = false;
    bool Fought = false;
    bool GameOver = false;
    public string message = null;
    Unit previous;


    //Cemera variables
    int DragLimit = 4;
    Vector3 lastMouseGroundPlanePosition;
    Vector3 cameraTargetOffset;

    //unit Data
    Hex[] Path;
    LineRenderer linerenderer;
    List<Unit> selectedSquad = null;
    List<Unit> targetSquad = null;
    List<List<Unit>> ChargedSquads = new List<List<Unit>>();
    List<GameObject> selections;
    List<GameObject> RangeBoxes;
    List<Hex> Range = new List<Hex>();
    public List<DamageLogEntry> dmgLog;


    Unit __Selectedunit = null;
    public Unit Selectedunit
    {
        get { return __Selectedunit; }
        set
        {
            __Selectedunit = value;

            if (__Selectedunit != null)
            {
                selectedSquad = hexGrid.CurrentT.squad(__Selectedunit);
                //Debug.Log(selectedSquad[0].name);
                foreach (Unit u in selectedSquad)
                {
                    //Debug.Log(u.name);
                }
            }

            if (hexGrid.CurrentT.Name != "AI" && Selectedunit != null)
            {
                UnitPanel.SetActive(__Selectedunit != null);
                DamageLog.SetActive(__Selectedunit != null);
                Range = hexGrid.Range(Selectedunit.moves, Selectedunit.Hex);

            }

                

            
            


        }
    }

    Unit __TargetUnit = null;
    public Unit TargetUnit
    {
        get { return __TargetUnit; }
        set
        {
            __TargetUnit = value;
            if (hexGrid.CurrentT.Name != "Player" && __TargetUnit != null)
            {
                targetSquad = hexGrid.Player.squad(__TargetUnit);
            }

            else if (hexGrid.CurrentT.Name != "AI" && __TargetUnit != null)
            {
                targetSquad = hexGrid.AI.squad(__TargetUnit);
            }

            if (targetSquad != null)
            {
                foreach (Unit u in targetSquad)
                {
                    //Debug.Log(u.name);
                }

            }



        }
    }




    delegate void UpdateFunction();
    UpdateFunction Update_CurrentFunction;

    public LayerMask HexLayerID;




    // Use this for initialization
    void Start()
    {
        Update_CurrentFunction = Update_ModeDetect;
        hexGrid = GameObject.FindObjectOfType<HexGrid>();
        linerenderer = transform.GetComponentInChildren<LineRenderer>();
        UnitPanel.SetActive(false);
        GameOverScreen.SetActive(false);
        DamageLog.SetActive(false);
        selections = new List<GameObject>();
        RangeBoxes = new List<GameObject>();
        dmgLog = new List<DamageLogEntry>();
        PopUpController.Initialize();
    }


    private void Reset_Mode()
    {

        if (!(Update_CurrentFunction == Update_CameraMovement))
        {
            Selectedunit = null;
            Path = null;
            TargetUnit = null;
            selectedSquad = null;
            targetSquad = null;
            ChargedSquads = new List<List<Unit>>();
        }
        Update_CurrentFunction = Update_ModeDetect;
        checkWounds();

    }

    private void Update()
    {

        if (hexGrid.CurrentT.Name != "AI")
        {
            SelectionBoxes();
        }

        if (hexGrid.currentPhase == HexGrid.Phase.Movement && hexGrid.CurrentT.Name != "AI")
        {
            RangeRender();
            Fought = false;
        }
        
        if (GameOver == false)
        {


            hexUnderMouse = MouseToHex();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Reset_Mode();
                Selectedunit = null;
            }

            Update_CurrentFunction();
            Update_ScrollControl();

            lastMousePos = Input.mousePosition;

            hexPrevious = hexUnderMouse;

            if (Selectedunit != null)
            {
                pathVisual((Path != null) ? Path : Selectedunit.HexQueue());
                //Debug.Log(Selectedunit.name.ToString());
            }

            else if (Selectedunit == null)
            {
                pathVisual(null);
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                hexGrid.endTurn();
            }

            if (hexGrid.currentPhase == HexGrid.Phase.Fight &&
                MoveHappening == false &&
                Fireing == false &&
                Chargeing == false &&
                Fighting == false &&
                hexGrid.animOn == false)
            {

                if (Fought != true)
                {
                    StartCoroutine(FightPhase());
                    Debug.Log("Should only run once");
                }

                



            }

            if (
                hexGrid.CurrentT.Name == "AI" &&
                MoveHappening == false &&
                Fireing == false &&
                Chargeing == false &&
                Fighting == false &&
                hexGrid.animOn == false &&
                hexGrid.currentPhase != HexGrid.Phase.Fight)
            {
                //Debug.Log(hexGrid.animOn);
                switch (hexGrid.currentPhase)
                {
                    case HexGrid.Phase.Movement:
                        StartCoroutine(AIMove());
                        Reset_Mode();
                        break;
                    case HexGrid.Phase.Fire:
                        StartCoroutine(AIFire());
                        Reset_Mode();
                        break;
                    case HexGrid.Phase.Charge:
                        StartCoroutine(AICharge());
                        Reset_Mode();
                        break;
                }
            }
        }
    }

    //Selects control mode to use
    void Update_ModeDetect()
    {

        

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Place holder most games dont do anything on release
            //Debug.Log("Mouse Down");
        }

        else if (Input.GetMouseButtonUp(0) && hexUnderMouse.Units().Length > 0)
        {

            //Debug.Log("Mouse Released");
            Unit[] us = hexUnderMouse.Units();

            foreach (Unit u in hexGrid.CurrentT.units)
            {

                if (u == us[0])
                {

                    if (us != null && us.Length > 0)
                    {
                        Selectedunit = us[0];
                    }

                }
            }
        }

        else if (Selectedunit != null && Input.GetMouseButtonDown(1) && hexGrid.currentPhase == HexGrid.Phase.Movement)
        {
            Update_CurrentFunction = Update_UnitMove;
        }

        else if ((Selectedunit != null && Input.GetMouseButtonDown(1)) && hexGrid.currentPhase == HexGrid.Phase.Fire)
        {
            Unit[] us = hexUnderMouse.Units();
            if (us != null && us.Length > 0 && us[0] != Selectedunit)
            {
                TargetUnit = us[0];
                Update_CurrentFunction = Update_Fire;
                Debug.Log("targeted : " + TargetUnit.name);
            }
            else
            {
                Debug.Log("no unit to target");
            }

        }

        else if ((Selectedunit != null && Input.GetMouseButtonDown(1)) && hexGrid.currentPhase == HexGrid.Phase.Charge)
        {
            Unit[] us = hexUnderMouse.Units();
            if (us != null && us.Length > 0 && us[0] != Selectedunit)
            {
                TargetUnit = us[0];
                Update_CurrentFunction = Update_Charge;
                Debug.Log("targeted : " + TargetUnit.name);
            }
            else
            {
                Debug.Log("no unit to target");
            }

        }

        else if (Input.GetMouseButton(0) && Vector3.Distance(Input.mousePosition, lastMousePos) > DragLimit)
        {
            //Camera Drag
            Update_CurrentFunction = Update_CameraMovement;
            lastMouseGroundPlanePosition = MouseToGround(Input.mousePosition);
            Update_CurrentFunction();

        }

        else if (Selectedunit != null && Input.GetMouseButton(1))
        {



        }

    }


    Hex MouseToHex()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        int layerMask = HexLayerID.value;

        if (Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, layerMask))
        {
            //Debug.Log(hitInfo.collider.name);

            GameObject hexGo = hitInfo.rigidbody.gameObject;

            return hexGrid.GetHexFromGameObject(hexGo);
        }
        //Debug.Log("Nope");
        return null;
    }


    //Mouse Position Return Function
    Vector3 MouseToGround(Vector3 MousePos)
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(MousePos);
        float rayLength = (mouseRay.origin.y / mouseRay.direction.y);
        return mouseRay.origin - (mouseRay.direction * rayLength);
    }


	//Unit Movement mode functions
    void Update_UnitMove()
    {

        if (Input.GetMouseButtonUp(1) && !Input.GetKey(KeyCode.LeftShift) || Selectedunit == null)
        {
            Debug.Log("complete Move");

            if (Selectedunit != null) //&& test.Length <= 0 )
            {
                Selectedunit.setHexPath(Path);

                StartCoroutine(hexGrid.UnitMove(Selectedunit));
            }

            Reset_Mode();
            return;
        }

        if ((Input.GetMouseButtonUp(1) && Input.GetKey(KeyCode.LeftShift)) || Selectedunit == null)
        {
            Debug.Log("complete Move");

            if (Selectedunit != null) //&& test.Length <= 0 )
            {
                SquadMovement(hexUnderMouse);
                StartCoroutine(hexGrid.GroupMovement(selectedSquad));
            }

            Reset_Mode();
            return;
        }

        if (Path == null || hexUnderMouse != hexPrevious)
        {

            Path = QPath.QPath.FindPath<Hex>(hexGrid, Selectedunit, Selectedunit.Hex, hexUnderMouse, Hex.CostEstimate);

        }

    }
	//Unit Fire mode functions
    void Update_Fire()
    {

        if ((Input.GetMouseButtonUp(1) && !Input.GetKey(KeyCode.LeftShift)) || Selectedunit == null || TargetUnit == null)
        {
            Debug.Log("complete Move");

            if (Selectedunit != null && TargetUnit != null && Hex.Distance(Selectedunit.Hex, TargetUnit.Hex) < Selectedunit.weapons[1].range)
            {
                int damage = Selectedunit.Fire(TargetUnit, false, Selectedunit.weapons[1]);
                dmgLog.Add(new DamageLogEntry(TargetUnit.name, Selectedunit.name, damage.ToString(), hexGrid.turnNum.ToString(), hexGrid.currentPhase, false, Selectedunit.weapons[1].type));
                TargetUnit.damageTaken(damage);
                Debug.Log("Unit " + TargetUnit.name + " took " + damage + "damage");
                DeathCheck(TargetUnit);
                /*
                if (TargetUnit.WoundsRem <= 0)
                {
                    
                    Hex DeathSpot = TargetUnit.Hex;
                    GameObject GO = hexGrid.GetUnitGO(TargetUnit);
                    GO.SetActive(false);
                    DeathSpot.RemoveUnit(TargetUnit);
                    
                     * should be in melee
                    if (Selectedunit.movesRem>0)
                    {
                        Hex[] deathRoute = QPath.QPath.FindPath<Hex>(hexGrid, Selectedunit, Selectedunit.Hex, DeathSpot, Hex.CostEstimate);
                        Selectedunit.setHexPath(deathRoute);
                        StartCoroutine(hexGrid.UnitMove(Selectedunit));
                    }
                }*/

            }


            else if (Hex.Distance(Selectedunit.Hex, TargetUnit.Hex) > Selectedunit.weapons[1].range)
            {
                Debug.Log("Too far away from target");
            }

            Reset_Mode();

            return;
        }

        if ((Input.GetMouseButtonUp(1) && Input.GetKey(KeyCode.LeftShift)) || Selectedunit == null || TargetUnit == null)
        {
            Debug.Log("complete Move");

            if (Selectedunit != null && TargetUnit != null && Hex.Distance(Selectedunit.Hex, TargetUnit.Hex) < Selectedunit.weapons[1].range)
            {
                squadFire(selectedSquad, targetSquad, false);

            }


            else if (Hex.Distance(Selectedunit.Hex, TargetUnit.Hex) > Selectedunit.weapons[1].range)
            {
                Debug.Log("Too far away from target");
            }

            Reset_Mode();

            return;
        }

    }
	//Unit Charge mode functions
    void Update_Charge()
    {

        if (Input.GetMouseButtonUp(1) || Selectedunit == null || TargetUnit == null)
        {
            //Debug.Log("Trigger Charge");

            if (selectedSquad != null && targetSquad != null)
            {
                int rnd = UnityEngine.Random.Range(0, 12);
                rnd = 100;
                Hex[] LongestPath = null;
                Hex[] route = null;

                foreach (Unit selected in selectedSquad)
                {
                    foreach (Unit targeted in targetSquad)
                    {
                        route = QPath.QPath.FindPath<Hex>(hexGrid, selected, selected.Hex, targeted.Hex, Hex.CostEstimate);

                        if (LongestPath == null)
                        {
                            LongestPath = route;
                        }

                        else if (LongestPath.Length < route.Length)
                        {
                            LongestPath = route;
                        }
                    }
                }



                if (LongestPath.Length - 1 <= rnd)
                {


                    SquadCharge(rnd);
                    StartCoroutine(hexGrid.GroupMovement(selectedSquad));

                    /*
                    List<Unit> targetUsed = new List<Unit>();
                    Unit currentUnitTested = null;
                    foreach (Unit selected in selectedSquad)
                    {
                        LongestPath = null;
                        currentUnitTested = null;
                        selected.movesRem = rnd;
                        foreach (Unit targeted in targetSquad)
                        {
                            bool UsedAlready = false;
                            
                            if (targetUsed != null)
                            {
                                foreach (Unit test in targetUsed)
                                {
                                    if (targeted == test)
                                    {
                                        UsedAlready = true;
                                    }

                                }
                            }
                            if (!UsedAlready)
                            {
                                //Debug.Log("!usedAlaredy passed for " + targeted.name);

                                route = QPath.QPath.FindPath<Hex>(hexGrid, selected, selected.Hex, targeted.Hex, Hex.CostEstimate);
                                if (LongestPath == null)
                                {
                                    LongestPath = route;
                                    currentUnitTested = targeted;

                                }
                                else if (LongestPath.Length < route.Length)
                                {
                                    LongestPath = route;
                                    currentUnitTested = targeted;

                                }

                            }



                        }
                        targetUsed.Add(currentUnitTested);
                        //Debug.Log(selected.name + LongestPath.Length);
                        Debug.Log(currentUnitTested.name + "  " + LongestPath.Length );
                        selected.setHexPath(LongestPath);
                        StartCoroutine(hexGrid.UnitMove(selected));

                    }

                */
                }


            }
            ChargedSquads.Add(selectedSquad);

            Reset_Mode();

            return;
        }

    }
	//Function to move an entire squad
    public void SquadMovement(Hex InitialHex)
    {
        Hex[] NewPath = null;
        Hex used = null;
        List<Hex> targetUsed = new List<Hex>();
        List<Hex> Neighbours = InitialHex.GetNeighboursList();
        if (Neighbours.Count <= 0)
        {
            Debug.Log("For Some Reason it fails to initilize neighbours");
        }
        foreach (Unit selected in selectedSquad)
        {

            if (selected.name == selectedSquad[0].name)
            {
                NewPath = QPath.QPath.FindPath<Hex>(hexGrid, selected, selected.Hex, InitialHex, Hex.CostEstimate);

                if (NewPath.Length > selected.movesRem)
                {
                    Neighbours = NewPath[selected.movesRem].GetNeighboursList();
                }
                else
                {
                    Neighbours = NewPath[NewPath.Length - 1].GetNeighboursList();
                }

            }

            else
            {
                foreach (Hex h in Neighbours)
                {

                    bool notusedbefore = true;
                    if (targetUsed.Count > 0)
                    {
                        foreach (Hex h2 in targetUsed)
                        {
                            if (h2 == h)
                            {
                                notusedbefore = false;
                            }
                        }
                    }



                    Hex[] TestPath = QPath.QPath.FindPath<Hex>(hexGrid, selected, selected.Hex, h, Hex.CostEstimate);

                    if (NewPath == null && notusedbefore)
                    {
                        NewPath = TestPath;
                        used = h;
                    }

                    if (NewPath != null)
                    {
                        if (TestPath.Length < NewPath.Length && notusedbefore)
                        {
                            NewPath = TestPath;
                            used = h;
                        }
                    }

                }

                targetUsed.Add(used);
                used = null;
                selected.movesRem = NewPath.Length;
            }


            //Debug.Log(selected.name + LongestPath.Length);
            //Debug.Log(currentUnitTested.name + "  " + NewPath.Length);
            if (selected.WoundsRem > 0 && NewPath.Length>1)
            {
                selected.setHexPath(NewPath);
            }

            NewPath = null;
            //StartCoroutine(hexGrid.UnitMove(selected));

        }
    }
	//Function to draw the path
    void pathVisual(Hex[] a)
    {
        if (a == null || a.Length == 0)
        {
            linerenderer.enabled = false;
            return;
        }

        linerenderer.enabled = true;

        Vector3[] ps = new Vector3[a.Length];

        for (int i = 0; i < a.Length; i++)
        {
            GameObject hexGO = hexGrid.GetHexGO(a[i]);
            ps[i] = hexGO.transform.position + (Vector3.up * 0.01f);
        }

        linerenderer.positionCount = ps.Length;
        linerenderer.SetPositions(ps);
    }



    // Function to Drag the Camera Around
    void Update_CameraMovement()
    {

        if (Input.GetMouseButtonUp(0))
        {

            Reset_Mode();
            return;
        }


        Vector3 hitPos = MouseToGround(Input.mousePosition);

        Vector3 diff = lastMouseGroundPlanePosition - hitPos;
        Camera.main.transform.Translate(diff, Space.World);

        lastMouseGroundPlanePosition = hitPos = MouseToGround(Input.mousePosition);


    }

    //Function to control the zooming of the map
    void Update_ScrollControl()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float mh = 5;
        float mxh = 20;

        Vector3 hitPos = MouseToGround(Input.mousePosition);

        Vector3 dir = hitPos - Camera.main.transform.position;

        Vector3 p = Camera.main.transform.position;

        if (scroll > 0 || p.y < mxh)
        {
            cameraTargetOffset += dir * scroll;
        }

        Vector3 lastCameraPosition = Camera.main.transform.position;
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, Camera.main.transform.position + cameraTargetOffset, Time.deltaTime * 5f);
        cameraTargetOffset -= Camera.main.transform.position - lastCameraPosition;

        p = Camera.main.transform.position;

        if (p.y < mh)
        {
            p.y = mh;
        }

        if (p.y > mxh)
        {
            p.y = mxh;
        }
        Camera.main.transform.position = p;

        Camera.main.transform.rotation = Quaternion.Euler(
        Mathf.Lerp(30, 60, p.y / (mxh / 1.5f)),
        Camera.main.transform.rotation.eulerAngles.y,
        Camera.main.transform.rotation.eulerAngles.z
        );
    }
	
	//Function make a squad charge an enemy
    public void SquadCharge(int rnd)
    {
        Hex[] LongestPath = null;
        Hex[] route = null;
        List<Unit> targetUsed = new List<Unit>();
        Unit currentUnitTested = null;
        foreach (Unit selected in selectedSquad)
        {
            LongestPath = null;
            //currentUnitTested = null;
            selected.movesRem = rnd;
            foreach (Unit targeted in targetSquad)
            {
                bool UsedAlready = false;

                if (targetUsed != null)
                {
                    foreach (Unit test in targetUsed)
                    {
                        if (targeted == test)
                        {
                            UsedAlready = true;
                            //Debug.Log(targeted + " Used Already");
                        }

                    }
                }
                if (!UsedAlready)
                {
                    //Debug.Log("!usedAlaredy passed for " + targeted.name);

                    route = QPath.QPath.FindPath<Hex>(hexGrid, selected, selected.Hex, targeted.Hex, Hex.CostEstimate);
                    if (LongestPath == null)
                    {
                        LongestPath = route;
                        currentUnitTested = targeted;

                    }
                    else if (LongestPath.Length < route.Length)
                    {
                        LongestPath = route;
                        currentUnitTested = targeted;

                    }

                }



            }



            if(LongestPath == null && targetUsed.Count>0)
            {
                Debug.Log(currentUnitTested.name);
                LongestPath = QPath.QPath.FindPath<Hex>(hexGrid, selected, selected.Hex, targetUsed[targetUsed.Count - 1].Hex, Hex.CostEstimate);
            }

            if(LongestPath != null)
            {
                if (selected.WoundsRem > 0 && LongestPath.Length > 1)
                {

                    targetUsed.Add(currentUnitTested);

                    //Debug.Log(selected.name + LongestPath.Length);
                    //Debug.Log(currentUnitTested.name + "  " + LongestPath.Length);
                    selected.setHexPath(LongestPath);
                    //StartCoroutine(hexGrid.UnitMove(selected));
                }
            }
            

        }




    }

	//Function to make a squad fire on an enemy
    public void squadFire(List<Unit> S, List<Unit> T, bool overwatch)
    {
        Unit Targeted = null;
        List<Unit> AlreadyAttacked = new List<Unit>();
        //Debug.Log("TestPoint 0.5 ");
        foreach (Unit s in S)
        {
            //Debug.Log("TestPoint 1 ");
            foreach (Unit t in T)
            {
                //Debug.Log("TestPoint 2 ");
                float i = Hex.Distance(s.Hex, t.Hex);
                if (i < s.weapons[1].range)
                {
                    //Debug.Log("TestPoint 3 ");
                    if (unitPresent(AlreadyAttacked, t))
                    {
                        if (Targeted == null)
                        {
                            Targeted = t;
                            //Debug.Log("Unit added on first pass " + t.name);
                        }

                        else if ((Hex.Distance(s.Hex, t.Hex) < Hex.Distance(s.Hex, Targeted.Hex)))
                        {
                            Targeted = t;
                            //Debug.Log("Unit added consequitive passes " + t.name);
                        }

                        else
                        {
                            //Debug.Log("Tested but Further Away "+ t.name);
                        }

                    }

                }
            }

            if (Targeted != null && !s.ranged && s.WoundsRem > 0)
            {
                int damage = s.Fire(Targeted, overwatch, s.weapons[1]);
                dmgLog.Add(new DamageLogEntry(Targeted.name, s.name, damage.ToString(), hexGrid.turnNum.ToString(), hexGrid.currentPhase, false, s.weapons[1].type));
                Targeted.damageTaken(damage);
                DeathCheck(Targeted);
                //Debug.Log("Unit " + Targeted.name + " took " + damage + "damage from FIRE");               
                AlreadyAttacked.Add(Targeted);
                s.ranged = true;
            }

            else if (s.ranged)
            {
                //Debug.Log("Unit Already Fired");
            }

            else if (s.WoundsRem <= 0)
            {
                //Debug.Log("Dead Unit");
            }

            //Debug.Log("targeted Unit " + Targeted.name);
            Targeted = null;

            foreach (Unit u in AlreadyAttacked)
            {
                //Debug.Log("already attacked List " + u.name);
            }
        }
    }

	//Checks units presence in a squad
    public bool unitPresent(List<Unit> u, Unit U)
    {
        bool check = true;
        foreach (Unit u2 in u)
        {
            if (u2.name == U.name)
            {
                check = false;
                //Debug.Log(U.name + " Already attacked");
            }
        }

        return check;
    }

	//Checks units health an removes if unit is below or equal to 0
    public void DeathCheck(Unit u)
    {
        if (u.WoundsRem <= 0)
        {
            Debug.Log("check Fireing");
            Hex DeathSpot = u.Hex;
            GameObject GO = hexGrid.GetUnitGO(u);
            //Debug.Log(u.name + " Is dead less than 0 wounds remaining");
            DeathSpot.RemoveUnit(u);
            GO.SetActive(false);
            //DeathSpot.RemoveUnit(u);
        }
    }

	//Runs through the fight phase.
    public IEnumerator FightPhase()
    {
        Fought = true;
        Debug.Log("Fighting has started");
        
        Team team = null;
        if (hexGrid.CurrentT.Name == "Player")
        {

            team = hexGrid.AI;

        }

        if (hexGrid.CurrentT.Name == "AI")
        {
            team = hexGrid.Player;
        }

        List<Team> teams = new List<Team>();
        teams.Add(hexGrid.CurrentT);
        teams.Add(team);

        foreach(Team t in teams)
        {
            yield return squadAttack(t.units.ToList());
            yield return new WaitForSeconds(2f);
        }
        
        
        
        
        /*foreach (Unit u in team.units)
        {
            List<Hex> neighbours = u.Hex.GetNeighboursList();
            List<Unit> Target = new List<Unit>();
            Unit selectedTarget = null;
            if (!u.melee && u.WoundsRem>0)
            {
                foreach (Hex h in neighbours)
                {
                    Unit[] units = h.Units();
                    foreach (Unit PossibleTarget in units)
                    {
                        if (!team.Teamfinder(PossibleTarget))
                        {
                            Target.Add(PossibleTarget);
                            //Debug.Log(PossibleTarget.name);
                        }

                    }
                }

                foreach (Unit LowestHP in Target)
                {

                    if (selectedTarget == null)
                    {
                        selectedTarget = LowestHP;
                    }

                    if (selectedTarget.WoundsRem > LowestHP.WoundsRem)
                    {
                        selectedTarget = LowestHP;
                    }


                }

                //Debug.Log(u.name);
                if (selectedTarget != null)
                {
                    int damage = u.Fire(selectedTarget, false, u.weapons[0]);
                    dmgLog.Add(new DamageLogEntry(selectedTarget.name, u.name, damage.ToString(), hexGrid.turnNum.ToString(), hexGrid.currentPhase, false, u.weapons[0].type));
                    selectedTarget.damageTaken(damage);
                    Debug.Log(selectedTarget.name + " Is Targeted By " + u.name + " for " + damage + "damage");
                    DeathCheck(selectedTarget);
                }





            }

            yield return new WaitForSeconds(0.5f);
        }*/
        

        Debug.Log("Fighting has Finished");
    }

	//Computers Movement selection and execution
    public IEnumerator AIMove()
    {
        MoveHappening = true;
        foreach (List<Unit> Squads in hexGrid.CurrentT.squads)
        {
            Debug.Log(Squads[0].name);
            Selectedunit = hexGrid.CurrentT.squadLead(Squads[0]);
            //Debug.Log(Selectedunit.name);

            SelectTarget();
            if (Selectedunit != null) //&& test.Length <= 0 )
            {
                //Debug.Log(TargetUnit.name);
                SquadMovement(hexGrid.Player.squadLead(TargetUnit).Hex);
                StartCoroutine(hexGrid.GroupMovement(selectedSquad));
            }

            //Debug.Log(Selectedunit.squad + " is selected and attacks " + TargetUnit.squad);

            yield return new WaitForSeconds(2f);

        }
        MoveHappening = false;
        hexGrid.endTurn();

    }

	//Computers fire selection and execution
    public IEnumerator AIFire()
    {
        int i = 0;
        Debug.Log("Squad Fireing Starts Here");
        Fireing = true;
        foreach (List<Unit> Squads in hexGrid.CurrentT.squads)
        {
            Selectedunit = Squads[i];
            SelectTarget();
            if (Selectedunit != null && selectedSquad.Count>0 && targetSquad.Count>0) //&& test.Length <= 0 )
            {
                squadFire(selectedSquad, targetSquad, false);
            }

            //Debug.Log(hexGrid.CurrentT.Name + Selectedunit.squad + " is selected and attacks " + hexGrid.Player.Name + TargetUnit.squad);

            yield return new WaitForSeconds(2f);
            i++;

        }
        Fireing = false;
        hexGrid.endTurn();       
        Debug.Log("Squad Fireing Finished Here");
    }

	//Computer charge selection and execution
    public IEnumerator AICharge()
    {
        Debug.Log("Squad Chargeing Starts Here");
        Chargeing = true;
        foreach (List<Unit> Squads in hexGrid.CurrentT.squads)
        {
            Selectedunit = hexGrid.CurrentT.squadLead(Squads[0]);

            SelectTarget();
            int roll = UnityEngine.Random.Range(2, 12);
            if (Selectedunit != null && selectedSquad.Count > 0 && targetSquad.Count > 0) //&& test.Length <= 0 )
            {
                Debug.Log(TargetUnit.name);
                Debug.Log("During OverWatch " + TargetUnit.name + " is selected and attacks " + Selectedunit.name);
                squadFire(targetSquad,selectedSquad, true);
                Debug.Log(roll + "Was Rolled ");
                if(roll>=Hex.Distance(Selectedunit.Hex, TargetUnit.Hex))
                {
                    SquadCharge(roll);
                    StartCoroutine(hexGrid.GroupMovement(selectedSquad));
                }

                else
                {
                    Debug.Log("For a distance of " + Hex.Distance(Selectedunit.Hex, TargetUnit.Hex));
                }
                
            }

            Debug.Log(Selectedunit.squad + " is selected and attacks " + TargetUnit.squad);
            yield return new WaitForSeconds(2f);

        }
        Debug.Log("Squad Chargeing Finishes Here");
        Chargeing = false;      
        hexGrid.endTurn();

    }

	//Target selection function for the above computer functions
    public void SelectTarget()
    {
        Unit Target = null;
        float distance = 0;

        foreach (List<Unit> TargetedSquads in hexGrid.Player.squads)
        {
            Unit TargetLead = hexGrid.Player.squadLead(TargetedSquads[0]);

            if (TargetLead != null)
            {

                float distance2 = Hex.Distance(Selectedunit.Hex, TargetLead.Hex);

                if (Target == null)
                {
                    Target = TargetLead;
                    distance = distance2;
                }

                if (distance2 < distance)
                {
                    distance = distance2;
                    Target = TargetLead;
                }

            }

            
        }
        if (Target != null)
        {
            TargetUnit = Target;
            //Debug.Log(TargetUnit.squad);
        }

        else
        {
            Debug.Log("nothing Left to attack");
        }

    }

	//Checks for wounds on a unit during combat
    public void checkWounds()
    {
        List<Unit> TestingMaterial = new List<Unit>();
        foreach (Unit u in hexGrid.CurrentT.units)
        {
            if (u.WoundsRem > 0)
            {
                TestingMaterial.Add(u);
            }
        }


        if (TestingMaterial.Count <= 0)
        {
            GameOverScreen.SetActive(true);
            message = hexGrid.CurrentT.Name + "Won";
            GameOver = true;
        }


        TestingMaterial = new List<Unit>();

        foreach (Unit u in hexGrid.Player.units)
        {
            if (u.WoundsRem > 0)
            {
                TestingMaterial.Add(u);
            }
        }

        if (TestingMaterial.Count <= 0)
        {
            GameOverScreen.SetActive(true);
            message = "AI Won";
            GameOver = true;
        }

        TestingMaterial = new List<Unit>();

        foreach (Unit u in hexGrid.AI.units)
        {
            if (u.WoundsRem > 0)
            {
                TestingMaterial.Add(u);
            }
        }

        if (TestingMaterial.Count <= 0)
        {
            GameOverScreen.SetActive(true);
            message = "Player Won";
            GameOver = true;
        }
    }

	//Draws selection buttons under selected squad
    public void SelectionBoxes()
    {
        
        if (Selectedunit != null && selections.Count<=0)
        {
            foreach (Unit u in selectedSquad)
            {
                Vector3 it = hexGrid.GetUnitGO(u).transform.position;


                GameObject selection = (GameObject)Instantiate(
                    SelectionQuad, it,
                    Quaternion.identity,
                    this.transform);

               // if(u == hexGrid.CurrentT.squadLead(Selectedunit))
               // {
                    //MeshRenderer mr = selection.GetComponentInChildren<MeshRenderer>();
                   // mr.material = hexGrid.SelectionMaterials[1];
                //}

                selections.Add(selection);
            }

            previous = Selectedunit;
        }

        if((Selectedunit == null && selections.Count>0) || Selectedunit != previous)
        {
            
            foreach(GameObject go in selections.ToList())
            {
                selections.Remove(go);
                Destroy(go);
            } 
        }

    }

	//Draws movement range for the selected unit
    public void RangeRender()
    {

        if (Selectedunit != null && RangeBoxes.Count <= 0)
        {
            foreach (Hex hex in Range)
            {
                Vector3 it = hexGrid.GetHexGO(hex).transform.position;


                GameObject RangeBox = (GameObject)Instantiate(
                    RangeQuad, it,
                    Quaternion.identity,
                    this.transform);

                // if(u == hexGrid.CurrentT.squadLead(Selectedunit))
                // {
                //MeshRenderer mr = selection.GetComponentInChildren<MeshRenderer>();
                // mr.material = hexGrid.SelectionMaterials[1];
                //}

                RangeBoxes.Add(RangeBox);
            }

            previous = Selectedunit;
        }

        if ((Selectedunit == null && RangeBoxes.Count > 0) || Selectedunit != previous)
        {

            foreach (GameObject go in RangeBoxes.ToList())
            {
                RangeBoxes.Remove(go);
                Destroy(go);
            }
        }

    }

	//Runs through the operations in a squad attack function
    public IEnumerator squadAttack(List<Unit> list)
    {

        foreach (Unit u in list)
        {
            List<Hex> neighbours = u.Hex.GetNeighboursList();
            List<Unit> Targets = new List<Unit>();
            Unit selectedTarget = null;


            if (!u.melee)
            {
                foreach (Hex h in neighbours)
                {
                    Unit[] units = h.Units();
                    foreach (Unit PossibleTarget in units)
                    {
                        if (hexGrid.AI.Teamfinder(u))
                        {
                            if (!hexGrid.AI.Teamfinder(PossibleTarget))
                            {
                                Targets.Add(PossibleTarget);
                            }
                        }

                        if (hexGrid.Player.Teamfinder(u))
                        {
                            if (!hexGrid.Player.Teamfinder(PossibleTarget))
                            {
                                Targets.Add(PossibleTarget);
                            }
                        }
                        



                    }
                }

                foreach (Unit LowestHP in Targets)
                {

                    if (selectedTarget == null)
                    {
                        selectedTarget = LowestHP;
                    }

                    if (selectedTarget.WoundsRem > LowestHP.WoundsRem)
                    {
                        selectedTarget = LowestHP;
                    }

                }

                if (selectedTarget != null)
                {
                    int damage = u.Fire(selectedTarget, false, u.weapons[0]);
                    dmgLog.Add(new DamageLogEntry(selectedTarget.name, u.name, damage.ToString(), hexGrid.turnNum.ToString(), hexGrid.currentPhase, false, u.weapons[0].type));
                    Debug.Log(selectedTarget.name + u.name + damage.ToString() + hexGrid.turnNum.ToString() + hexGrid.currentPhase + false + u.weapons[0].type);
                    selectedTarget.damageTaken(damage);
                    DeathCheck(selectedTarget);

                }


            }
            yield return new WaitForSeconds(2f);
        }


    }

}








