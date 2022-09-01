using UnityEngine;

public class FractalProcedurial : MonoBehaviour
{
    #region Constants

    private const float _positionOffset = 1.5f;
    private const float _scaleBias = 0.5f;
    private const int _childCount = 5;

    #endregion

    #region Static fields

    private static readonly int _matricesID = Shader.PropertyToID("_Matrices");
    private static MaterialPropertyBlock _propertyBlock;
    
    private static Vector3[] _directions = new Vector3[]
    {
        Vector3.up,
        Vector3.left,
        Vector3.right,
        Vector3.forward,
        Vector3.back,
    };

    private static Quaternion[] _rotations = new Quaternion[]
    {
        Quaternion.identity,
        Quaternion.Euler(0f, 0f, 90f),
        Quaternion.Euler(0f, 0f, -90f),
        Quaternion.Euler(90f, 0f, 0f),
        Quaternion.Euler(-90f, 0f, 0f)
    };

    #endregion

    #region Fields

    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    [SerializeField, Range(1, 8)] private int _depth = 4;
    [SerializeField, Range(1, 360)] private int _rotationSpeed = 80;
    [SerializeField] private Vector3 _boundsSize = Vector3.one * 3;

    private FractalPart[][] _parts;
    private Matrix4x4[][] _matrices;
    private ComputeBuffer[] _matricesBuffers;

    #endregion

    #region Unity events

    private void OnEnable()
    {
        _parts              = new FractalPart[_depth][];
        _matrices           = new Matrix4x4[_depth][];
        _matricesBuffers    = new ComputeBuffer[_depth];

        var stride = 16 * 4;
        
        for (int i = 0, length = 1; i < _parts.Length; i++, length *= _childCount)
        {
            _parts[i]           = new FractalPart[length];
            _matrices[i]        = new Matrix4x4[length];
            _matricesBuffers[i] = new ComputeBuffer(length, stride);
        };

        _parts[0][0] = CreatePart(0);
        _parts[0][0].Direction = Vector3.zero;


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
        for(int i = 0; i < _matricesBuffers.Length; i++)
        {
            _matricesBuffers[i].Release();
        };

        _parts              = null;
        _matrices           = null;
        _matricesBuffers    = null;
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
        
        var spinAngleDelta = _rotationSpeed * Time.deltaTime;
        
        var rootPart = _parts[0][0];

        rootPart.SpinAngle += spinAngleDelta;
        
        var deltaRotation = Quaternion.Euler(0, rootPart.SpinAngle, 0);
        
        rootPart.WorldRotation  = rootPart.Rotation * deltaRotation;

        _parts[0][0] = rootPart;
        
        _matrices[0][0] =
            Matrix4x4.TRS(
                rootPart.WorldPosition,
                rootPart.WorldRotation,
                scale * Vector3.one);

        for (var li = 1; li < _parts.Length; li++)
        {
            scale *= _scaleBias;
            
            var parentParts     = _parts[li - 1];
            var levelParts      = _parts[li];
            var levelMatrices   = _matrices[li];
            
            for (var fpi = 0; fpi < levelParts.Length; fpi++)
            {
                var parent  = parentParts[fpi / _childCount];
                var part    = levelParts[fpi];
                
                part.SpinAngle += spinAngleDelta;

                deltaRotation = Quaternion.Euler(0, part.SpinAngle, 0);

                part.WorldRotation = parent.WorldRotation * part.Rotation * deltaRotation;
                part.WorldPosition = parent.WorldPosition + parent.WorldRotation * (_positionOffset * scale * part.Direction);
                
                levelParts[fpi]     = part;
                levelMatrices[fpi]  = Matrix4x4.TRS(part.WorldPosition, part.WorldRotation, scale * Vector3.one);
            };
        };

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
            Direction   = _directions[childIndex],
            Rotation    = _rotations[childIndex],
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

    #endregion
}