using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScript : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        grid = GameObject.FindObjectOfType<MouseController>();
    }

    public Text Message;

    MouseController grid;

    // Update is called once per frame
    void Update()
    {
        Message.text = grid.message;
    }

    

}
