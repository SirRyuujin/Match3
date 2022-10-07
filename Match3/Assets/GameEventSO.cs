using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameEventSO : ScriptableObject
{
    private List<GameEventListener> _listeners = new List<GameEventListener>();
    public GameObject RecentCaller;

    [ContextMenu("RAISE")]
    public void Raise(GameObject caller)
    {
        RecentCaller = caller;

        for (int i = _listeners.Count - 1; i >= 0; i--)
            _listeners[i].OnEventRaised();
    }

    public void RegisterListener(GameEventListener listener) 
    {
        _listeners.Add(listener);
    }

    public void UnregisterListener(GameEventListener listener)
    {
        _listeners.Remove(listener);
    }
}