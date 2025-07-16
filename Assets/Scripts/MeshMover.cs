using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PathPoint
{
    public Transform point;
    [Tooltip("Time to reach the NEXT point from this one (seconds)")]
    public float timeToNext = 1f;
}

public class MeshMover : MonoBehaviour
{
    [Header("Path to follow")]
    public List<PathPoint> pathPoints = new List<PathPoint>();

    [Header("Moving Mesh")]
    public Transform meshToMove;

    public void StartMoving()
    {

        FindObjectOfType<EndGameScript>().enabled = true;

        if (pathPoints.Count < 2 || meshToMove == null)
        {
            Debug.LogWarning("MeshMover: Not enough points or mesh not assigned.");
            return;
        }

        StartCoroutine(MoveAlongPath());
        FindObjectOfType<EndGameScript>().TriggerEndSequence();
    }

    IEnumerator MoveAlongPath()
    {
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Vector3 startPos = pathPoints[i].point.position;
            Vector3 endPos = pathPoints[i + 1].point.position;
            float segmentDuration = pathPoints[i].timeToNext;

            float elapsed = 0f;

            while (elapsed < segmentDuration)
            {
                float t = elapsed / segmentDuration;
                meshToMove.position = Vector3.Lerp(startPos, endPos, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Snap exactly to the target point
            meshToMove.position = endPos;
        }

        Debug.Log("MeshMover: Finished moving along custom path!");
    }
}
