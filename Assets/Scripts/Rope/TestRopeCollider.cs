using UnityEngine;

public class TestRopeCollider : MonoBehaviour
{
    [SerializeField] LineRenderer rope;
    [SerializeField] GameObject player1;
    [SerializeField] GameObject player2;
    PolygonCollider2D polygonCollider;
    HingeJoint2D joint1;
    HingeJoint2D joint2;

    Vector2[] points;

    private void Start()
    {
        points = new Vector2[rope.positionCount];
        polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        //rb.bodyType = RigidbodyType2D.Static;
        rb.mass = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        joint1 = gameObject.AddComponent<HingeJoint2D>();
        joint1.connectedBody = player1.GetComponent<Rigidbody2D>();
        joint1.autoConfigureConnectedAnchor = false;
        joint1.connectedAnchor = new Vector2 (0, 0);    
        //joint1.enabled = false;
        joint2 = gameObject.AddComponent<HingeJoint2D>();
        joint2.connectedBody = player2.GetComponent<Rigidbody2D>();
        joint2.autoConfigureConnectedAnchor = false;
        joint2.connectedAnchor = new Vector2(0, 0);
        //joint2.enabled = false;

        Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), player1.GetComponent<Collider2D>(), true);
        Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), player2.GetComponent<Collider2D>(), true);

        getNewPositions();
        UpdateColliders();

    }

    private void Update()
    {
        getNewPositions();
        UpdateColliders();

        joint1.anchor = player1.transform.localPosition;
        joint2.anchor = player2.transform.localPosition;
        //transform.position = new Vector3(0,0,0);
        //transform.localPosition = transform.parent.position * -1;
    }

    private void UpdateColliders()
    {
        polygonCollider.pathCount = rope.positionCount - 1;

        for (int i = 0; i < polygonCollider.pathCount; i++) 
        {
            int widthIndex = i * rope.widthCurve.length / polygonCollider.pathCount;
            float width = rope.widthCurve[widthIndex].value;

            Vector2 start1 = rope.GetPosition(i);
            start1.y += width;
            Vector2 start2 = rope.GetPosition(i);
            start2.y -= width;
            Vector2 end1 = rope.GetPosition(i + 1);
            end1.y -= width;
            Vector2 end2 = rope.GetPosition(i + 1);
            end2.y += width;
            Vector2[] path = { start1, start2, end1, end2};
            polygonCollider.SetPath(i, path);
        }
    }    

    private void getNewPositions()
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = rope.GetPosition(i);
        }
    }
}