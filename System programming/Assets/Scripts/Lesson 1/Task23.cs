using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Lesson_1
{
    public class Task23 : MonoBehaviour
    {
        #region Fields

        [SerializeField] private float _secondsLimit;
        [SerializeField] private int _framesLimit;
    
        [SerializeField] private Button _startTaskWaitForSecond;
        [SerializeField] private Button _startTaskWaitForFrames;
        [SerializeField] private Button _startTaskCompetition;
        [SerializeField] private Button _cancelTasks;

        private CancellationTokenSource _cancellationTokenSource;

        #endregion

        #region Unity events

        private void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _startTaskWaitForSecond.onClick.AddListener(StartTaskWaitForSecond);
            _startTaskWaitForFrames.onClick.AddListener(StartTaskWaitForFrames);
            _startTaskCompetition.onClick.AddListener(StartTaskCompetition);
            _cancelTasks.onClick.AddListener(CancelTasks);
        }

        private void OnDestroy()
        {
            _startTaskWaitForSecond
                .onClick
                .RemoveAllListeners();

            _startTaskWaitForFrames
                .onClick
                .RemoveAllListeners();

            _startTaskCompetition
                .onClick
                .RemoveAllListeners();

            _cancelTasks
                .onClick
                .RemoveAllListeners();

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        #endregion

        #region Methods

        private async void StartTaskWaitForSecond()
        {
            RecreateCancellationToken();

            await Task.Run(() => TaskWaitForSecondAsync(_cancellationTokenSource.Token));
        }

        private async void StartTaskWaitForFrames()
        {
            RecreateCancellationToken();

            await Task.Run(() => TaskWaitForFramesAsync(_cancellationTokenSource.Token));
        }

        private async void StartTaskCompetition()
        {
            RecreateCancellationToken();

            await Task.Run(() => TaskWaitCompetitionAsync(_cancellationTokenSource.Token));
        }
    
        private void CancelTasks()
        {
            _cancellationTokenSource.Cancel();
        }

        private void RecreateCancellationToken()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private async Task<bool> TaskWaitForSecondAsync(CancellationToken token)
        {
            var linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(token);

            var dateFinish = DateTime.Now.AddSeconds(_secondsLimit);

            while(DateTime.Now < dateFinish)
            {
                if (linkedCTS.IsCancellationRequested)
                {
                    linkedCTS.Cancel();
                    linkedCTS.Dispose();

                    Debug.Log("Wait for seconds task was cancelled.");

                    return false;
                };

                await Task.Yield();
            };

            Debug.Log("Wait for seconds task is completed.");

            linkedCTS.Cancel();
            linkedCTS.Dispose();
        
            return true;
        }

        private async Task<bool> TaskWaitForFramesAsync(CancellationToken token)
        {
            var linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(token);
        
            var framesElapsed = 0;

            while (framesElapsed < _framesLimit)
            {
                if (linkedCTS.IsCancellationRequested)
                {
                    linkedCTS.Cancel();
                    linkedCTS.Dispose();

                    Debug.Log("Wait for frames task was cancelled.");

                    return false;
                };

                await Task.Yield();

                framesElapsed++;
            };
        
            Debug.Log("Wait for frames task is completed.");

            linkedCTS.Cancel();
            linkedCTS.Dispose();

            return true;
        }

        private async Task<bool> TaskWaitCompetitionAsync(CancellationToken token)
        {
            var linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(token);

            var taskWaitForSeconds  = Task.Run(() => TaskWaitForSecondAsync(linkedCTS.Token));
            var taskWaitForFrames   = Task.Run(() => TaskWaitForFramesAsync(linkedCTS.Token));

            var tasks = new List<Task<bool>>();

            tasks.Add(taskWaitForSeconds);
            tasks.Add(taskWaitForFrames);
        
            Task firstFinishedTask = null;

            while (true)
            {
                await Task.Yield();

                for (int i = tasks.Count - 1; i >= 0; i--)
                {
                    var task = tasks[i];

                    if (task.IsCompleted)
                    {
                        if(task.Result == true)
                        {
                            firstFinishedTask = task;

                            break;
                        }
                        else
                        {
                            tasks.RemoveAt(i);
                        };
                    };
                };

                if (firstFinishedTask != null)
                {
                    linkedCTS.Cancel();
                    linkedCTS.Dispose();

                    if (firstFinishedTask == taskWaitForSeconds)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    };
                };

                if (tasks.Count == 0)
                {
                    linkedCTS.Cancel();
                    linkedCTS.Dispose();
                
                    return false;
                };
            };
        }

        #endregion
    }
}