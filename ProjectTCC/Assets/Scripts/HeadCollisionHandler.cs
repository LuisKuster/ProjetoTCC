using System.Collections.Generic;
using UnityEngine;

public class HeadCollisionHandler : MonoBehaviour
{
    [SerializeField]
    private HeadCollisionDetector _detector;

    [SerializeField]
    private CharacterController _characterController;

    [SerializeField]
    private float _pushBackStrength = 1.0f;

    private Vector3 CalculatePushBackDirection(List<RaycastHit> colliderHits)
    {
        Vector3 combinedNormal = Vector3.zero;
        foreach (RaycastHit hit in colliderHits)
        {
            // zera o Y pra empurrar só na horizontal (não joga o player pra cima/baixo)
            combinedNormal += new Vector3(hit.normal.x, 0f, hit.normal.z);
        }
        return combinedNormal;
    }

    private void Update()
    {
        if (_detector == null || _detector.DetectedColliderHits.Count <= 0)
            return;

        Vector3 pushBackDirection = CalculatePushBackDirection(_detector.DetectedColliderHits);

        if (pushBackDirection == Vector3.zero)
            return;

        _characterController.Move(pushBackDirection.normalized * _pushBackStrength * Time.deltaTime);
    }
}