using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GenerateMap();
	}

    public GameObject HexPrefab;

    public Material[] HexMaterials;

    int dimensions = 25;

    public void GenerateMap()
    {
        for(int column = 0; column < dimensions; column++)
        {
            for(int row = 0; row < dimensions; row++)
            {
                Hex h = new Hex(column, row);

                GameObject hexGO = (GameObject)Instantiate(
                    HexPrefab, h.position(), 
                    Quaternion.identity, 
                    this.transform);
                MeshRenderer mr =hexGO.GetComponentInChildren<MeshRenderer>();
                mr.material = HexMaterials[Random.Range(0, HexMaterials.Length)];
                
            }
        }
    }
}
