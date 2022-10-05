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
        public int instanceCount;
        public float force;
        public float radius;
        public float spawnHeight;
        public Vector2 positionRange;
        public Vector2 scaleRange;
        [Space] public Mesh mesh;
        public Material material;
        public Transform head;
        public Vector3[][] velocities;
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
                    var newPos = new Vector3(Random.Range(-positionRange.x, positionRange.x),
                        spawnHeight, Random.Range(-positionRange.y, positionRange.y));
                    var randomRotate = new Vector3(0, Random.Range(-360, 360), 0);
                    var scale = Random.Range(scaleRange.x, scaleRange.y);
                    var randomScale = Vector3.one * scale;
                    matrices[i][j] = Matrix4x4.TRS(newPos, Quaternion.Euler(randomRotate), randomScale);
                    velocities[i][j] = Vector3.zero;
                }
            }
        }


        void Update()
        {
            velocityUtil.Update();
            for (int i = 0; i < instanceCount / 1023 + 1; i++)
            {
                for (int j = 0; j < 1023; j++)
                {
                    Vector3 pos;
                    Quaternion rot;
                    Vector3 scale;
                    var gravity = new Vector3(0, 0, 0);
                    gravity.y = -9;
                    matrices[i][j].Decompose(out pos, out rot, out scale);
                    velocities[i][j] -= gravity * Time.deltaTime;
                    if (pos.y <= 0.2f)
                    {
                        pos.y = 0;
                        gravity.y = 0;
                        velocities[i][j] = Vector3.MoveTowards(velocities[i][j], Vector3.zero, 0.3f);
                    }

                    velocities[i][j] -= (velocities[i][j]) * (Time.deltaTime);
                    var dist = Vector3.Distance(head.position, pos);
                    if (dist < radius)
                    {
                        var t = 1 - dist / radius;
                        var dir = head.position - pos;
                        dir.y -= 0.6f;
                        if (velocityUtil.speed > 0.5f)
                        {
                            velocities[i][j] -= (Time.deltaTime * force * dir * t) *
                                                Mathf.Clamp(velocityUtil.speed, 0, 1);
                        }
                    }

                    if (velocities[i][j].magnitude > 1)
                    {
                        rot = Quaternion.Slerp(rot, Random.rotation, 0.2f);
                    }

                    pos -= velocities[i][j] * Time.deltaTime;
                    matrices[i][j].SetTRS(pos, rot, scale);
                }
            }

            Draw();
        }

        public void Draw()
        {
            foreach (Matrix4x4[] batch in matrices)
            {
                var materialblock = new MaterialPropertyBlock();
                Graphics.DrawMeshInstanced(mesh, 0, material, batch, 1023, new MaterialPropertyBlock(),
                    ShadowCastingMode.On);
            }
        }
    }

    [BurstCompile]
    public struct CalculateVelocities : IJob
    {
        public float velocity;
        public int radius;
        public float force;
        public Vector3 headPosition;
        public int instanceCount;
        public Vector3[][] velocities;
        private Matrix4x4[][] matrices;

        public void Execute()
        {
            for (int i = 0; i < instanceCount / 1023 + 1; i++)
            {
                for (int j = 0; j < 1023; j++)
                {
                    Vector3 pos;
                    Quaternion rot;
                    Vector3 scale;
                    var gravity = new Vector3(0, 0, 0);
                    gravity.y = -9;
                    matrices[i][j].Decompose(out pos, out rot, out scale);
                    velocities[i][j] -= gravity * Time.deltaTime;
                    if (pos.y <= 0.2f)
                    {
                        pos.y = 0;
                        gravity.y = 0;
                        velocities[i][j] = Vector3.MoveTowards(velocities[i][j], Vector3.zero, 0.3f);
                    }
                    
                    velocities[i][j] -= (velocities[i][j]) * (Time.deltaTime);
                    var dist = Vector3.Distance(headPosition, pos);
                    if (dist < radius)
                    {
                        var t = 1 - dist / radius;
                        var dir = headPosition - pos;
                        dir.y -= 0.6f;
                        if (velocity > 0.5f)
                        {
                            velocities[i][j] -= (Time.deltaTime * force * dir * t) *
                                                Mathf.Clamp(velocity, 0, 1);
                        }
                    }

                    if (velocities[i][j].magnitude > 1)
                    {
                        rot = Quaternion.Slerp(rot, Random.rotation, 0.2f);
                    }

                    pos -= velocities[i][j] * Time.deltaTime;
                    matrices[i][j].SetTRS(pos, rot, scale);
                }
            }
        }
    }
}