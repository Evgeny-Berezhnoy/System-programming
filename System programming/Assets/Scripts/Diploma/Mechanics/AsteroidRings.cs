using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

using float4x4 = Unity.Mathematics.float4x4;
using quaternion = Unity.Mathematics.quaternion;
using Random = UnityEngine.Random;

namespace Mechanics
{
    public class AsteroidRings : MonoBehaviour
    {
        #region Static fields

        private static readonly int _matricesID = Shader.PropertyToID("_Matrices");
        private static MaterialPropertyBlock _propertyBlock;

        #endregion

        #region Fields

        [SerializeField] private Mesh[] _meshes;
        [SerializeField] private RingData[] _rings;
        [SerializeField] private Material _material;

        private List<AsteroidGroup> _asteroids;
        private Vector3 _boundsSize;
        private Coroutine _rotationCoroutine;

        #endregion

        #region Unity events

        private void OnEnable()
        {
            _asteroids = new List<AsteroidGroup>();

            var stride      = 16 * 4;
            var maxRadius   = 0f;

            for(int i = 0; i < _rings.Length; i++)
            {
                var ring = _rings[i];

                var meshDictionary = new Dictionary<Mesh, List<AsteroidData>>();

                for (int j = 0; j < _meshes.Length; j++)
                {
                    var mesh = _meshes[j];

                    if (meshDictionary.ContainsKey(mesh)) continue;

                    meshDictionary.Add(mesh, new List<AsteroidData>());
                };
                
                for (int j = 0; j < ring.Amount; j++)
                {
                    var asteroidData = new AsteroidData();

                    var angle = 2 * PI * j / ring.Amount;

                    asteroidData.Direction      = new Vector3(ring.Radius * cos(angle), 0, ring.Radius * sin(angle)) + Random.insideUnitSphere * ring.Offset;
                    asteroidData.Rotation       = mul(quaternion.identity, quaternion.RotateY(angle));
                    asteroidData.RotationSpeed  = ring.RotationSpeed;

                    var mesh = _meshes.Random();

                    meshDictionary[mesh].Add(asteroidData);
                };

                foreach (var kvp in meshDictionary)
                {
                    if (kvp.Value.Count == 0) continue;

                    var asteroidGroup = new AsteroidGroup();

                    asteroidGroup.Mesh              = kvp.Key;
                    asteroidGroup.Elements          = new NativeArray<AsteroidData>(kvp.Value.Count, Allocator.Persistent);
                    asteroidGroup.Matrices          = new NativeArray<Matrix4x4>(kvp.Value.Count, Allocator.Persistent);
                    asteroidGroup.MatricesBuffer    = new ComputeBuffer(kvp.Value.Count, stride);
                    asteroidGroup.RotationSpeed     = ring.RotationSpeed;

                    for (int j = 0; j < kvp.Value.Count; j++)
                    {
                        asteroidGroup.Elements[j] = kvp.Value[j];
                    };

                    _asteroids.Add(asteroidGroup);
                };
                
                if (maxRadius < ring.Radius)
                {
                    maxRadius = ring.Radius;
                };
            };

            _propertyBlock ??= new MaterialPropertyBlock();

            _boundsSize = new float3(maxRadius * 2);

            _rotationCoroutine = StartCoroutine(RotationCoroutine());
        }

        private void OnDisable()
        {
            StopCoroutine(_rotationCoroutine);

            for (int i = 0; i < _asteroids.Count; i++)
            {
                var asteroidGroup = _asteroids[i];

                asteroidGroup.Elements.Dispose();
                asteroidGroup.Matrices.Dispose();
                asteroidGroup.MatricesBuffer.Release();
            };

            _asteroids = null;
        }

        private void OnValidate()
        {
            if (!enabled || _asteroids is null) return;

            OnDisable();
            OnEnable();
        }

        #endregion

        #region Methods

        private IEnumerator RotationCoroutine()
        {
            while (true)
            {
                yield return null;

                JobHandle jobHandle = default;

                for (var i = 1; i < _asteroids.Count; i++)
                {
                    var asteroidGroup = _asteroids[i];

                    asteroidGroup.SpinAngle += mul(_asteroids[i].RotationSpeed, mul(PI, Time.deltaTime));
                    asteroidGroup.WorldRotation = mul(quaternion.identity, quaternion.RotateY(asteroidGroup.SpinAngle));

                    _asteroids[i] = asteroidGroup;

                    var job = new AsteroidRotationJob
                    {
                        ElapsedTime = Time.deltaTime,
                        ParentPosition = transform.position,
                        ParentRotation = _asteroids[i].WorldRotation,
                        Elements = _asteroids[i].Elements,
                        Matrices = _asteroids[i].Matrices
                    };

                    jobHandle = job.Schedule(_asteroids[i].Elements.Length, 0, jobHandle);
                };

                jobHandle.Complete();

                var bounds = new Bounds(transform.position, _boundsSize);

                for (var i = 0; i < _asteroids.Count; i++)
                {
                    var asteroidGroup = _asteroids[i];

                    var buffer = asteroidGroup.MatricesBuffer;

                    buffer.SetData(asteroidGroup.Matrices);

                    _propertyBlock.SetBuffer(_matricesID, buffer);

                    _material.SetBuffer(_matricesID, buffer);

                    Graphics
                        .DrawMeshInstancedProcedural(
                            asteroidGroup.Mesh,
                            0,
                            _material,
                            bounds,
                            buffer.count,
                            _propertyBlock);
                };
            };
        }

        #endregion

        #region Nested structs

        [Serializable]
        private struct RingData
        {
            public float Radius;
            public int Amount;
            public float RotationSpeed;
            public float Offset;
        }

        private struct AsteroidGroup
        {
            public float SpinAngle;
            public float RotationSpeed;
            public Quaternion WorldRotation;
            public Mesh Mesh;
            public NativeArray<AsteroidData> Elements;
            public NativeArray<Matrix4x4> Matrices;
            public ComputeBuffer MatricesBuffer;
        }

        private struct AsteroidData
        {
            public Vector3 Direction;
            public Quaternion Rotation;
            public Vector3 WorldPosition;
            public Quaternion WorldRotation;
            public float RotationSpeed;
            public float SpinAngle;
        }

        [BurstCompile(FloatPrecision = FloatPrecision.Standard, FloatMode = FloatMode.Fast)]
        private struct AsteroidRotationJob : IJobParallelFor
        {
            #region Fields

            public float ElapsedTime;

            public Vector3 ParentPosition;
            public Quaternion ParentRotation;
            
            public NativeArray<AsteroidData> Elements;

            [WriteOnly] public NativeArray<Matrix4x4> Matrices;

            #endregion

            #region Base methods

            public void Execute(int index)
            {
                var asteroid = Elements[index];

                asteroid.SpinAngle      += mul(asteroid.RotationSpeed, mul(PI, ElapsedTime));
                asteroid.WorldRotation  = mul(ParentRotation, mul(asteroid.Rotation, quaternion.RotateY(asteroid.SpinAngle)));
                asteroid.WorldPosition  = ParentRotation * asteroid.Direction + ParentPosition;

                Elements[index] = asteroid;

                Matrices[index] =
                    float4x4.TRS(
                        asteroid.WorldPosition,
                        asteroid.WorldRotation,
                        float3(1));
            }

            #endregion
        }

        #endregion
    }
}
