using System.Collections.Generic;
using UnityEngine;

public class RopeCollider : MonoBehaviour
{
    // Collision radius around reach node. Set it high to avoid tunneling
    [SerializeField] public static float COLLISION_RADIUS = 0.1f;

    public static Dictionary<Collider2D, HashSet<int>> SnapshotCollision(List<RopeSegment> ropeSegments)
    {
        // This seems like a lot of overhead to always recreate the hashset
        Dictionary<Collider2D, HashSet<int>> frameCollisionInfo = new();
        // Loop through each node and get collisions within a radius
        for (int i = 0; i < ropeSegments.Count; i++)
        {
            Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(ropeSegments[i].posNow, COLLISION_RADIUS);

            ropeSegments[i].isColliding = false;

            foreach (Collider2D col in overlappingColliders)
            {
                // Ignore collision with player
                if (col.CompareTag("MainCamera") || col.CompareTag("Player"))
                {
                    continue;
                }
                // Maybe a redundant check
                if (!col.bounds.Contains(ropeSegments[i].posNow))
                {
                    continue;
                }
                ropeSegments[i].isColliding = true;

                // Add the current rope segment as a collision object to the collider
                frameCollisionInfo.TryAdd(col, new(i));
                frameCollisionInfo[col].Add(i);
            }
        }
        return frameCollisionInfo;
    }

    // Takes a list of rope segment and a Dictionary with references for the 
    public static List<RopeSegment> AdjustCollision(List<RopeSegment> ropeSegments, Dictionary<Collider2D, HashSet<int>> collisionInfo)
    {
        foreach (KeyValuePair<Collider2D, HashSet<int>> colliderPair in collisionInfo)
        {
            Collider2D collider = colliderPair.Key;
            HashSet<int> collidingRopeSegments = colliderPair.Value;
            switch (collider)
            {
                case CircleCollider2D circleCollider:
                    AdjustCircleCollision(circleCollider, ropeSegments, collidingRopeSegments);
                    break;
                case BoxCollider2D boxCollider:
                    AdjustRectangleCollision(boxCollider, ropeSegments, collidingRopeSegments);
                    break;
                case CompositeCollider2D compositeCollider:
                    AdjustCompositeCollision(compositeCollider, ropeSegments, collidingRopeSegments);
                    break;
            }
        }
        return ropeSegments;
    }

    private static List<RopeSegment> AdjustCircleCollision(CircleCollider2D collider, List<RopeSegment> ropeSegments, HashSet<int> collidingRopeSegments)
    {
        float scale = Mathf.Max(collider.transform.localToWorldMatrix.GetColumn(0).magnitude,
                                collider.transform.localToWorldMatrix.GetColumn(1).magnitude);

        float radius = ((CircleCollider2D)collider).radius * scale;
        Vector2 colliderCenter = collider.bounds.center;
        foreach (int collidingRopeSegmentIndex in collidingRopeSegments)
        {
            RopeSegment node = ropeSegments[collidingRopeSegmentIndex];
            float distance = Vector2.Distance(colliderCenter, node.posNow);

            // Early out if we are not colliding
            if (distance - radius > 0) { continue; }

            // Push point outside circle
            Vector2 dir = (node.posNow - colliderCenter).normalized;
            Vector2 hitPos = colliderCenter + dir * radius;
            node.posNow = hitPos;
        }
        return ropeSegments;
    }

    private static List<RopeSegment> AdjustRectangleCollision(BoxCollider2D collider, List<RopeSegment> ropeSegments, HashSet<int> collidingRopeSegments)
    {
        Vector2 scalar = new(collider.transform.localToWorldMatrix.GetColumn(0).magnitude,
                     collider.transform.localToWorldMatrix.GetColumn(1).magnitude);

        Vector2 half = collider.size * .5f;

        foreach (int collidingRopeSegmentIndex in collidingRopeSegments)
        {
            RopeSegment node = ropeSegments[collidingRopeSegmentIndex];

            Vector2 localPoint = collider.transform.worldToLocalMatrix.MultiplyPoint(node.posNow);

            // If distance from center is more than box "radius" then we can't be colliding
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

            Vector2 hitPos = collider.transform.localToWorldMatrix.MultiplyPoint(localPoint);
            node.posNow = hitPos;
        }
        return ropeSegments;
    }

