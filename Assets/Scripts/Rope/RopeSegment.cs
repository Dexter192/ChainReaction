using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeSegment
{
    public Vector2 posNow { get; set; }
    public Vector2 posOld { get; set; }
    public RopeSegment(Vector3 pos)
    {
            this.posNow = pos;
            this.posOld = pos;
    }    
}
