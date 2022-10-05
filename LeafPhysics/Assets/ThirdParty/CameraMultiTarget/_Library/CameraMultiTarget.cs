using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class CameraMultiTarget : MonoBehaviour
{
    public static CameraMultiTarget I;
    public float Pitch;
    public float Yaw;
    public float Roll;
    public float PaddingLeft;
    public float PaddingRight;
    public float PaddingUp;
    public float PaddingDown;
    public float MoveSmoothTime = 0.19f;

    public Camera _camera;
    public GameObject[] _targets = new GameObject[0];
    private DebugProjection _debugProjection;

    enum DebugProjection
    {
        DISABLE,
        IDENTITY,
        ROTATED
    }

    enum ProjectionEdgeHits
    {
        TOP_BOTTOM,
        LEFT_RIGHT
    }

    public void SetTargets(List<GameObject> targets)
    {
        _targets = targets.ToArray();
    }

    public void SetTargets(GameObject[] targets)
    {
        _targets = targets;
    }

    public void SetTargets(GameObject[] targets, float _pitch)
    {
        Pitch = _pitch;
        _targets = targets;
    }

    private void Awake()
    {
        I = this;
        _camera = gameObject.GetComponent<Camera>();
        _debugProjection = DebugProjection.ROTATED;
    }

    private void LateUpdate()
    {
        if (_targets.Length == 0)
            return;

        var targetPositionAndRotation = TargetPositionAndRotation(_targets);

        Vector3 velocity = Vector3.zero;
        float velocityYaw = 0;
        transform.position = Vector3.SmoothDamp(transform.position, targetPositionAndRotation.Position, ref velocity,
            MoveSmoothTime);
        transform.rotation = targetPositionAndRotation.Rotation;
    }

    PositionAndRotation TargetPositionAndRotation(GameObject[] targets)
    {
        float halfVerticalFovRad = (_camera.fieldOfView * Mathf.Deg2Rad) / 2f;
        float halfHorizontalFovRad = Mathf.Atan(Mathf.Tan(halfVerticalFovRad) * _camera.aspect);

        var rotation = Quaternion.Euler(Pitch, Yaw, Roll);
        var inverseRotation = Quaternion.Inverse(rotation);

        var targetsRotatedToCameraIdentity =
            targets.Select(target => inverseRotation * target.transform.position).ToArray();

        float furthestPointDistanceFromCamera = targetsRotatedToCameraIdentity.Max(target => target.z);
        float projectionPlaneZ = furthestPointDistanceFromCamera + 3f;

        ProjectionHits viewProjectionLeftAndRightEdgeHits =
            ViewProjectionEdgeHits(targetsRotatedToCameraIdentity, ProjectionEdgeHits.LEFT_RIGHT, projectionPlaneZ,
                halfHorizontalFovRad).AddPadding(PaddingRight, PaddingLeft);
        ProjectionHits viewProjectionTopAndBottomEdgeHits =
            ViewProjectionEdgeHits(targetsRotatedToCameraIdentity, ProjectionEdgeHits.TOP_BOTTOM, projectionPlaneZ,
                halfVerticalFovRad).AddPadding(PaddingUp, PaddingDown);

        var requiredCameraPerpedicularDistanceFromProjectionPlane =
            Mathf.Max(
                RequiredCameraPerpedicularDistanceFromProjectionPlane(viewProjectionTopAndBottomEdgeHits,
                    halfVerticalFovRad),
                RequiredCameraPerpedicularDistanceFromProjectionPlane(viewProjectionLeftAndRightEdgeHits,
                    halfHorizontalFovRad)
            );

        Vector3 cameraPositionIdentity = new Vector3(
            (viewProjectionLeftAndRightEdgeHits.Max + viewProjectionLeftAndRightEdgeHits.Min) / 2f,
            (viewProjectionTopAndBottomEdgeHits.Max + viewProjectionTopAndBottomEdgeHits.Min) / 2f,
            projectionPlaneZ - requiredCameraPerpedicularDistanceFromProjectionPlane);

        DebugDrawProjectionRays(cameraPositionIdentity,
            viewProjectionLeftAndRightEdgeHits,
            viewProjectionTopAndBottomEdgeHits,
            requiredCameraPerpedicularDistanceFromProjectionPlane,
            targetsRotatedToCameraIdentity,
            projectionPlaneZ,
            halfHorizontalFovRad,
            halfVerticalFovRad);

        return new PositionAndRotation(rotation * cameraPositionIdentity, rotation);
    }

    private static float RequiredCameraPerpedicularDistanceFromProjectionPlane(ProjectionHits viewProjectionEdgeHits,
        float halfFovRad)
    {
        float distanceBetweenEdgeProjectionHits = viewProjectionEdgeHits.Max - viewProjectionEdgeHits.Min;
        return (distanceBetweenEdgeProjectionHits / 2f) / Mathf.Tan(halfFovRad);
    }

    private ProjectionHits ViewProjectionEdgeHits(IEnumerable<Vector3> targetsRotatedToCameraIdentity,
        ProjectionEdgeHits alongAxis, float projectionPlaneZ, float halfFovRad)
    {
        float[] projectionHits = targetsRotatedToCameraIdentity
            .SelectMany(target => TargetProjectionHits(target, alongAxis, projectionPlaneZ, halfFovRad))
            .ToArray();
        return new ProjectionHits(projectionHits.Max(), projectionHits.Min());
    }

    private float[] TargetProjectionHits(Vector3 target, ProjectionEdgeHits alongAxis, float projectionPlaneDistance,
        float halfFovRad)
    {
        float distanceFromProjectionPlane = projectionPlaneDistance - target.z;
        float projectionHalfSpan = Mathf.Tan(halfFovRad) * distanceFromProjectionPlane;

        if (alongAxis == ProjectionEdgeHits.LEFT_RIGHT)
        {
            return new[] {target.x + projectionHalfSpan, target.x - projectionHalfSpan};
        }
        else
        {
            return new[] {target.y + projectionHalfSpan, target.y - projectionHalfSpan};
        }
    }

    private void DebugDrawProjectionRays(Vector3 cameraPositionIdentity,
        ProjectionHits viewProjectionLeftAndRightEdgeHits,
        ProjectionHits viewProjectionTopAndBottomEdgeHits, float requiredCameraPerpedicularDistanceFromProjectionPlane,
        IEnumerable<Vector3> targetsRotatedToCameraIdentity, float projectionPlaneZ, float halfHorizontalFovRad,
        float halfVerticalFovRad)
    {
        if (_debugProjection == DebugProjection.DISABLE)
            return;

        DebugDrawProjectionRay(
            cameraPositionIdentity,
            new Vector3((viewProjectionLeftAndRightEdgeHits.Max - viewProjectionLeftAndRightEdgeHits.Min) / 2f,
                (viewProjectionTopAndBottomEdgeHits.Max - viewProjectionTopAndBottomEdgeHits.Min) / 2f,
                requiredCameraPerpedicularDistanceFromProjectionPlane), new Color32(31, 119, 180, 255));
        DebugDrawProjectionRay(
            cameraPositionIdentity,
            new Vector3((viewProjectionLeftAndRightEdgeHits.Max - viewProjectionLeftAndRightEdgeHits.Min) / 2f,
                -(viewProjectionTopAndBottomEdgeHits.Max - viewProjectionTopAndBottomEdgeHits.Min) / 2f,
                requiredCameraPerpedicularDistanceFromProjectionPlane), new Color32(31, 119, 180, 255));
        DebugDrawProjectionRay(
            cameraPositionIdentity,
            new Vector3(-(viewProjectionLeftAndRightEdgeHits.Max - viewProjectionLeftAndRightEdgeHits.Min) / 2f,
                (viewProjectionTopAndBottomEdgeHits.Max - viewProjectionTopAndBottomEdgeHits.Min) / 2f,
                requiredCameraPerpedicularDistanceFromProjectionPlane), new Color32(31, 119, 180, 255));
        DebugDrawProjectionRay(
            cameraPositionIdentity,
            new Vector3(-(viewProjectionLeftAndRightEdgeHits.Max - viewProjectionLeftAndRightEdgeHits.Min) / 2f,
                -(viewProjectionTopAndBottomEdgeHits.Max - viewProjectionTopAndBottomEdgeHits.Min) / 2f,
                requiredCameraPerpedicularDistanceFromProjectionPlane), new Color32(31, 119, 180, 255));

        foreach (var target in targetsRotatedToCameraIdentity)
        {
            float distanceFromProjectionPlane = projectionPlaneZ - target.z;
            float halfHorizontalProjectionVolumeCircumcircleDiameter =
                Mathf.Sin(Mathf.PI - ((Mathf.PI / 2f) + halfHorizontalFovRad)) / (distanceFromProjectionPlane);
            float projectionHalfHorizontalSpan =
                Mathf.Sin(halfHorizontalFovRad) / halfHorizontalProjectionVolumeCircumcircleDiameter;
            float halfVerticalProjectionVolumeCircumcircleDiameter =
                Mathf.Sin(Mathf.PI - ((Mathf.PI / 2f) + halfVerticalFovRad)) / (distanceFromProjectionPlane);
            float projectionHalfVerticalSpan =
                Mathf.Sin(halfVerticalFovRad) / halfVerticalProjectionVolumeCircumcircleDiameter;

            DebugDrawProjectionRay(target,
                new Vector3(projectionHalfHorizontalSpan, 0f, distanceFromProjectionPlane),
                new Color32(214, 39, 40, 255));
            DebugDrawProjectionRay(target,
                new Vector3(-projectionHalfHorizontalSpan, 0f, distanceFromProjectionPlane),
                new Color32(214, 39, 40, 255));
            DebugDrawProjectionRay(target,
                new Vector3(0f, projectionHalfVerticalSpan, distanceFromProjectionPlane),
                new Color32(214, 39, 40, 255));
            DebugDrawProjectionRay(target,
                new Vector3(0f, -projectionHalfVerticalSpan, distanceFromProjectionPlane),
                new Color32(214, 39, 40, 255));
        }
    }

    private void DebugDrawProjectionRay(Vector3 start, Vector3 direction, Color color)
    {
        Quaternion rotation = _debugProjection == DebugProjection.IDENTITY ? Quaternion.identity : transform.rotation;
        Debug.DrawRay(rotation * start, rotation * direction, color);
    }
}