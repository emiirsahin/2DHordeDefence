using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1 : Mob
{
    void Start()
    {
        currentZone = GameEngine.instance.zoneAmount - 1;
        SetSubValues();
        mobLocations = GameEngine.instance.allyLocations;
        mobsInsideZones = GameEngine.instance.alliesInsideZones;
        localAttackLaneRandomizer = new List<int>();
        for (int i = 0; i < GameEngine.instance.numberOfLanes; i++)
        {
            localAttackLaneRandomizer.Add(i);
        }
    }

    protected override bool GetNextStageCondition()
    {
        return transform.position.x <= targetPosition.x;
    }

    protected override void SetSubValues()
    {
        type = false;
        range = 7f;
        moveSpeed = 14f;
        currentWaypointIndex = GameEngine.instance.waypointAmount - 1;
        nextWaypointIndex = currentWaypointIndex - 1;
        transform.position = GetOffsetApplied(currentWaypointIndex);
        targetPosition = GetOffsetApplied(nextWaypointIndex);
        locOffset = GameEngine.instance.HeightOffsetTranslateToIndex(heightOffset);
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
