using System.Collections.Generic;
using UnityEngine;

public class HeadCollisionDetector : MonoBehaviour
{
    [SerializeField, Range(0f, 0.5f)]
    private float _detectionDelay = 0.05f;

    [SerializeField]
    private float _detectionDistance = 0.2f;

    [SerializeField]
    private LayerMask _detectionLayers;

    public List<RaycastHit> DetectedColliderHits { get; private set; } = new();

    private float _currentTime = 0f;

    private List<RaycastHit> PerformDetection(Vector3 position, float distance, LayerMask mask)
    {
        List<RaycastHit> detectedHits = new();

        // 4 direções agora (incluindo -forward) -> não atravessa mais andando de ré
        List<Vector3> directions = new()
        {
            transform.forward,
            -transform.forward,
            transform.right,
            -transform.right
        };

        foreach (var dir in directions)
        {
            if (Physics.Raycast(position, dir, out RaycastHit hit, distance, mask))
            {
                detectedHits.Add(hit);
            }
        }

        return detectedHits;
    }

    private void Start()
    {
        DetectedColliderHits = PerformDetection(transform.position, _detectionDistance, _detectionLayers);
    }

    private void Update()
    {
        _currentTime += Time.deltaTime;
        if (_currentTime > _detectionDelay)
        {
            _currentTime = 0f;
            DetectedColliderHits = PerformDetection(transform.position, _detectionDistance, _detectionLayers);
        }
    }
}