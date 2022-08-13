using System.Collections;
using UnityEngine;

public class RayShooter : FireAction
{
    #region Fields

    [SerializeField] private Camera _camera;

    #endregion

    #region Properties

    public Camera Camera
    {
        get => _camera;
        set => _camera = value;
    }

    #endregion

    #region Unity events

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        };

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        };

        if(Input.anyKey && !Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState    = CursorLockMode.Locked;
            Cursor.visible      = false;
        }
        else
        {
            Cursor.lockState    = CursorLockMode.None;
            Cursor.visible      = true;
        };
    }

    #endregion

    #region Base methods

    protected override void Shoot()
    {
        base.Shoot();

        if(_bullets.Count > 0)
        {
            StartCoroutine(ShootCoroutine());
        };
    }

    #endregion

    #region Methods

    private IEnumerator ShootCoroutine()
    {
        if (_reloading)
        {
            yield break;
        };

        var point =
            new Vector3(
                _camera.pixelWidth / 2,
                _camera.pixelHeight / 2,
                0);

        var ray = _camera.ScreenPointToRay(point);

        if(!Physics.Raycast(ray, out var hit))
        {
            yield break;
        };

        var bullet = _bullets.Dequeue();

        bullet.SetActive(true);
        bullet.transform.position   = hit.point;
        bullet.transform.parent     = hit.transform;

        _countBullet = _bullets.Count.ToString();

        _ammunition.Enqueue(bullet);

        yield return new WaitForSeconds(2f);

        bullet.SetActive(false);
    }

    #endregion
}