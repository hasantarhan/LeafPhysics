using UnityEngine;

public class VelocityUtil
{
    private readonly Transform _transform;
    private Vector3 _lastPosition;
    public Vector3 Motion { get; private set; }
    public float Speed { get; private set; }

    public VelocityUtil(Transform transform)
    {
        _transform = transform;
    }

    public void Update()
    {
        var currentPosition = _transform.position;
        Motion = (currentPosition - _lastPosition) / Time.deltaTime;
        Speed = Motion.magnitude;
        _lastPosition = currentPosition;
    }
}
