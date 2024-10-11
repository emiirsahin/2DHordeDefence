using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ally1 : Mob
{
    void Start()
    {
        currentZone = 0;
        SetSubValues();
    }

    protected override bool GetNextStageCondition()
    {
        return transform.position.x >= targetPosition.x;
    }

    protected override void SetSubValues()
    {
        type = true;
        currentWaypointIndex = 0;
        nextWaypointIndex = 1;
        transform.position = GetOffsetApplied(currentWaypointIndex);
        targetPosition = GetOffsetApplied(nextWaypointIndex);
    }

    protected override void MovementStageUpdate() // Next stage update for enemy
    {
        if (nextWaypointIndex < GameEngine.instance.waypointAmount - 1)
        {
            nextWaypointIndex++;
            currentWaypointIndex++;
        }
    }
}