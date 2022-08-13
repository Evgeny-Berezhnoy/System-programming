using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerCharacter : Character
{
    #region Constants

    private const float GRAVITY = -9.8f;
    private const string HORIZONTAL_AXIS = "Horizontal";
    private const string VERTICAL_AXIS   = "Vertical";

    #endregion

    #region Fields

    [Header("Components")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private MouseLook _mouseLook;

    [Header("Variables")]
    [SerializeField][Range(0, 100)] private int _health = 100;
    [SerializeField][Range(0.5f, 10f)] private float _movingSpeed = 8f;
    [SerializeField] private float _acceleration = 3f;

    private Vector3 _currentVelocity;

    #endregion

    #region Unity events

    private void Start()
    {
        Initiate();        
    }

    private void OnGUI()
    {
        if (!Camera.main) return;

        var size = 12;
        var posX    = Camera.main.pixelWidth / 2 - size / 4;
        var posY    = Camera.main.pixelHeight / 2 - size / 2;
        GUI.Label(new Rect(posX, posY, size, size), "+");

        var info = $"Health: {_health}\nClip: {_fireAction.CountBullet}";
        var bulletCountSize = 50;
        var posXBul = Camera.main.pixelWidth / 2 - bulletCountSize * 2;
        var posYBul = Camera.main.pixelHeight / 2 - bulletCountSize;
        GUI.Label(new Rect(posXBul, posYBul, bulletCountSize * 2, bulletCountSize * 2), info);
    }

    #endregion

    #region Base methods

    public override void Movement()
    {
       if(_mouseLook != null && _mouseLook.Camera != null)
        {
            _mouseLook.Camera.enabled = hasAuthority;
        };

        if (!hasAuthority)
        {
            transform.position =
                Vector3
                    .SmoothDamp(
                        transform.position,
                        _serverPosition,
                        ref _currentVelocity,
                        _movingSpeed * Time.deltaTime);

            return;
        };

        var movement =
            new Vector3(
                Input.GetAxis(HORIZONTAL_AXIS) * _movingSpeed,
                0,
                Input.GetAxis(VERTICAL_AXIS) * _movingSpeed);

        movement = Vector3.ClampMagnitude(movement, _movingSpeed) * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            movement *= _acceleration;
        };

        movement.y = GRAVITY;

        movement = transform.TransformDirection(movement);

        _characterController.Move(movement);

        _mouseLook.Look();

        CmdUpdateTransform(transform.position, transform.rotation);
    }

    protected override void Initiate()
    {
        base.Initiate();

        _fireAction.Reload();
    }

    #endregion
}