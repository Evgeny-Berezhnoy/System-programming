using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterController))]
public abstract class Character : NetworkBehaviour
{
    #region Events

    protected event Action _onUpdateAction;
    
    #endregion

    #region Fields

    [Header("Components")]
    [SerializeField] protected FireAction _fireAction;
    
    [SyncVar] protected Vector3 _serverPosition;
    [SyncVar] protected Quaternion _serverRotation;

    #endregion

    #region Unity events

    private void Update()
    {
        _onUpdateAction?.Invoke();
    }

    private void OnDestroy()
    {
        var handlers =
            _onUpdateAction
                ?.GetInvocationList()
                .Cast<Action>()
                .ToList();

        if(handlers != null)
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                _onUpdateAction -= handlers[i];
            };
        };
    }

    #endregion

    #region Methods

    protected virtual void Initiate()
    {
        _onUpdateAction += Movement;
    }

    [Command]
    protected void CmdUpdateTransform(Vector3 position, Quaternion rotation)
    {
        _serverPosition = position;
        _serverRotation = rotation;
    }

    public abstract void Movement();

    #endregion
}