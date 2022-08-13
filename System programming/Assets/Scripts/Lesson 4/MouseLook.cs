using UnityEngine;
using UnityEngine.Networking;

public class MouseLook : NetworkBehaviour
{
    #region Fields

    [Header("Components")]
    [SerializeField] private Camera _camera;

    [Header("Settings")]
    [SerializeField][Range(0.1f, 10f)] private float _sensitivity = 2f;
    [SerializeField][Range(-90f, 0f)] private float _minVert = -45f;
    [SerializeField][Range(0f, 90f)] private float _maxVert = 45f;

    private float _rotationX;
    private float _rotationY;

    #endregion

    #region Properties

    public Camera Camera => _camera;

    #endregion

    #region Methods

    public void Look()
    {
        // Rotation around X axis
        _rotationX -=   Input.GetAxis("Mouse Y") * _sensitivity;
        _rotationX =    Mathf.Clamp(_rotationX, _minVert, _maxVert);

        _camera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);

        // Rotation around Y axis
        _rotationY +=   Input.GetAxis("Mouse X") * _sensitivity;

        transform.rotation = Quaternion.Euler(0, _rotationY, 0);
    }

    #endregion
}