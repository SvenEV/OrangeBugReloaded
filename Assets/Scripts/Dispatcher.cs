using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Schedules actions to be executed on the main thread.
/// </summary>
public class Dispatcher : MonoBehaviour
{
    private static Queue<Action> _actions = new Queue<Action>();
    private static object _lock = new object();

    public static void Run(Action action)
    {
        lock (_lock)
           _actions.Enqueue(action);
    }

    private void Update()
    {
        if (_actions != null && _actions.Count != 0)
            lock (_lock)
                while (_actions.Count != 0)
                    _actions.Dequeue()();
    }
}
