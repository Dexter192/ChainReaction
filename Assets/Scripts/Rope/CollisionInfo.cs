using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionInfo
{
    public int id { get; set; }

    public ColliderType colliderType { get; set; }
    public Collider2D collider2D { get; set; }
    public Vector2 colliderSize { get; set; }
    public Vector2 position { get; set; }
    public Vector2 scale { get; set; }
    public Matrix4x4 wtl { get; set; }
    public Matrix4x4 ltw { get; set; }
    public int numCollisions { get; set; }
    public int[] collidingNodes { get; set; }  // You probably want to use byte[] here instead, unless you have >255 nodes.

    public CollisionInfo(int maxCollisions) {
        id = -1;
        colliderType = ColliderType.None;
        colliderSize = Vector2.zero;
        position = Vector2.zero;
        scale = Vector2.zero;
        wtl = Matrix4x4.zero;
        ltw = Matrix4x4.zero;

        numCollisions = 0;
        collidingNodes = new int[maxCollisions];
    }
}
