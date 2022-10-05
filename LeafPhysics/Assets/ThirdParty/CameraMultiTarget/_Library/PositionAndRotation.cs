using UnityEngine;

class PositionAndRotation
{
    public Vector3 Position { get; private set; }
    public Quaternion Rotation { get; private set; }

    public PositionAndRotation(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }
}