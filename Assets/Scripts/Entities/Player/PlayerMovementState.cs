using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class PlayerMovementState : IEntityState
{
    public static PlayerMovementState RunningLeft { get; } = new PlayerMovementState();
    public static PlayerMovementState RunningRight { get; } = new PlayerMovementState();


    private PlayerMovementState()
    {
    }

    public static IEnumerable<IEntityState> List()
    {
        return new[] { RunningLeft, RunningRight };
    }
}
