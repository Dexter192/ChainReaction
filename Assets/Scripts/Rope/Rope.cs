using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Rope : MonoBehaviour
{
    // Rope physics features
    [SerializeField] private float friction = 0.5f;
    [SerializeField] private float ropePullbackForce = 0.1f;
    [SerializeField] private Vector2 forceGravity = new(0f, -1f);

    // Rope features
    [SerializeField] private float ropeSegLen = 0.5f;
    [SerializeField] private int totalSegmentCount = 20;
    [SerializeField] private int numIterations = 10;
    [SerializeField] private float lineWidth = 1f;
    private List<RopeSegment> ropeSegments;

    // Gameobject links
    private GameObject startPoint;
    private GameObject endPoint;
    private Playerhandler _playerhandler;
    
    // Render
    private LineRenderer lineRenderer;
    private Vector2 ROPE_RENDER_OFFSET = new(0, 0.04f);

    private bool shouldSnapshotCollision;

    // Collision attributes
    // Stop the player from sliding if the changed position is small
    private const float STOP_PLAYER_SLIDING_DISTANCE = 0.0005f;
    // Dictionary collecting the colliding rope segments for each collider
    private Dictionary<Collider2D, HashSet<int>> collisionInfo = new();


    // Start is called before the first frame update    
    private void Start()
    {
        if (totalSegmentCount % 2 != 0)
        {
            Destroy(gameObject);
            throw new ArgumentException("Rope segments must be an even integer", nameof(totalSegmentCount));
        }
        
   
        //Disable rope for test simulation
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = totalSegmentCount;

        AssignConnectedPlayers();
        InitializeRopes();
    }
    public void ResetRopes()
    {   
        InitializeRopes();
    }
    private void InitializeRopes()
    {
        Vector3 startPos = startPoint.transform.position;
        Vector3 endPos = endPoint.transform.position;

        ropeSegments = new();

        for (int i = 0; i < totalSegmentCount; i++)
        {
            ropeSegments.Add(new RopeSegment(
                    new Vector2(startPos.x - (startPos.x - endPos.x) * i / (totalSegmentCount-1),
                                startPos.y - (startPos.y - endPos.y) * i / (totalSegmentCount-1)
                    )));
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (shouldSnapshotCollision)
        {
            collisionInfo = RopeCollider.SnapshotCollision(ropeSegments);
            shouldSnapshotCollision = false;
        }
        Simulate();
        DrawRope();
    }
    private void FixedUpdate()
    {
        shouldSnapshotCollision = true;
    }

    private void Simulate()
    {
        // SIMULATION - Physics part
        foreach (RopeSegment ropeSegment in ropeSegments)
        {
            Vector2 velocity = ropeSegment.posNow - ropeSegment.posOld;
            velocity *= friction;
            // Don't apply gravity if we are colliding
            if (!ropeSegment.isColliding)
            {
                velocity += forceGravity * Time.deltaTime;
            }
            ropeSegment.posOld = ropeSegment.posNow;
            ropeSegment.posNow += velocity;
        }

        // CONSTRAINTS
        for (int i = 0; i < numIterations; i++)
        {
            ApplyConstraints();
            ropeSegments = RopeCollider.AdjustCollision(ropeSegments, collisionInfo);
        }
    }

    private void ApplyConstraints()
    {
        // In order to simulate a realistic rope, we want to simulate the first half from the startpoint and the second half from the endpoint
        ApplyConstraintFirstHalf();
        ApplyConstraintSecondHalfBackwards();
    }
    private void ApplyConstraintSecondHalfBackwards()
    {

        // Last Segment is pinned to the second player
        AttachRopeSegmentToPlayer(endPoint, totalSegmentCount - 1);

        // CONSTRAINT - Ensure that rope segments are always a certain distance apart
        for (int i = totalSegmentCount-1; i >= totalSegmentCount/2; i--)
        {
            RopeSegment segment = ropeSegments[i];
            RopeSegment previousSegment = ropeSegments[i - 1];

            float dist = (previousSegment.posNow - segment.posNow).magnitude;
            float error = dist - ropeSegLen;
            Vector2 changeDir = (segment.posNow - previousSegment.posNow).normalized;
            Vector2 changeAmount = changeDir * error;
            float modifier = 0.5f;

            // The last segment (attached to the second player) allows the rope to stretch  
            if (i == totalSegmentCount - 1)
                modifier = ropePullbackForce;

            AdjustAdjacentSegments(segment, previousSegment, changeAmount, modifier);
        }

        AttachPlayerToRopeSegment(endPoint, totalSegmentCount - 1);
    }

    private void ApplyConstraintFirstHalf()
    {
        // If the player is moving, we pin the point to the player
        AttachRopeSegmentToPlayer(startPoint, 0);
        // CONSTRAINT - Ensure that rope segments are always a certain distance apart
        for (int i = 0; i < totalSegmentCount/2; i++)
        {
            RopeSegment segment = ropeSegments[i];
            RopeSegment nextSegment = ropeSegments[i + 1];

            float dist = (segment.posNow - nextSegment.posNow).magnitude;
            float error = dist - ropeSegLen;
            Vector2 changeDir = (segment.posNow - nextSegment.posNow).normalized;
            Vector2 changeAmount = changeDir * error;
            float modifier = 0.5f;

            // The first segment (attached to the first player) allows the rope to stretch  
            if (i == 0)
                modifier = ropePullbackForce;

            AdjustAdjacentSegments(segment, nextSegment, changeAmount, modifier);

        }
        
        AttachPlayerToRopeSegment(startPoint, 0);
    }

    private void AdjustAdjacentSegments(RopeSegment segment1, RopeSegment segment2, Vector2 changeAmount, float modifier = 0.5f)
    {
        segment1.posNow -= changeAmount * modifier;
        segment2.posNow += changeAmount * (1 - modifier);
    }

    private void AttachRopeSegmentToPlayer(GameObject player, int ropeSegmentIndex)
    {
        RopeSegment firstSegment = ropeSegments[ropeSegmentIndex];
        firstSegment.posNow = player.transform.position;
        ropeSegments[ropeSegmentIndex] = firstSegment;
    }

    private void AttachPlayerToRopeSegment(GameObject player, int ropeSegmentIndex)
    {
        RopeSegment ropeSegment = ropeSegments[ropeSegmentIndex];
        // Only attach the player back to the rope if the distance is significant
        Vector2 playerPosition = player.transform.position;
        if ((playerPosition - ropeSegment.posNow).magnitude > STOP_PLAYER_SLIDING_DISTANCE)
            player.transform.position = ropeSegment.posNow;
    }

    // Finds the two players which are connected to this rope
    private void AssignConnectedPlayers()
    {
        _playerhandler = Playerhandler.Instance;
        startPoint = transform.parent.gameObject;
        endPoint = _playerhandler.GetPreviousPlayer(startPoint);
        if (endPoint == null)
        {
            Destroy(gameObject);
        }
    }

    private void DrawRope()
    {
        Vector3[] ropePositions = new Vector3[totalSegmentCount];
        for (int i = 0; i < totalSegmentCount; i++)
        {
            ropePositions[i] = ropeSegments[i].posNow - ROPE_RENDER_OFFSET;
        }
        lineRenderer.SetPositions(ropePositions);
    }
}

