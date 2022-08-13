using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public abstract class FireAction : MonoBehaviour
{
    #region Fields

    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private int _startAmmunition = 20;

    protected string _countBullet;
    protected Queue<GameObject> _bullets    = new Queue<GameObject>();
    protected Queue<GameObject> _ammunition = new Queue<GameObject>();
    protected bool _reloading;

    #endregion

    #region Properties

    public string CountBullet => _countBullet;

    #endregion

    #region Unity events

    protected virtual void Start()
    {
        for(int i = 0; i < _startAmmunition; i++)
        {
            GameObject bullet;

            if(_bulletPrefab == null)
            {
                bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bullet.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }
            else
            {
                bullet = Instantiate(_bulletPrefab);
            };

            bullet.SetActive(false);

            _ammunition.Enqueue(bullet);
        };
    }

    #endregion

    #region Methods
    
    public virtual async void Reload()
    {
        _bullets = await ReloadProcess();
    }

    protected virtual void Shoot()
    {
        if(_bullets.Count == 0)
        {
            Reload();
        };
    }

    private async Task<Queue<GameObject>> ReloadProcess()
    {
        if (_reloading) return _bullets;

        _reloading = true;

        StartCoroutine(ReloadingAnimation());

        return await Task.Run(() => ReloadingTask());
    }

    private Queue<GameObject> ReloadingTask()
    {
        var cage = 10;

        var bullets = new Queue<GameObject>(_bullets);

        if (bullets.Count < cage)
        {
            Thread.Sleep(3000);
            
            while(bullets.Count > 0)
            {
                _ammunition.Enqueue(bullets.Dequeue());
            };

            cage = Mathf.Min(cage, _ammunition.Count);

            if(cage > 0)
            {
                for(int i = 0; i < cage; i++)
                {
                    bullets.Enqueue(_ammunition.Dequeue());
                };
            };
        };

        _reloading = false;

        return bullets;
    }

    private IEnumerator ReloadingAnimation()
    {
        while (_reloading)
        {
            _countBullet = " | ";
            yield return new WaitForSeconds(0.01f);
            _countBullet = @" \ ";
            yield return new WaitForSeconds(0.01f);
            _countBullet = "---";
            yield return new WaitForSeconds(0.01f);
            _countBullet = " / ";
            yield return new WaitForSeconds(0.01f);
        };

        _countBullet = _bullets.Count.ToString();
    }

    #endregion
}