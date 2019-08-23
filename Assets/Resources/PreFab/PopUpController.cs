using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpController : MonoBehaviour {

    private static FloatingText popuptext;
    private static GameObject canvas;
    private static GameObject PreFab;

    public static void Initialize()
    {
        canvas = GameObject.Find("Canvas");
        if (!popuptext)
        {
            popuptext = Resources.Load<FloatingText>("PreFab/PopUpParent");
        }
        
    }

	public static void CreateTXT (string Damage, Transform location)
    {

        FloatingText instance = Instantiate(popuptext);
        Vector2 screenpos = Camera.main.WorldToScreenPoint(location.position);
        instance.transform.SetParent(canvas.transform, false);
        instance.transform.position = screenpos;
        instance.SetText(Damage);

    }
}
