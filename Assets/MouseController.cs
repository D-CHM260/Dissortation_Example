using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;


public class MouseController : MonoBehaviour {


    public GameObject UnitPanel;

    //General Variables
    HexGrid hexGrid;
    Hex hexUnderMouse;
    Hex hexPrevious;
    Vector3 lastMousePos;
    int turn;
    string phase;


    //Cemera variables
    int DragLimit = 4;
    Vector3 lastMouseGroundPlanePosition;
    Vector3 cameraTargetOffset;

    //unit Data
    Hex[] Path;
    LineRenderer linerenderer;
    List<Unit> selectedSquad = null;
    List<Unit> targetSquad = null;


    Unit __Selectedunit = null;
     public Unit Selectedunit
    {
        get { return __Selectedunit; }
        set
        {
            __Selectedunit = value;

            if(__Selectedunit != null)
            {
                selectedSquad = hexGrid.CurrentT.squad(__Selectedunit);
                foreach (Unit u in selectedSquad)
                {
                    //Debug.Log(u.name);
                }
            }
            
            UnitPanel.SetActive(__Selectedunit != null);

            
        }
    }

    Unit __TargetUnit = null;
    public Unit TargetUnit
    {
        get { return __TargetUnit; }
        set
        {
            __TargetUnit = value;
            if(hexGrid.CurrentT.Name != "Player" && __TargetUnit != null)
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
    void Start ()
    {
        Update_CurrentFunction = Update_ModeDetect;
        hexGrid = GameObject.FindObjectOfType<HexGrid>();
        linerenderer = transform.GetComponentInChildren<LineRenderer>();
        UnitPanel.SetActive(false);
    }

   
    private void Reset_Mode()
    {

        Update_CurrentFunction = Update_ModeDetect;
        Selectedunit = null;
        Path = null;
        TargetUnit = null;
        selectedSquad = null;
        targetSquad = null;

    }

    private void Update()
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

        if(Selectedunit != null)
        {
            pathVisual((Path!=null) ? Path : Selectedunit.HexQueue());
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

    }

    //Selects control mode to use
    void Update_ModeDetect()
    {

        if ( EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Place holder most games dont do anything on release
            Debug.Log("Mouse Down");
        }

        else if (Input.GetMouseButtonUp(0) && hexUnderMouse.Units().Length > 0)
        {

            Debug.Log("Mouse Released");
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

        else if(Selectedunit!=null && Input.GetMouseButtonDown(1) && hexGrid.currentPhase == HexGrid.Phase.Movement)
        {
            Update_CurrentFunction = Update_UnitMove;
        }

        else if ((Selectedunit != null && Input.GetMouseButtonDown(1))&& hexGrid.currentPhase == HexGrid.Phase.Fire)
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

        else if(Input.GetMouseButton(0) && Vector3.Distance(Input.mousePosition,lastMousePos) > DragLimit)
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

        if(Physics.Raycast(mouseRay,out hitInfo, Mathf.Infinity, layerMask))
        {
            //Debug.Log(hitInfo.collider.name);

            GameObject hexGo = hitInfo.rigidbody.gameObject;

            return hexGrid.GetHexFromGameObject(hexGo);
        }
        //Debug.Log("Nope");
        return null;
    }


    //Mouse Position Return Function
    Vector3 MouseToGround (Vector3 MousePos)
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(MousePos);
        float rayLength = (mouseRay.origin.y / mouseRay.direction.y);
        return mouseRay.origin - (mouseRay.direction * rayLength);
    }



    void Update_UnitMove()
    {

        if (Input.GetMouseButtonUp(1) || Selectedunit == null)
        {
            Debug.Log("complete Move");

            Unit[] test = hexUnderMouse.Units();

            if (Selectedunit != null && test.Length <= 0 )
            {
                Selectedunit.setHexPath(Path);

                StartCoroutine(hexGrid.UnitMove(Selectedunit));
            }

            Reset_Mode();
            return;
        }

        if(Path == null || hexUnderMouse != hexPrevious)
        {

            Path = QPath.QPath.FindPath<Hex>(hexGrid, Selectedunit, Selectedunit.Hex, hexUnderMouse, Hex.CostEstimate);

        }

    }

    void Update_Fire()
    {

        if (Input.GetMouseButtonUp(1) || Selectedunit == null || TargetUnit == null)
        {
            Debug.Log("complete Move");

            if (Selectedunit != null && TargetUnit != null && Hex.Distance(Selectedunit.Hex, TargetUnit.Hex) < Selectedunit.weapons[1].range)
            {
                int damage = Selectedunit.Fire(TargetUnit, false);
                TargetUnit.damageTaken(damage);
                Debug.Log("Unit " + TargetUnit.name + " took " + damage + "damage");
                if (TargetUnit.WoundsRem <= 0)
                {
                    Hex DeathSpot = TargetUnit.Hex;
                    GameObject GO = hexGrid.GetUnitGO(TargetUnit);
                    GO.SetActive(false);
                    DeathSpot.RemoveUnit(TargetUnit);
                    /*
                     * should be in melee
                    if (Selectedunit.movesRem>0)
                    {
                        Hex[] deathRoute = QPath.QPath.FindPath<Hex>(hexGrid, Selectedunit, Selectedunit.Hex, DeathSpot, Hex.CostEstimate);
                        Selectedunit.setHexPath(deathRoute);
                        StartCoroutine(hexGrid.UnitMove(Selectedunit));
                    }*/
                }

            }
            else if(Hex.Distance(Selectedunit.Hex, TargetUnit.Hex) > Selectedunit.weapons[1].range)
            {
                Debug.Log("Too far away from target");
            }

            Reset_Mode();
            
            return;
        }

    }

    void Update_Charge()
    {

        if (Input.GetMouseButtonUp(1) || Selectedunit == null || TargetUnit == null)
        {
            Debug.Log("Trigger Charge");

            if (selectedSquad != null && targetSquad != null)
            {
                int rnd = UnityEngine.Random.Range(0, 12);
                rnd = 100;
                Hex[] LongestPath = null;
                Hex[] route = null;

                foreach (Unit selected in selectedSquad)
                {
                    foreach(Unit targeted in targetSquad)
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

                

                if(LongestPath.Length-1 <= rnd)
                {
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

                }


            }          

            Reset_Mode();

            return;
        }

    }

    void pathVisual(Hex[] a)
    {
        if(a == null || a.Length == 0)
        {
            linerenderer.enabled = false;
            return;
        }

        linerenderer.enabled = true;

        Vector3[] ps = new Vector3[a.Length];

        for (int i = 0; i < a.Length; i++)
        {
            GameObject hexGO = hexGrid.GetHexGO(a[i]);
            ps[i] = hexGO.transform.position + (Vector3.up*0.01f);
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



}


