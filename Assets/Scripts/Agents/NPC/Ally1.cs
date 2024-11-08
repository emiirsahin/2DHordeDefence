using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ally1 : Mob
{
    void Start()
    {
        currentZone = 0;
        SetSubValues();
        mobLocations = GameEngine.instance.enemyLocations;
        mobsInsideZones = GameEngine.instance.enemiesInsideZones;
        localAttackLaneRandomizer = new List<int>();
        for (int i = 0; i < GameEngine.instance.numberOfLanes; i++)
        {
            localAttackLaneRandomizer.Add(i);
        }
    }

    protected override bool GetNextStageCondition()
    {
        return transform.position.x >= targetPosition.x;
    }

    protected override void SetSubValues()
    {
        type = true;
        range = 7f;
        moveSpeed = 14f;
        currentWaypointIndex = 0;
        nextWaypointIndex = 1;
        transform.position = GetOffsetApplied(currentWaypointIndex);
        targetPosition = GetOffsetApplied(nextWaypointIndex);
        locOffset = GameEngine.instance.HeightOffsetTranslateToIndex(heightOffset);
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