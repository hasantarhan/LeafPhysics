using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace _Game.Code
{
    public class GPUInstancing : MonoBehaviour
    {
        [SerializeField] private int instanceCount;
        [SerializeField] private float force;
        [SerializeField] [Range(0,1)] private float upForce=0.4f;
        [SerializeField] private float radius;
        [SerializeField] private float spawnHeight;
        [SerializeField] private float groundHeight;
        [SerializeField] private Vector2 positionRange;
        [SerializeField] private Vector2 scaleRange;
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material material;
        [SerializeField] private Transform head;
        private Vector3[][] velocities;
        private Matrix4x4[][] matrices;
        private VelocityUtil velocityUtil;

        void Start()
        {
            velocityUtil = new VelocityUtil(head);
            velocities = new Vector3[instanceCount / 1023 + 1][];
            matrices = new Matrix4x4[instanceCount / 1023 + 1][];

            for (int i = 0; i < instanceCount / 1023 + 1; i++)
            {
                matrices[i] = new Matrix4x4[1023];
                velocities[i] = new Vector3[1023];
                for (int j = 0; j < 1023; j++)
                {
                    var newPos = new Vector3(Random.Range(-positionRange.x, positionRange.x), spawnHeight,
                        Random.Range(-positionRange.y, positionRange.y));
                    var randomRotate = new Vector3(0, Random.Range(-360, 360), 0);
                    var scale = Random.Range(scaleRange.x, scaleRange.y);
                    var randomScale = Vector3.one * scale;
                    matrices[i][j] = Matrix4x4.TRS(newPos, Quaternion.Euler(randomRotate), randomScale);
                    velocities[i][j] = Vector3.zero;
                }
            }
        }

        private void Update()
        {
            velocityUtil.Update();
            CalculateMatrices();
            Draw();
        }

        private void CalculateMatrices()
        {
            for (int i = 0; i < instanceCount / 1023 + 1; i++)
            {
                for (int j = 0; j < 1023; j++)
                {
                    Vector3 pos;
                    Quaternion rot;
                    Vector3 scale;
                    var gravity = new Vector3(0, -9.81f, 0);
                    matrices[i][j].Decompose(out pos, out rot, out scale);
                    velocities[i][j] -= gravity * Time.deltaTime;
                    velocities[i][j] -= (velocities[i][j]) * (Time.deltaTime);
                    var dist = Vector3.Distance(head.position, pos);
                    if (dist < radius)
                    {
                        var t = 1 - dist / radius;
                        var dir = head.position - pos;
                        dir.y -= upForce;
                        if (velocityUtil.speed > 0.5f)
                        {
                            velocities[i][j] += (Time.deltaTime * force * dir * t) * Mathf.Clamp(velocityUtil.speed, 0, 1);
                        }
                    }

                    if (velocities[i][j].magnitude > 1)
                    {
                        rot = Quaternion.Slerp(rot, Random.rotation, 0.2f);
                    }
                    if (pos.y < groundHeight)
                    {
                        pos.y = groundHeight;
                        gravity.y = 0;
                        velocities[i][j] = Vector3.MoveTowards(velocities[i][j], Vector3.zero, 0.2f);
                    }
                    pos -= velocities[i][j] * Time.deltaTime;
                    matrices[i][j].SetTRS(pos, rot, scale);
                }
            }
        }

        private void Draw()
        {
            foreach (Matrix4x4[] batch in matrices)
            {
                Graphics.DrawMeshInstanced(mesh, 0, material, batch, 1023, new MaterialPropertyBlock(),
                    ShadowCastingMode.On);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(head.position, 0.5f);
        }
    }
}