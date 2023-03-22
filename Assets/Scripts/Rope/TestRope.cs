using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ColliderType
{
    Circle,
    Box,
    CompositeCollider,
    None
}
public class TestRope : MonoBehaviour
{
    // Rope Physics features
    [SerializeField] private float friction = 0.5f;
    [SerializeField] private float ropeElasticity = 0.001f;
    [SerializeField] private float ropePullbackForce = 0.1f;
    [SerializeField] Vector2 forceGravity = new Vector2(0f, -1f);

    // Rope features
    [SerializeField] private float ropeSegLen = 0.5f;
    [SerializeField] private int totalNodes = 20;
    [SerializeField] private float lineWidth = 1f;

    [SerializeField] private GameObject endPoint;
    [SerializeField] private GameObject startPoint;
    [SerializeField] private LineRenderer lineRenderer;

    Playerhandler _playerhandler;
    private List<RopeSegment> ropeNodes = new List<RopeSegment>();

    // Collision attributes
    // Maximum total number of colliders that the rope can touch
    private const int MAX_ROPE_COLLISIONS = 32;
    // Collision radius around reach node. Set it high to avoid tunneling
    [SerializeField] public float COLLISION_RADIUS = 0.25f;
    // Collider buffer size; the maximum number of colliders that a single node can touch at once.
    private const int COLLIDER_BUFFER_SIZE = 8;

    // -- *snip* --
    private int numCollisions;
    private bool shouldSnapshotCollision;
    private CollisionInfo[] collisionInfos;
    private Collider2D[] colliderBuffer;

    private void Awake()
    {
        // -- *snip* --
        // Allocate collision structures.
        collisionInfos = new CollisionInfo[MAX_ROPE_COLLISIONS];
        for (int i = 0; i < collisionInfos.Length; i++)
        {
            collisionInfos[i] = new CollisionInfo(totalNodes);
        }

        colliderBuffer = new Collider2D[MAX_ROPE_COLLISIONS];
    }

    private void Start()
    {
        Debug.Log("Creating Test Rope");
        
        Vector3 startPos = startPoint.transform.position;
        Vector3 endPos = endPoint.transform.position;

        for (int i = 0; i < totalNodes; i++)
        {
            ropeNodes.Add(new RopeSegment(
                    new Vector2((startPos.x - endPos.x) * i / (this.totalNodes),
                                (startPos.y - endPos.y) * i / (this.totalNodes)
                    )));
        }

        AttachRopeSegmentToPlayer(this.startPoint, 0);
        AttachRopeSegmentToPlayer(this.endPoint, this.totalNodes - 1);
    }

    private void Update()
    {
        if (shouldSnapshotCollision)
        {
            SnapshotCollision();
        }
        Simulate();
        this.DrawRope();
    }

    private void FixedUpdate()
    {
        shouldSnapshotCollision = true;
        //this.Simulate(); // Moved to Simulate
    }

    private void SnapshotCollision()
    {
        numCollisions = 0;
        // Loop through each node and get collisions within a radius
        for (int i = 0; i < totalNodes; i++)
        {
            Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(ropeNodes[i].posNow, COLLISION_RADIUS);
            foreach (Collider2D col in  overlappingColliders) { 
            //for (int j = 0; j < collisions; j++) {
                //Collider2D col = colliderBuffer[j];
                int id = col.GetInstanceID();

                // Ignore collision with player
                if (col.tag == "Player")
                {
                    continue;
                }
                if (!col.bounds.Contains(ropeNodes[i].posNow))
                {
                    continue;
                }

                // Check if we already have this collider in our collisionInfos
                // Consider replacing this with a HashSet of some form.
                int idx = -1;
                for (int k = 0; k < numCollisions; k++) {
                    if (collisionInfos[k].id == id)
                    {
                        idx = k;
                        break;
                    }
                }

                // If we didn'T have the collider, we need to add it 
                CollisionInfo ci = collisionInfos[numCollisions];
                if (idx == -1)
                {
                    // Record all the data we need to use into our class.
                    ci.collider2D = col;
                    ci.id = id;
                    ci.wtl = col.transform.worldToLocalMatrix;
                    ci.ltw = col.transform.localToWorldMatrix;
                    ci.scale = new Vector2(ci.ltw.GetColumn(0).magnitude,
                                           ci.ltw.GetColumn(1).magnitude);
                    ci.position = col.transform.position;
                    ci.numCollisions = 1; // 1 collision, this one
                    ci.collidingNodes[0] = i;

                    switch (col)
                    {
                        case CircleCollider2D c:
                            ci.colliderType = ColliderType.Circle;
                            ci.colliderSize = new Vector2(c.radius, c.radius);
                            break;
                        case BoxCollider2D b:
                            ci.colliderType = ColliderType.Box;
                            ci.colliderSize = b.size;
                            break;
                        case CompositeCollider2D c:
                            ci.colliderType = ColliderType.CompositeCollider;
                            break;
                        default:
                            ci.colliderType = ColliderType.None;
                            break;
                    }

                    numCollisions++;
                    if (numCollisions >= MAX_ROPE_COLLISIONS)
                    {
                        return;
                    }
                }
                // If we found the collider, the we just have to increment the collisions and add our node
                else
                {
                    ci = collisionInfos[idx];
                    if (ci.numCollisions >= MAX_ROPE_COLLISIONS - 1)
                    {
                        continue;
                    }
                    ci.collidingNodes[ci.numCollisions++] = i;
                }
            }
            shouldSnapshotCollision = false;
        }
    }

