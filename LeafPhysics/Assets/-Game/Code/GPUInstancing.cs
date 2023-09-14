using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace _Game.Code
{
    public class GPUInstancing : MonoBehaviour
    {
        [SerializeField] private bool useSingleMatrix = true;
        [SerializeField] private bool useJobs;
        [SerializeField] private int instanceCount;
        [SerializeField] private float force;
        [SerializeField] private Vector3 gravity;
        [SerializeField] [Range(0, 1)] private float upForce = 0.4f;
        [SerializeField] [Range(0.1f, 1)] private float friction;
        [SerializeField] private float radius;
        [SerializeField] private float spawnHeight;
        [SerializeField] private float groundHeight;
        [SerializeField] private Vector2 positionRange;
        [SerializeField] private Vector2 scaleRange;
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material material;
        [SerializeField] private Transform head;

        private Matrix4x4[] allMatrices;
        private Vector3[] allVelocities;

        private Vector3[][] velocities;
        private Matrix4x4[][] matrices;
        private VelocityUtil velocityUtil;

        private MaterialPropertyBlock mpb;

        private CustomSampler jobsSampler;
        private CustomSampler sampler;

        void Start()
        {
            sampler = CustomSampler.Create("Leafs");
            jobsSampler = CustomSampler.Create("Leafs.Jobs");

            mpb = new MaterialPropertyBlock();

            velocityUtil = new VelocityUtil(head);

            if (useSingleMatrix)
            {
                allMatrices = new Matrix4x4[instanceCount];
                allVelocities = new Vector3[instanceCount];

                for (int i = 0; i < instanceCount; i++)
                {
                    var newPos = new Vector3(Random.Range(-positionRange.x, positionRange.x), spawnHeight,
                        Random.Range(-positionRange.y, positionRange.y));
                    var randomRotate = new Vector3(0, Random.Range(-360, 360), 0);
                    var scale = Random.Range(scaleRange.x, scaleRange.y);
                    var randomScale = Vector3.one * scale;
                    allMatrices[i] = Matrix4x4.TRS(newPos, Quaternion.Euler(randomRotate), randomScale);
                    allVelocities[i] = Vector3.zero;
                }
            }
            else
            {
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
        }


        private void Update()
        {
            if (!useJobs)
            {
                sampler.Begin();

                velocityUtil.Update();
                CalculateMatrices();

                sampler.End();
            }
            else
            {
                jobsSampler.Begin();

                velocityUtil.Update();
                CalcuateMatricesJobs();

                jobsSampler.End();
            }

            Draw();
        }

        private void CalcuateMatricesJobs()
        {
            if (useSingleMatrix)
            {
                var matrices = new NativeArray<Matrix4x4>(allMatrices, Allocator.TempJob);
                var velocities = new NativeArray<Vector3>(allVelocities, Allocator.TempJob);

                var job = new PhysicsJob()
                {
                    deltaTime = Time.deltaTime,
                    force = force,
                    friction = friction,
                    groundHeight = groundHeight,
                    headPosition = head.position,
                    radius = radius,
                    speed = velocityUtil.speed,
                    upForce = upForce,
                    baseSeed = 1,
                    velocities = velocities,
                    matrices = matrices,
                    gravity = gravity
                }.Schedule(matrices.Length, 64);

                job.Complete();

                matrices.CopyTo(allMatrices);
                velocities.CopyTo(allVelocities);

                matrices.Dispose();
                velocities.Dispose();
            }
            else
            {
                for (int i = 0; i < instanceCount / 1023 + 1; i++)
                {
                    var matricesChunk = this.matrices[i];
                    var velocitiesChunk = this.velocities[i];

                    var matrices = new NativeArray<Matrix4x4>(matricesChunk, Allocator.TempJob);
                    var velocities = new NativeArray<Vector3>(velocitiesChunk, Allocator.TempJob);

                    var job = new PhysicsJob()
                    {
                        deltaTime = Time.deltaTime,
                        force = force,
                        friction = friction,
                        groundHeight = groundHeight,
                        headPosition = head.position,
                        radius = radius,
                        speed = velocityUtil.speed,
                        upForce = upForce,
                        baseSeed = i + 1,
                        velocities = velocities,
                        matrices = matrices,
                        gravity = gravity
                    }.Schedule(matrices.Length, 64);

                    job.Complete();

                    NativeArray<Matrix4x4>.Copy(matrices, this.matrices[i]);
                    NativeArray<Vector3>.Copy(velocities, this.velocities[i]);

                    matrices.Dispose();
                    velocities.Dispose();
                }
            }
        }


        private void CalculateMatrices()
        {
            if (useSingleMatrix)
            {
                for (int i = 0; i < allMatrices.Length; i++)
                {
                    Vector3 pos;
                    Quaternion rot;
                    Vector3 scale;
                    allMatrices[i].Decompose(out pos, out rot, out scale);

                    allVelocities[i] -= gravity * Time.deltaTime;
                    allVelocities[i] *= (1 - Time.deltaTime);

                    float dist = Vector3.Distance(head.position, pos);
                    if (dist < radius)
                    {
                        float t = 1 - dist / radius;
                        Vector3 dir = head.position - pos;
                        dir.y -= upForce;
                        if (velocityUtil.speed > 0.5f)
                        {
                            allVelocities[i] += dir * (Time.deltaTime * force * t * Mathf.Clamp01(velocityUtil.speed));
                        }
                    }

                    if (allVelocities[i].magnitude > 5)
                    {
                        rot = Quaternion.Slerp(rot, Random.rotation, 0.2f);
                    }

                    if (pos.y < groundHeight)
                    {
                        pos.y = groundHeight;
                        allVelocities[i] = Vector3.MoveTowards(allVelocities[i], Vector3.zero, friction);
                    }

                    pos -= allVelocities[i] * Time.deltaTime;
                    allMatrices[i].SetTRS(pos, rot, scale);
                }
            }
            else
            {
                for (int i = 0; i < instanceCount / 1023 + 1; i++)
                {
                    for (int j = 0; j < 1023; j++)
                    {
                        Vector3 pos;
                        Quaternion rot;
                        Vector3 scale;
                        matrices[i][j].Decompose(out pos, out rot, out scale);

                        velocities[i][j] -= gravity * Time.deltaTime;
                        velocities[i][j] *= (1 - Time.deltaTime);

                        float dist = Vector3.Distance(head.position, pos);
                        if (dist < radius)
                        {
                            float t = 1 - dist / radius;
                            Vector3 dir = head.position - pos;
                            dir.y -= upForce;
                            if (velocityUtil.speed > 0.5f)
                            {
                                velocities[i][j] +=
                                    dir * (Time.deltaTime * force * t * Mathf.Clamp01(velocityUtil.speed));
                            }
                        }

                        if (velocities[i][j].magnitude > 5)
                        {
                            rot = Quaternion.Slerp(rot, Random.rotation, 0.2f);
                        }

                        if (pos.y < groundHeight)
                        {
                            pos.y = groundHeight;
                            velocities[i][j] = Vector3.MoveTowards(velocities[i][j], Vector3.zero, friction);
                        }

                        pos -= velocities[i][j] * Time.deltaTime;
                        matrices[i][j].SetTRS(pos, rot, scale);
                    }
                }
            }
        }


        private void Draw()
        {
            if (useSingleMatrix)
            {
                Graphics.DrawMeshInstanced(mesh, 0, material, allMatrices, allMatrices.Length, mpb,
                    ShadowCastingMode.On);
            }
            else
            {
                foreach (Matrix4x4[] batch in matrices)
                {
                    Graphics.DrawMeshInstanced(mesh, 0, material, batch, 1023, mpb, ShadowCastingMode.On);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(head.position, 0.5f);
        }

        [BurstCompile]
        public struct PhysicsJob : IJobParallelFor
        {
            [ReadOnly] public float force;
            [ReadOnly] public float upForce;
            [ReadOnly] public float radius;
            [ReadOnly] public float speed;
            [ReadOnly] public float deltaTime;
            [ReadOnly] public float groundHeight;
            [ReadOnly] public float friction;

            [ReadOnly] public Vector3 headPosition;
            [ReadOnly] public Vector3 gravity;

            [ReadOnly] public int baseSeed;
            [ReadOnly] public bool useSingleMatrix;

            public NativeArray<Matrix4x4> matrices;
            public NativeArray<Vector3> velocities;

            public void Execute(int index)
            {
                if (useSingleMatrix && index >= matrices.Length) return;

                var seed = baseSeed + index;
                var rnd = new Unity.Mathematics.Random((uint)seed);

                matrices[index].Decompose(out Vector3 pos, out Quaternion rot, out Vector3 scale);
                velocities[index] -= gravity * deltaTime;
                velocities[index] -= (velocities[index]) * deltaTime;
                var dist = Vector3.Distance(headPosition, pos);

                if (dist < radius)
                {
                    var t = 1 - dist / radius;
                    var dir = headPosition - pos;
                    dir.y -= upForce;
                    if (speed > 0.5f)
                    {
                        velocities[index] += dir * (deltaTime * force * t * Mathf.Clamp01(speed));
                    }
                }

                if (velocities[index].magnitude > 1f)
                {
                    quaternion q = new quaternion(rnd.NextFloat(-1, 1), rnd.NextFloat(-1, 1), rnd.NextFloat(-1, 1), 0);
                    q = quaternion.Euler(rnd.NextFloat3(-360, 360));
                    rot = math.slerp(rot, q, 0.75f);
                }

                if (pos.y < groundHeight)
                {
                    pos.y = groundHeight;
                    velocities[index] = Vector3.MoveTowards(velocities[index], Vector3.zero, friction);
                }

                velocities[index] += deltaTime * Vector3.one;
                pos -= velocities[index] * deltaTime;

                matrices[index] = float4x4.TRS(pos, rot, scale);
            }
        }
    }
}