using System.Collections.Generic;
using System.ComponentModel.Design;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Diagnostics;
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
    [SerializeField] public int numIterations = 10;
    [SerializeField] private float lineWidth = 1f;

    [SerializeField] private GameObject endPoint;
    [SerializeField] private GameObject startPoint;
    [SerializeField] private LineRenderer lineRenderer;

    Playerhandler _playerhandler;
    private List<RopeSegment> ropeSegments = new();

    // Collision attributes
    // Collision radius around reach node. Set it high to avoid tunneling
    [SerializeField] public float COLLISION_RADIUS = 0.1f;
    // Stop the player from sliding if the change is position is less than this
    private const float STOP_PLAYER_SLIDING_DISTANCE = 0.0005f;
    // Dictionary collecting the colliding rope segments for each collider
    private Dictionary<Collider2D, HashSet<int>> collisionInfo = new();

    // Render the rope with an offset such that the rope doesn't hover above the ground
    private Vector2 ROPE_RENDER_OFFSET = new(0, 0.04f);

    private bool shouldSnapshotCollision;

    private void Start()
    {
        Debug.Log("Creating Test Rope");
        
        Vector3 startPos = startPoint.transform.position;
        Vector3 endPos = endPoint.transform.position;

        for (int i = 0; i < totalNodes; i++)
        {
            ropeSegments.Add(new RopeSegment(
                    new Vector2(startPos.x - (startPos.x - endPos.x) * i / (this.totalNodes),
                                startPos.y - (startPos.y - endPos.y) * i / (this.totalNodes)
                    )));
        }

        AttachRopeSegmentToPlayer(this.startPoint, 0);
        AttachRopeSegmentToPlayer(this.endPoint, this.totalNodes - 1);
    }

    private void Update()
    {
        if (shouldSnapshotCollision)
        {
            collisionInfo = TestRopeCollider.SnapshotCollision(ropeSegments);
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
        // SIMULATION - The physics part
        for (int i = 0; i < totalNodes - 1; i++)
        {
            RopeSegment segment = ropeSegments[i];
            Vector2 velocity = segment.posNow - segment.posOld;
            velocity *= friction;
            // Don't apply gravity if we are colliding
            if (!segment.isColliding)
                velocity += forceGravity * Time.deltaTime;

            segment.posOld = segment.posNow;
            segment.posNow += velocity;
            ropeSegments[i] = segment;
        }

        // CONSTRAINTS
        for (int i = 0; i < numIterations; i++)
        {
            ApplyConstraint();
            ropeSegments = TestRopeCollider.AdjustCollision(ropeSegments, collisionInfo);
        }
    }

    private void ApplyConstraint()
    {
        // If the player is moving, we pin the point to the player
        AttachRopeSegmentToPlayer(startPoint, 0);

        // Last Segment is pinned to the second player
        AttachRopeSegmentToPlayer(endPoint, totalNodes - 1);

        // CONSTRAINT - Ensure that rope segments are always a certain distance apart
        for (int i = 0; i < totalNodes -1; i++)
        {
            RopeSegment segment = ropeSegments[i];
            RopeSegment nextSegment = ropeSegments[i + 1];

            float dist = (segment.posNow - nextSegment.posNow).magnitude;
            float error = dist - ropeSegLen;
            Vector2 changeDir = (segment.posNow - nextSegment.posNow).normalized;
            Vector2 changeAmount = changeDir * error;

            if (i == 0)
            {
                segment.posNow -= changeAmount * ropePullbackForce;
                ropeSegments[i] = segment;
                nextSegment.posNow += changeAmount * (1-ropePullbackForce);
                ropeSegments[i + 1] = nextSegment;

                // Slow the player down if the rope is stretched
                ReducePlayersSpeedOnStretchedRope(startPoint, changeAmount);
            }
            else if (i == totalNodes - 2)
            {
                segment.posNow -= changeAmount * (1 - ropePullbackForce);
                ropeSegments[i] = segment;
                nextSegment.posNow += changeAmount * ropePullbackForce;
                ropeSegments[i + 1] = nextSegment;

                // Slow the player down if the rope is stretched
                ReducePlayersSpeedOnStretchedRope(endPoint, -changeAmount);
            }

            else
            {
                segment.posNow -= changeAmount * 0.5f;
                ropeSegments[i] = segment;
                nextSegment.posNow += changeAmount * 0.5f;
                ropeSegments[i + 1] = nextSegment;
            }

        }

        AttachPlayerToRopeSegment(startPoint, 0);
        AttachPlayerToRopeSegment(endPoint, totalNodes -1);
    }

    private void ReducePlayersSpeedOnStretchedRope(GameObject player, Vector2 changeAmount)
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        // Reduce the players speed depending on the rope stretch
        if (playerMovement.GetMovementState() != PlayerMovement.MovementState.idle)
        {
            playerMovement.AddVelocity(-changeAmount * ropeElasticity);
        }
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

    private void DrawRope()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Vector3[] ropePositions = new Vector3[totalNodes];
        for (int i = 0; i < totalNodes; i++)
        {
            // Render the line slightly lower such that it doesn't hover above the ground
            ropePositions[i] = ropeSegments[i].posNow - ROPE_RENDER_OFFSET;
        }
        lineRenderer.positionCount = ropePositions.Length;
        lineRenderer.SetPositions(ropePositions);
    }
}