    private void AdjustCollision()
    {
        for (int i = 0; i < numCollisions; i++)
        {
            CollisionInfo ci = collisionInfos[i];

            switch (ci.colliderType)
            {
            case ColliderType.Circle:
                {
                    float radius = ci.colliderSize.x * Mathf.Max(ci.scale.x, ci.scale.y);
                    for (int j = 0; j < ci.numCollisions; j++)
                    {
                        RopeSegment node = ropeNodes[ci.collidingNodes[j]];
                        float distance = Vector2.Distance(ci.position, node.posNow);

                        // Early out if we are not colliding
                        if (distance - radius > 0) { continue; }

                        // Push point outside circle
                        Vector2 dir = (node.posNow - ci.position).normalized;
                        Vector2 hitPos = ci.position + dir * radius;
                        node.posNow = hitPos;
                    }
                }
                break;
            case ColliderType.Box:
                {
                    for (int j = 0; j < ci.numCollisions; j++)
                    {
                        RopeSegment node = ropeNodes[ci.collidingNodes[j]];
                        Vector2 pointOnPerimeter = ci.collider2D.ClosestPoint(node.posNow);
                        // Only adjust the point if it is inside the collider
                        
                        
                        if (ci.collider2D.bounds.Contains(node.posNow))
                        {
                            Vector2 adjustDirection = (node.posNow - pointOnPerimeter).normalized;
                            Vector2 newPosNow = pointOnPerimeter + adjustDirection * ci.colliderSize;
                        }
                                

                        // SOLUTION FROM TUTORIAL - DONT DELETE UNLESS MY SOLUTION WORKS
                        Vector2 localPoint = ci.wtl.MultiplyPoint(node.posNow);

                        // If distance from center is more than box "radius" then we can't be colliding
                        Vector2 half = ci.colliderSize * .5f;
                        Vector2 scalar = ci.scale;
                        float dx = localPoint.x;
                        float px = half.x - Mathf.Abs(dx);
                        if (px <= 0)
                        {
                            continue;
                        }

                        float dy = localPoint.y;
                        float py = half.y - Mathf.Abs(dy);
                        if (py <= 0)
                        {
                            continue;
                        }

                        // Push node out along the closest edge.
                        // Need to multiply distance by scale or we'll mess up on scaled box corners.
                        if (px * scalar.x < py * scalar.y)
                        {
                            float sx = Mathf.Sign(dx);
                            localPoint.x = half.x * sx;
                        }
                        else
                        {
                            float sy = Mathf.Sign(dy);
                            localPoint.y = half.y * sy;
                        }

                        Vector2 hitPos = ci.ltw.MultiplyPoint(localPoint);
                        node.posNow = hitPos;
                    }
                }
                break;
                // TODO: Break up in methods
                case ColliderType.CompositeCollider:
                {
                    for (int j = 0; j < ci.numCollisions; j++)
                    {
                        RopeSegment node = ropeNodes[ci.collidingNodes[j]];
                        // If we are not in the collider, we can ignore the collider (It would make sense to move this to the collision check)
                        if (!ci.collider2D.bounds.Contains(node.posNow))
                        {
                            continue;
                        }
                        CompositeCollider2D col = (CompositeCollider2D)ci.collider2D;
                        float minDistance = Mathf.Infinity;
                        Vector2 newPos = Vector2.zero;
                        Vector2 posOnPerimeter = Vector2.zero;
                        // Iterate over all paths and adjust the collision if we overlap
                        PolygonCollider2D polygonCollider2D = gameObject.AddComponent<PolygonCollider2D>();
                        polygonCollider2D.pathCount = 1;
                            
                        for (int k = 0; k < col.pathCount; k++) {
                            Vector2[] points = new Vector2[col.GetPathPointCount(k)];
                            col.GetPath(k, points);
    
                            // Check if the point is intersecting with the current path - Maybe obsolete(?) 
                            polygonCollider2D.SetPath(0, points);    
                            if (polygonCollider2D.bounds.Contains(node.posNow))
                            {
                                // Find the minimal distance between the edges of the polygon and the position 
                                for (int l = 0; l < points.Length - 1; l++)
                                {
                                    float distance = DistancePointAndLine(points[l], points[l + 1], node.posNow);
                                    if (minDistance > distance)
                                    {
                                        minDistance = distance;
                                        posOnPerimeter = ProjectPointOnLine(points[l], points[l + 1], node.posNow);
                                            newPos = posOnPerimeter + (posOnPerimeter - node.posNow).normalized * 0.005f;
                                    }                            
                                }
                                node.posOld = node.posNow;
                                node.posNow = newPos;
                            }
                        }
                        Destroy(polygonCollider2D);
                    }
                }
                break;
            }
        } 
    }

