using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

using float4x4 = Unity.Mathematics.float4x4;
using quaternion = Unity.Mathematics.quaternion;

public class FractalJobs : MonoBehaviour
{
    #region Constants

    private const float _positionOffset = 1.5f;
    private const float _scaleBias = 0.5f;
    private const int _childCount = 5;

    #endregion

    #region Static fields

    private static readonly float3[] _directions = new float3[]
    {
        up(),
        left(),
        right(),
        forward(),
        back()
    };

    private static quaternion[] _rotations = new quaternion[]
    {
        quaternion.identity,
        quaternion.RotateZ(0.5f * PI),
        quaternion.RotateZ(-0.5f * PI),
        quaternion.RotateX(0.5f * PI),
        quaternion.RotateX(-0.5f * PI)
    };

    private static readonly int _matricesID = Shader.PropertyToID("_Matrices");
    private static MaterialPropertyBlock _propertyBlock;
    
    #endregion

    #region Fields

    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    [SerializeField, Range(1, 8)] private int _depth = 4;
    [SerializeField, Range(0, 1)] private float _rotationSpeed = 0.125f;
    [SerializeField] private Vector3 _boundsSize = Vector3.one * 3;

    private NativeArray<FractalPart>[] _parts;
    private NativeArray<Matrix4x4>[] _matrices;
    private ComputeBuffer[] _matricesBuffers;
    
    #endregion

    #region Unity events

    private void OnEnable()
    {
        _parts              = new NativeArray<FractalPart>[_depth];
        _matrices           = new NativeArray<Matrix4x4>[_depth];
        _matricesBuffers    = new ComputeBuffer[_depth];

        var stride = 16 * 4;

        for (int i = 0, length = 1; i < _parts.Length; i++, length *= _childCount)
        {
            _parts[i]           = new NativeArray<FractalPart>(length, Allocator.Persistent);
            _matrices[i]        = new NativeArray<Matrix4x4>(length, Allocator.Persistent);
            _matricesBuffers[i] = new ComputeBuffer(length, stride);
        };

        _parts[0][0] = CreatePart(0);

        for (var li = 1; li < _parts.Length; li++)
        {
            var levelParts = _parts[li];

            for (var fpi = 0; fpi < levelParts.Length; fpi += _childCount)
            {
                for (var ci = 0; ci < _childCount; ci++)
                {
                    levelParts[fpi + ci] = CreatePart(ci);
                };
            };
        };

        _propertyBlock ??= new MaterialPropertyBlock();
    }

    private void OnDisable()
    {
        for (int i = 0; i < _matricesBuffers.Length; i++)
        {
            _parts[i].Dispose();
            _matrices[i].Dispose();
            _matricesBuffers[i].Release();
        };

        _parts = null;
        _matrices = null;
        _matricesBuffers = null;
    }

    private void OnValidate()
    {
        if (!enabled || _parts is null) return;

        OnDisable();
        OnEnable();
    }

    private void Update()
    {
        var scale = 1f;

        var spinAngleDelta = _rotationSpeed * PI * Time.deltaTime;

        var rootPart = _parts[0][0];

        rootPart.SpinAngle += spinAngleDelta;
        rootPart.WorldRotation = mul(rootPart.Rotation, quaternion.RotateY(rootPart.SpinAngle));

        _parts[0][0] = rootPart;

        _matrices[0][0] =
            Matrix4x4.TRS(
                rootPart.WorldPosition,
                rootPart.WorldRotation,
                scale * Vector3.one);

        JobHandle jobHandle = default;

        for (var li = 1; li < _parts.Length; li++)
        {
            scale *= _scaleBias;

            var job = new FractalJob
            {
                SpinAngleDelta  = spinAngleDelta,
                Scale           = scale,
                Parents         = _parts[li - 1],
                Parts           = _parts[li],
                Matrices        = _matrices[li]
            };

            jobHandle = job.Schedule(_parts[li].Length, 0, jobHandle);
        };

        jobHandle.Complete();

        var bounds = new Bounds(rootPart.WorldPosition, _boundsSize);

        for (var i = 0; i < _matricesBuffers.Length; i++)
        {
            var buffer = _matricesBuffers[i];

            buffer.SetData(_matrices[i]);

            _propertyBlock.SetBuffer(_matricesID, buffer);

            _material.SetBuffer(_matricesID, buffer);

            Graphics
                .DrawMeshInstancedProcedural(
                    _mesh,
                    0,
                    _material,
                    bounds,
                    buffer.count,
                    _propertyBlock);
        };
    }

    #endregion

    #region Methods

    private FractalPart CreatePart(int childIndex)
    {
        return new FractalPart
        {
            Direction = _directions[childIndex],
            Rotation = _rotations[childIndex],
        };
    }

    #endregion

    #region Nested structs

    private struct FractalPart
    {
        public Vector3 Direction;
        public Quaternion Rotation;
        public Vector3 WorldPosition;
        public Quaternion WorldRotation;
        public float SpinAngle;
    }

    [BurstCompile(FloatPrecision = FloatPrecision.Standard, FloatMode = FloatMode.Fast)]
    private struct FractalJob : IJobParallelFor
    {
        #region Fields

        public float SpinAngleDelta;
        public float Scale;

        public NativeArray<FractalPart> Parts;

        [ReadOnly] public NativeArray<FractalPart> Parents;

        [WriteOnly] public NativeArray<Matrix4x4> Matrices;

        #endregion

        #region Base methods

        public void Execute(int index)
        {
            var parent  = Parents[index / _childCount];
            var part    = Parts[index];

            part.SpinAngle += SpinAngleDelta;
            part.WorldRotation = mul(parent.WorldRotation, mul(part.Rotation, quaternion.RotateY(part.SpinAngle)));
            part.WorldPosition = parent.WorldPosition + parent.WorldRotation * (_positionOffset * Scale * part.Direction);

            Parts[index] = part;

            Matrices[index] =
                float4x4.TRS(
                    part.WorldPosition,
                    part.WorldRotation,
                    float3(Scale));
        }

        #endregion
    }

    #endregion
}