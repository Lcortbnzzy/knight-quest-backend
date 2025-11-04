using System.Collections.Generic;
using Godot;

namespace Game.Utils;

public partial class ImmediateDelegateStateMachine : RefCounted
{
    // TODO: test this state machine implementation

    public delegate void State();

    private State currentState;
    private readonly HashSet<State> states = [];

    public void AddState(State state)
    {
        if (state == null) return;
        states.Add(state);
    }

    public void AddStates(params State[] stateArray)
    {
        if (stateArray == null) return;

        states.UnionWith(stateArray);
    }

    public void ChangeState(State toState)
    {
        if (toState == null || !states.Contains(toState)) return;
        currentState = toState;
        currentState();
    }

    public void ChangeStateDeferred(State toState) => Callable.From(() => ChangeState(toState)).CallDeferred();

    public State GetCurrentState() => currentState;
}