    private static List<RopeSegment> AdjustCompositeCollision(CompositeCollider2D collider, List<RopeSegment> ropeSegments, HashSet<int> collidingRopeSegments)
    {
        foreach (int collidingRopeSegmentIndex in collidingRopeSegments)
        {
            RopeSegment node = ropeSegments[collidingRopeSegmentIndex];
            // If we are not in the collider, we can ignore the collider (It would make sense to move this to the collision check)

            // First rough check if the bounding box contains the point
            if (!collider.bounds.Contains(node.posNow))
            {
                continue;
            }
            float minDistance = Mathf.Infinity;
            int minDistanceIndex = -1;

            // Iterate over all paths and adjust the collision if we overlap                            
            for (int k = 0; k < collider.pathCount; k++)
            {
                Vector2[] points = new Vector2[collider.GetPathPointCount(k)];
                collider.GetPath(k, points);
                for (int l = 0; l < points.Length; l++)
                {
                    points[l] = collider.transform.localToWorldMatrix.MultiplyPoint(points[l]);
                }
                // Check if the point is intersecting with the current polygon collider (using raycasting)
                if (IsPointInPolygon(node.posNow, points))
                {
                    // Find the minimal distance between the edges of the polygon and the position 
                    for (int l = 0; l < points.Length - 1; l++)
                    {
                        float distance = DistancePointAndLine(points[l], points[l + 1], node.posNow);
                        if (minDistance > distance)
                        {
                            minDistance = distance;
                            minDistanceIndex = l;
                            if (minDistance < COLLISION_RADIUS)
                            {
                                break;
                            }
                        }
                    }
                    for (int l = 0; l < points.Length; l++)
                    {
                       // points[l] = collider.transform.worldToLocalMatrix.MultiplyPoint(points[l]);
                    }

                    node.posNow = ProjectPointOnLine(points[minDistanceIndex], points[minDistanceIndex + 1], node.posNow);
                    break;
                }
            }
        }
        return ropeSegments;
    }

    // Iterate over the edges of a given polygon and return if a point is inside the polygon using an adaptation of the raycast algorithm 
    // Since we always have 90 degree angles we can speed up the algorithm
    private static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        int countIntersections = 0;
        for (int l = 0; l < polygon.Length - 1; l++)
        {
            if (HorizontalRayIntersectsWithEdge(point, polygon[l], polygon[l + 1]))
                countIntersections++;
        }
        // If the number of intersections is odd, then the point is in the polygon
        return (countIntersections % 2) == 1;
    }

    // Assumes that |l1,l2| are either horizontal (1,0) or vertical (0,1) since we always have 90 degree edges in the polygon
    private static bool HorizontalRayIntersectsWithEdge(Vector2 p, Vector2 l1, Vector2 l2)
    {
        Vector2 lineDir = (l1 - l2).normalized;
        // Cast a horizontal ray
        Vector2 rayDir = new Vector2(1, 0);

        // If the directions are the same, the line is parallel to the ray. Thus no intersection
        if (Mathf.Abs(lineDir.x) == rayDir.x && Mathf.Abs(lineDir.y) == rayDir.y)
            return false;

        // Since the line is not parallel, we know it is orthogonal. Thus l1.x == l2.x. If p.x > l1.x the point is to the right of the line the ray does not intersect
        if (p.x > l1.x)
            return false;

        // Point is below the polygon edge
        if (p.y < Mathf.Min(l1.y, l2.y))
            return false;

        // Point is above the polygon edge
        if (p.y > Mathf.Max(l1.y, l2.y))
            return false;

        // If none of the conditions are true, we intersect with the line
        return true;
    }

    private static float DistancePointAndLine(Vector2 startP, Vector2 endP, Vector2 point)
    {
        Vector2 direction = (startP - endP).normalized;
        return Mathf.Abs((point.x - startP.x) * direction.y - (point.y - startP.y) * direction.x);
    }

    private static Vector2 ProjectPointOnLine(Vector2 startP, Vector2 endP, Vector2 point)
    {
        Vector2 direction = (startP - endP).normalized;
        Vector2 lhs = point - startP;

        float dotP = Vector2.Dot(lhs, direction);
        return startP + direction * dotP;
    }

}