using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1 : Mob
{
    void Start()
    {
        currentZone = GameEngine.instance.zoneAmount - 1;
        SetSubValues();
    }

    protected override bool GetNextStageCondition()
    {
        return transform.position.x <= targetPosition.x;
    }

    protected override void SetSubValues()
    {
        type = false;
        currentWaypointIndex = GameEngine.instance.waypointAmount - 1;
        nextWaypointIndex = currentWaypointIndex - 1;
        transform.position = GetOffsetApplied(currentWaypointIndex);
        targetPosition = GetOffsetApplied(nextWaypointIndex);
    }

    protected override void MovementStageUpdate() // Next stage update for enemy
    {
        if (nextWaypointIndex > 0)
        {
            nextWaypointIndex--;
            currentWaypointIndex--;
        }
    }
}
