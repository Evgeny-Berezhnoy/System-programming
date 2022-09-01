using UnityEngine;

public class Fractal : MonoBehaviour
{
    #region Constants
    
    private const float _positionOffset = 1.5f;
    private const float _scaleBias = 0.5f;

    #endregion

    #region Static fields

    private static readonly Vector3[] _directions = new Vector3[]
    {
        Vector3.up,
        Vector3.left,
        Vector3.right,
        Vector3.forward,
        Vector3.back
    };

    private static readonly Quaternion[] _rotations = new Quaternion[]
    {
        Quaternion.identity,
        Quaternion.Euler(0, 0, 90),
        Quaternion.Euler(0, 0, -90),
        Quaternion.Euler(90, 0, 0),
        Quaternion.Euler(-90, 0, 0)
    };
    
    #endregion

    #region Fields

    [SerializeField, Range(1, 8)] private int _depth = 4;
    [SerializeField, Range(1, 360)] private int _rotationSpeed;
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;

    private FractalPart[][] _parts;
    private int _childCount;

    #endregion

    #region Unity events

    private void OnEnable()
    {
        _parts      = new FractalPart[_depth][];
        _childCount = _directions.Length;

        for (int i = 0, length = 1; i < _parts.Length; i++, length *= _childCount)
        {
            _parts[i] = new FractalPart[length];
        };

        var scale = 1f;
        
        _parts[0][0] = CreatePart(0, 0, scale);
        
        for (var li = 1; li < _parts.Length; li++)
        {
            scale *= _scaleBias;

            var levelParts = _parts[li];

            for (var fpi = 0; fpi < levelParts.Length; fpi += _childCount)
            {
                for (var ci = 0; ci < _childCount; ci++)
                {
                    levelParts[fpi + ci] = CreatePart(li, ci, scale);
                };
            };
        };
    }

    private void Update()
    {
        var deltaRotation = Quaternion.Euler(0f, _rotationSpeed * Time.deltaTime, 0f);

        var rootPart = _parts[0][0];
        
        rootPart.Rotation *= deltaRotation;
        rootPart.Transform.localRotation = rootPart.Rotation;
        
        _parts[0][0] = rootPart;
        
        for (var li = 1; li < _parts.Length; li++)
        {
            var parentParts = _parts[li - 1];
            var levelParts  = _parts[li];

            for (var fpi = 0; fpi < levelParts.Length; fpi++)
            {
                var parentTransform = parentParts[fpi / _childCount].Transform;
                var part            = levelParts[fpi];
                
                part.Rotation *= deltaRotation;
                part.Transform.localRotation = parentTransform.localRotation * part.Rotation;
                part.Transform.localPosition = parentTransform.localPosition + parentTransform.localRotation * (_positionOffset * part.Transform.localScale.x * part.Direction);

                levelParts[fpi] = part;
            };
        };
    }

    #endregion

    #region Methods

    private FractalPart CreatePart(int levelIndex, int childIndex, float scale)
    {
        var go = new GameObject($"Fractal Part L{levelIndex} C{childIndex}");
        
        go.transform.SetParent(transform, false);
        go.transform.localScale = new Vector3(scale, scale, scale);

        go.AddComponent<MeshFilter>().mesh = _mesh;
        go.AddComponent<MeshRenderer>().material = _material;

        return new FractalPart()
        {
            Direction = _directions[childIndex],
            Rotation = _rotations[childIndex],
            Transform = go.transform
        };
    }

    #endregion

    #region Nested structs

    private struct FractalPart
    {
        #region Fields

        public Vector3 Direction;
        public Quaternion Rotation;
        public Transform Transform;

        #endregion
    }

    #endregion
}