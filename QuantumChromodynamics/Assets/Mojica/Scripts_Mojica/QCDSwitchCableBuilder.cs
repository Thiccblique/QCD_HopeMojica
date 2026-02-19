using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class QCDSwitchCableBuilder : MonoBehaviour
{
    [Header("Cable Prefabs")]
    public GameObject horizontalSegmentPrefab;
    public GameObject verticalSegmentPrefab;

    [Header("Settings")]
    public float segmentLength = 1f;

    [Header("Triggered Object")]
    public GameObject activatedObject;

    [HideInInspector]
    public List<GameObject> spawnedSegments = new List<GameObject>();

    public Vector2 currentDirection = Vector2.right;
    public string directionString = "Right";

    // Tip where the next segment will spawn
    [SerializeField] private Vector3 currentEndPosition;

    // Stack to track all previous tips for undo
    [SerializeField] private Stack<Vector3> endPositionsStack = new Stack<Vector3>();

    private void Start()
    {
        currentEndPosition = transform.position;
    }

    /// <summary>
    /// Adds a straight segment in the current direction
    /// </summary>
    public void AddStraightSegment()
    {
        CleanNullSegments();

        GameObject prefabToSpawn = GetCorrectStraightPrefab();
        if (prefabToSpawn == null) return;

        // Push the current tip onto the stack
        endPositionsStack.Push(currentEndPosition);

        // Calculate spawn position
        Vector3 spawnPos = currentEndPosition + (Vector3)(currentDirection * (segmentLength * 0.5f));

        GameObject segment = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, transform);

        spawnedSegments.Add(segment);

        // Update tip for next segment
        currentEndPosition = spawnPos + (Vector3)(currentDirection * (segmentLength * 0.5f));
    }

    /// <summary>
    /// Rotates the cable direction 90° clockwise
    /// </summary>
    public void RotateDirection90()
    {
        currentDirection = new Vector2(-currentDirection.y, currentDirection.x);
        UpdateDirectionString();
    }

    /// <summary>
    /// Undoes the last segment
    /// </summary>
    public void UndoLastSegment()
    {
        if (spawnedSegments.Count == 0 || endPositionsStack.Count == 0)
            return;

        GameObject lastSegment = spawnedSegments[^1];
        spawnedSegments.RemoveAt(spawnedSegments.Count - 1);

        if (lastSegment != null)
            DestroyImmediate(lastSegment);

        // Restore the previous tip from the stack
        currentEndPosition = endPositionsStack.Pop();
    }

    /// <summary>
    /// Clears all spawned cable segments
    /// </summary>
    public void ClearCable()
    {
        foreach (var segment in spawnedSegments)
        {
            if (segment != null)
                DestroyImmediate(segment);
        }

        spawnedSegments.Clear();
        endPositionsStack.Clear();
        currentEndPosition = transform.position;
    }

    /// <summary>
    /// Returns the correct prefab (horizontal or vertical) based on current direction
    /// </summary>
    private GameObject GetCorrectStraightPrefab()
    {
        if (currentDirection == Vector2.right || currentDirection == Vector2.left)
            return horizontalSegmentPrefab;
        if (currentDirection == Vector2.up || currentDirection == Vector2.down)
            return verticalSegmentPrefab;

        return null;
    }

    /// <summary>
    /// Removes null segments from the list
    /// </summary>
    private void CleanNullSegments()
    {
        spawnedSegments.RemoveAll(item => item == null);

        if (spawnedSegments.Count == 0)
        {
            currentEndPosition = transform.position;
            endPositionsStack.Clear();
        }
    }

    /// <summary>
    /// Updates the direction string for display
    /// </summary>
    private void UpdateDirectionString()
    {
        if (currentDirection == Vector2.right)
            directionString = "Right";
        else if (currentDirection == Vector2.up)
            directionString = "Up";
        else if (currentDirection == Vector2.left)
            directionString = "Left";
        else
            directionString = "Down";
    }
}
