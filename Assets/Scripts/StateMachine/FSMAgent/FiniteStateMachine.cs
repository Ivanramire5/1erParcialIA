using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine 
{
    private State _currentState;
    private HashSet<State> _allStates = new();

    public void AddState(State state)
    {
        if(!_allStates.Contains(state))
        {
            _allStates.Add(state);
        }
    }

    public void ChangeState(State state)
    {
        if (!_allStates.Contains(state))
        {
            Debug.LogError("Missing State!");
            return;
        }

        _currentState?.Exit();
        _currentState = state;
        _currentState.Enter();
    }

    public void Update()
    {
        if (_currentState != null) _currentState.Update();
    }
}
