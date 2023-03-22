using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Rope : MonoBehaviour
{

    [SerializeField] private float friction = 0.5f;
    [SerializeField] private float lineWidth = 1f;
    [SerializeField] private float ropeElasticity = 0.001f;
    [SerializeField] private float ropePullbackForce = 0.1f;
    [SerializeField] private float ropeSegLen = 0.5f;
    [SerializeField] private int segmentLength = 20;

    private GameObject startPoint;
    private GameObject endPoint;

    Playerhandler _playerhandler;
    private LineRenderer lineRenderer;
    private List<RopeSegment> ropeSegments;

    private void Awake()
    {
        
    }
    // Start is called before the first frame update    
    private void Start()
    {
        //Disable rope for test simulation
        //Destroy(gameObject);
        lineRenderer = GetComponent<LineRenderer>();

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
        Vector3 direction = (startPos - endPos).normalized;
        Debug.Log("Player transform position: " + startPos);
        Debug.Log("Rope transform position: " + gameObject.transform.position);

        this.ropeSegments = new();

        for (int i = 0; i < segmentLength; i++)
        {
            ropeSegments.Add(new RopeSegment(startPos + direction * (i+1) / segmentLength));
        }

        AttachRopeSegmentToPlayer(startPoint, 0);
        AttachRopeSegmentToPlayer(endPoint, segmentLength - 1);

    }

    // Update is called once per frame
    private void Update()
    {
        DrawRope();
    }
    private void FixedUpdate()
    {
        Simulate();
    }

    private void Simulate()
    {
        // SIMULATION
        Vector2 forceGravity = new Vector2(0f, -1f);
        for (int i = 0; i < segmentLength; i++)
        {
            RopeSegment segment = ropeSegments[i];
            Vector2 velocity = segment.posNow - segment.posOld;
            velocity *= friction;
            segment.posOld = segment.posNow;
            segment.posNow += velocity;
            segment.posNow += forceGravity * Time.deltaTime;
            ropeSegments[i] = segment;
        }

        // CONSTRAINTS
        for (int i = 0; i < 50; i++)
        {
            ApplyConstraint();
        }
    }
    private void ApplyConstraint()
    {
        // If the player is moving, we pin the point to the player
        AttachRopeSegmentToPlayer(startPoint, 0);

        // Last Segment is pinned to the second player
        AttachRopeSegmentToPlayer(endPoint, segmentLength - 1);

        // CONSTRAINT - Ensure that rope segments are always a certain distance apart
        for (int i = 0; i < segmentLength - 1; i++)
        {
            RopeSegment segment = ropeSegments[i];
            RopeSegment nextSegment = ropeSegments[i + 1];

            float dist = (segment.posNow - nextSegment.posNow).magnitude;
            float error = dist - ropeSegLen;
            Vector2 changeDir = (segment.posNow - nextSegment.posNow).normalized;
            Vector2 changeAmount = changeDir * error;

            // Handle first segment (attached to the first player)
            if (i == 0)
            {
                segment.posNow -= changeAmount * ropePullbackForce;
                ropeSegments[i] = segment;
                nextSegment.posNow += changeAmount * (1 - ropePullbackForce);
                ropeSegments[i + 1] = nextSegment;

                // Slow the player down if the rope is stretched
                ReducePlayersSpeedOnStretchedRope(startPoint, changeAmount);
            }

            // Handle last segment (attached to the second player)
            else if (i == segmentLength - 2)
            {
                segment.posNow -= changeAmount * (1 - ropePullbackForce);
                ropeSegments[i] = segment;
                nextSegment.posNow += changeAmount * ropePullbackForce;
                ropeSegments[i + 1] = nextSegment;

                // Slow the player down if the rope is stretched
                ReducePlayersSpeedOnStretchedRope(endPoint, -changeAmount);
            }

            // Handle Segments in the middle
            else
            {
                segment.posNow -= changeAmount * 0.5f;
                ropeSegments[i] = segment;
                nextSegment.posNow += changeAmount * 0.5f;
                ropeSegments[i + 1] = nextSegment;
            }

        }
        
        AttachPlayerToRopeSegment(startPoint, 0);
        AttachPlayerToRopeSegment(endPoint, segmentLength - 1);
    }

    private void ReducePlayersSpeedOnStretchedRope(GameObject player, Vector2 changeAmount)
    {
        if (player.GetComponent<PlayerMovement>().GetMovementState() != PlayerMovement.MovementState.idle)
        {
            player.GetComponent<PlayerMovement>().AddVelocity(-changeAmount * ropeElasticity);
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
        player.transform.position = ropeSegments[ropeSegmentIndex].posNow;
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
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Vector3[] ropePositions = new Vector3[segmentLength];
        for (int i = 0; i < segmentLength; i++)
        {
            ropePositions[i] = ropeSegments[i].posNow;
        }
        lineRenderer.positionCount = ropePositions.Length;
        lineRenderer.SetPositions(ropePositions);
    }
}

