using UnityEngine;

public class GhostFloat : MonoBehaviour
{
    public float amplitude = 0.015f;
    public float speed = 1.2f;
    Vector3 startPos;

    void Start() => startPos = transform.localPosition;

    void Update()
    {
        transform.localPosition = startPos + Vector3.up * Mathf.Sin(Time.time * speed) * amplitude;
    }
}