    private float DistancePointAndLine(Vector2 startP, Vector2 endP, Vector2 point)
    {
        Vector2 direction = (startP - endP).normalized;
        return Mathf.Abs((point.x - startP.x) * direction.y - (point.y - startP.y) * direction.x);
    }
    private Vector2 ProjectPointOnLine(Vector2 startP, Vector2 endP, Vector2 point)
    {
        Vector2 direction = (startP - endP).normalized;
        Vector2 lhs = point - startP;

        float dotP = Vector2.Dot(lhs, direction);
        return startP + direction * dotP;
    }
    private void ApplyConstraint()
    {
        // If the player is moving, we pin the point to the player
        AttachRopeSegmentToPlayer(this.startPoint, 0);

        // Last Segment is pinned to the second player
        AttachRopeSegmentToPlayer(this.endPoint, this.totalNodes - 1);

        // CONSTRAINT - Ensure that rope segments are always a certain distance apart
        for (int i = 0; i < this.totalNodes-1; i++)
        {
            RopeSegment segment = this.ropeNodes[i];
            RopeSegment nextSegment = this.ropeNodes[i + 1];

            float dist = (segment.posNow - nextSegment.posNow).magnitude;
            float error = Mathf.Abs(dist - this.ropeSegLen);
            Vector2 changeDir = Vector2.zero;

            if (dist > ropeSegLen)
            {
                changeDir = (segment.posNow - nextSegment.posNow).normalized;
            }
            else if (dist < ropeSegLen)
            {
                changeDir = (nextSegment.posNow - segment.posNow).normalized;
            }
            Vector2 changeAmount = changeDir * error;
            if (i == 0)
            {
                segment.posNow -= changeAmount * this.ropePullbackForce;
                this.ropeNodes[i] = segment;
                nextSegment.posNow += changeAmount * (1-this.ropePullbackForce);
                this.ropeNodes[i + 1] = nextSegment;

                // Slow the player down if the rope is stretched
                ReducePlayersSpeedOnStretchedRope(this.startPoint, changeAmount);
            }
            else if (i == this.totalNodes - 2)
            {
                segment.posNow -= changeAmount * (1 - this.ropePullbackForce);
                this.ropeNodes[i] = segment;
                nextSegment.posNow += changeAmount * this.ropePullbackForce;
                this.ropeNodes[i + 1] = nextSegment;

                // Slow the player down if the rope is stretched
                ReducePlayersSpeedOnStretchedRope(this.endPoint, -changeAmount);
            }

            else
            {
                segment.posNow -= changeAmount * 0.5f;
                this.ropeNodes[i] = segment;
                nextSegment.posNow += changeAmount * 0.5f;
                this.ropeNodes[i + 1] = nextSegment;
            }

        }

        AttachPlayerToRopeSegment(this.startPoint, 0);
        AttachPlayerToRopeSegment(this.endPoint, this.totalNodes-1);
    }

    private void ReducePlayersSpeedOnStretchedRope(GameObject player, Vector2 changeAmount)
    {
        // Reduce the players speed depending on the rope stretch
        if (player.GetComponent<PlayerMovement>().GetMovementState() != PlayerMovement.MovementState.idle)
        {
            player.GetComponent<PlayerMovement>().AddVelocity(-changeAmount * this.ropeElasticity);
        }
    }
    private void AttachRopeSegmentToPlayer(GameObject player, int ropeSegmentIndex)
    {
        RopeSegment firstSegment = this.ropeNodes[ropeSegmentIndex];
        firstSegment.posNow = player.transform.position;
        this.ropeNodes[ropeSegmentIndex] = firstSegment;
    }

    private void AttachPlayerToRopeSegment(GameObject player, int ropeSegmentIndex)
    {
        RopeSegment ropeSegment = this.ropeNodes[ropeSegmentIndex];
        // If the player is not moving, drag him back if the line is under tension 
        player.transform.position = ropeSegment.posNow;
    }

    private void DrawRope()
    {
        float lineWidth = this.lineWidth;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Vector3[] ropePositions = new Vector3[this.totalNodes];
        for (int i = 0; i < this.totalNodes; i++)
        {
            ropePositions[i] = this.ropeNodes[i].posNow;
        }
        lineRenderer.positionCount = ropePositions.Length;
        lineRenderer.SetPositions(ropePositions);
    }


    private void Simulate()
    {
        // SIMULATION - The physics part
        for (int i = 0; i < totalNodes-1; i++)
        {
            RopeSegment segment = this.ropeNodes[i];
            Vector2 velocity = segment.posNow - segment.posOld;
            velocity = velocity * this.friction;
            segment.posOld = segment.posNow;
            segment.posNow += velocity;
            segment.posNow += forceGravity * Time.deltaTime;           
            this.ropeNodes[i] = segment;
        }

        // CONSTRAINTS
        for (int i = 0; i < 50; i++)
        {
            ApplyConstraint();
            SnapshotCollision();
            AdjustCollision();
        }
    }
}

