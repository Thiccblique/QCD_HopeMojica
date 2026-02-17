using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ProbeManager : MonoBehaviour
{
    [Header("Probe GameObjects")]
    [SerializeField] private GameObject probe1;
    [SerializeField] private GameObject probe2;
    [SerializeField] private GameObject probe3;

    [Header("Float Settings")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float floatFrequency = 1f;

    [Header("Swap Settings")]
    [SerializeField] private Vector2 swapTimeRange = new Vector2(5f, 10f);
    [SerializeField] private int swapsPerCycle = 4;
    [SerializeField] private float swapDelay = 0.15f;

    private float swapTimer;
    private int currentSwapCount = 0;
    private bool isSwapping = false;
    private float burstTimer;
    private List<ProbeData> probeDataList = new List<ProbeData>();
    private class ProbeData
    {
        public GameObject probe;
        public Transform targetTransform;
        public float timeOffset;
    }

    void Start()
    {
        AssignProbePositions();
        swapTimer = Random.Range(swapTimeRange.x, swapTimeRange.y);
    }

    private void AssignProbePositions()
    {
        // Get all child transforms
        List<Transform> childPositions = new List<Transform>();
        for (int i = 0; i < transform.childCount && i < 3; i++)
        {
            childPositions.Add(transform.GetChild(i));
        }

        // Validate we have 3 children
        if (childPositions.Count < 3)
        {
            Debug.LogWarning($"ProbeManager requires 3 children, but only {childPositions.Count} found.");
            return;
        }

        // Create list of probes
        List<GameObject> probes = new List<GameObject> { probe1, probe2, probe3 };
        
        // Validate all probes are assigned
        if (probes.Any(p => p == null))
        {
            Debug.LogWarning("ProbeManager: One or more probe GameObjects are not assigned in the inspector.");
            return;
        }

        // Shuffle the probes randomly
        ShuffleList(probes);

        // Assign shuffled probes to target positions with random time offsets
        for (int i = 0; i < probes.Count; i++)
        {
            ProbeData data = new ProbeData
            {
                probe = probes[i],
                targetTransform = childPositions[i],
                timeOffset = Random.Range(0f, Mathf.PI * 2f)
            };
            probeDataList.Add(data);
            Debug.Log($"Assigned {probes[i].name} to follow {childPositions[i].name}");
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        // Fisher-Yates shuffle algorithm
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    void Update()
    {
        // Handle burst swapping
        if (isSwapping)
        {
            burstTimer -= Time.deltaTime;
            if (burstTimer <= 0f)
            {
                SwapProbePositions();
                currentSwapCount++;
                
                if (currentSwapCount >= swapsPerCycle)
                {
                    // Done with burst, reset for next cycle
                    isSwapping = false;
                    currentSwapCount = 0;
                    swapTimer = Random.Range(swapTimeRange.x, swapTimeRange.y);
                }
                else
                {
                    // Continue burst
                    burstTimer = swapDelay;
                }
            }
        }
        else
        {
            // Countdown to next burst
            swapTimer -= Time.deltaTime;
            if (swapTimer <= 0f)
            {
                // Start burst
                isSwapping = true;
                burstTimer = 0f; // First swap happens immediately
            }
        }

        // Update each probe's position with floaty motion
        foreach (var data in probeDataList)
        {
            if (data.probe == null || data.targetTransform == null) continue;

            // Calculate floating offset using sine wave
            float floatOffset = Mathf.Sin((Time.time * floatFrequency) + data.timeOffset) * floatAmplitude;
            Vector3 floatVector = new Vector3(0, floatOffset, 0);

            // Target position with float offset
            Vector3 targetWithFloat = data.targetTransform.position + floatVector;

            // Smoothly move toward target with delay
            data.probe.transform.position = Vector3.Lerp(
                data.probe.transform.position,
                targetWithFloat,
                Time.deltaTime * followSpeed
            );
        }
    }

    private void SwapProbePositions()
    {
        // Collect all current target transforms
        List<Transform> targets = new List<Transform>();
        foreach (var data in probeDataList)
        {
            targets.Add(data.targetTransform);
        }

        // Rotate targets: 1->2, 2->3, 3->1
        Transform firstTarget = targets[0];
        for (int i = 0; i < targets.Count - 1; i++)
        {
            probeDataList[i].targetTransform = targets[i + 1];
        }
        probeDataList[targets.Count - 1].targetTransform = firstTarget;

        Debug.Log($"Probes rotated positions! (Swap {currentSwapCount + 1}/{swapsPerCycle})");
    }
}
