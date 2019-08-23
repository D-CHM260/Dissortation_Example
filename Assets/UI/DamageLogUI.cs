using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageLogUI : MonoBehaviour
{

    MouseController mouseController;
    void Start()
    {
        mouseController = GameObject.FindObjectOfType<MouseController>();
    }

    public Text DamageLog;
    public Text unit;

    // Update is called once per frame
    void Update () {

        if (mouseController.Selectedunit != null)
        {
            //Debug.Log("Before Everything");
            if (mouseController.dmgLog.Count>0)
            {
                unit.text = null;
                DamageLog.text = null;
                //Debug.Log("This should not fire");
                foreach (DamageLogEntry dl in mouseController.dmgLog)
                {
                    if (dl.Defender == mouseController.Selectedunit.name)
                    {
                        string oldtext = DamageLog.text.ToString();
                        string newtext = oldtext + dl.AttackedBy + " attacked for " + dl.ForXDamage + " during phase " + dl.OnPhase + "\n";
                        DamageLog.text = newtext.ToString();
                        unit.text = mouseController.Selectedunit.name;
                    }
                }
            }

            else
            {
                //Debug.Log("this should fire at the start");
                unit.text = mouseController.Selectedunit.name;
                DamageLog.text = "No unit has attacked this unit yet";
            }
            
        }
        
        else if (mouseController.Selectedunit == null && DamageLog.text != null)
        {
            //Debug.Log("Log Reset");
            unit.text = null;
            DamageLog.text = null;
        }

        //Debug.Log("Testing Update");
	}
}
