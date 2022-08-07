using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Lesson_1
{
    public class Unit : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Button _decreaseHealthButton;
        [SerializeField] private Button _healButton;
    
        [SerializeField] private int _healthMax;
        [SerializeField] private int _health;

        private Coroutine _healingProcess;

        #endregion

        #region Unity events

        private void Start()
        {
            _healthMax  = 100;
            _health     = _healthMax;

            _decreaseHealthButton.onClick.AddListener(() =>
            {
                StopHeal();

                _health = 1;
            });

            _healButton.onClick.AddListener(() => Heal(3f, 0.5f, 7));
        }

        private void OnDestroy()
        {
            StopHeal();

            _decreaseHealthButton
                .onClick
                .RemoveAllListeners();

            _healButton
                .onClick
                .RemoveAllListeners();
        }

        #endregion

        #region Methods

        private void StopHeal()
        {
            if (_healingProcess != null)
            {
                StopCoroutine(_healingProcess);
            };

            _healingProcess = null;
        }

        private void Heal(float timeLength, float timeStep, int healAmount)
        {
            StopHeal();

            _healingProcess = StartCoroutine(ReceiveHealing(timeLength, timeStep, healAmount));
        }

        private IEnumerator ReceiveHealing(float timeLength, float healTimeStep, int healAmount)
        {
            var timeElapsed     = 0f;
            var healTimeElapsed = 0f;

            while(timeElapsed <= timeLength)
            {
                _health = Mathf.Clamp(_health + healAmount, 0, _healthMax);

                if (_health == _healthMax)
                {
                    break;
                };

                while (healTimeElapsed < healTimeStep)
                {
                    yield return null;

                    timeElapsed     += Time.deltaTime;
                    healTimeElapsed += Time.deltaTime;
                };

                healTimeElapsed = 0;
            };

            StopHeal();
        }

        #endregion
    }
}