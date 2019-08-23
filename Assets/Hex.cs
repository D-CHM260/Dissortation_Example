using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex{

    public readonly int Col;
    public readonly int Row;
    public readonly int Aggregate;
    float radius = 1f;

    public Hex(int c, int r)
    {
        this.Col = c;
        this.Row = r;
        this.Aggregate = -(c + r);
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





}
