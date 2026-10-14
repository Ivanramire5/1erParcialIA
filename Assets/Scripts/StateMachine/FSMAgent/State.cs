using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public abstract class State
{
    public abstract void Enter();

    public abstract void Update();

    public abstract void Exit();

}