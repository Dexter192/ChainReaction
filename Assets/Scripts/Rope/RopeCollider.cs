using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

public class RopeCollider : MonoBehaviour
{
    [SerializeField]LineRenderer rope;
    EdgeCollider2D edgeCollider;
    Rigidbody2D rigidbody;

    Vector2[] points;

    private void Start()
    {
        transform.localPosition = transform.parent.position * -1;
  
        rigidbody = gameObject.AddComponent<Rigidbody2D>();

        edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
        edgeCollider.isTrigger = true;
        
        points = new Vector2[rope.positionCount];

        getNewPositions();

        edgeCollider.points = points;
    }

    private void Update()
    {
        getNewPositions();
        edgeCollider.points = points;

        transform.localPosition = transform.parent.position * -1;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Colliding with " + collision.GetType());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collision Trigger " + collision.tag);
    }

    private void getNewPositions()
    {
        for (int i = 0; i < points.Length; i++)
        {   
            points[i] = rope.GetPosition(i);
        }
    }
}