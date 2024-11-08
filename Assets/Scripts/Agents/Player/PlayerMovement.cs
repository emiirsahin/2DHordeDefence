using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerMovement : Agent
{
    [SerializeField] private Transform gunPivot;
    private float[] gunRotations;
    public List<Agent> targets;
    private float xInput;

    void Start()
    {
        SetSubValues();
    }

    void Update()
    {
        MoveWithInput();
        ZoneBorderDetection();
    }

    private void MoveWithInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        if (xInput != 0)
        {
            if (attackCoroutineActive && !attackLandedCoroutineActive)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutineActive = false;
                targets.Clear();
            }
            SetGunRotation(currentWaypointIndex, xInput);
            targetPosition = GameEngine.instance.waypoints[xInput > 0 ? nextWaypointIndex : currentWaypointIndex].position;
            Movement();
            if (GetNextStageCondition())
            {
                MovementStageUpdate();
            }
            transform.localScale = new Vector3(xInput, 1, 1);
        }
        else
        {
            if (!attackCoroutineActive)
            {
                attackCoroutine = StartCoroutine(AttackRoutine());
            }
        }
    }

    private void SetGunRotation(int rotationIndex, float direction)
    {
        gunPivot.localEulerAngles = new Vector3(gunPivot.localEulerAngles.x, gunPivot.localEulerAngles.y, direction * gunRotations[rotationIndex]);
    }

    protected override bool TargetFinderX()
    {
        bool a = false;

        if (transform.localScale.x == 1)
        {
            if (mobsInsideZones[currentZone] > 0)
            {
                a |= TargetFinderRepeatedLogic(currentZone);
            }
            if ((currentZone + 1 < GameEngine.instance.zoneAmount && mobsInsideZones[currentZone + 1] > 0))
            {
                a |= TargetFinderRepeatedLogic(currentZone + 1);
            }
        }
        else
        {
            if (mobsInsideZones[currentZone] > 0)
            {
                a |= TargetFinderRepeatedLogic(currentZone);
            }
            if ((currentZone > 0 && mobsInsideZones[currentZone - 1] > 0))
            {
                a |= TargetFinderRepeatedLogic(currentZone - 1);
            }
        }

        return a;
    }

    protected override bool TargetFinderRepeatedLogic(int currentZoneTmp)
    {
        for (int i = 0; i < GameEngine.instance.numberOfLanes; i++)
        {
            targets.AddRange(mobLocations[currentZoneTmp, i].FindAll(item => FindDirectionSensitiveRange(transform.position.x, item.transform.position.x)));
        }
        if (targets.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void GetGunRotations()
    {
        for (int i = 0; i < GameEngine.instance.waypoints.Length - 1; i++)
        {
            Vector3 positionA = GameEngine.instance.waypoints[i].position;
            Vector3 positionB = GameEngine.instance.waypoints[i + 1].position;
            Vector3 direction = positionB - positionA;
            gunRotations[i] = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
    }

    protected override void MovementStageUpdate() // Next stage update for enemy
    {
        if (xInput > 0)
        {
            if (nextWaypointIndex < GameEngine.instance.waypoints.Length - 1)
            {
                nextWaypointIndex++;
                currentWaypointIndex++;
            }
        }
        else if (xInput < 0)
        {
            if (currentWaypointIndex > 0)
            {
                nextWaypointIndex--;
                currentWaypointIndex--;
            }
        }
    }

    protected override void Die()
    {
        Debug.Log("PLAYER DEATH LOGIC");
    }

    protected override void Attack()
    {
        foreach (var target in targets)
        {
            if (target != null)
            {
                target.ReceiveDamage(damage);
            }
        }
        targets.Clear();
    }

    protected override IEnumerator AttackRoutine()
    {
        if (!TargetFinderX())
        {
            yield break;
        }
        attackCoroutineActive = true;
        yield return new WaitForSeconds(attackInterval / 3);
        Attack();
        attackLandedCoroutineActive = true;
        attackLandedCoroutine = StartCoroutine(AttackLandedRoutine());
    }

    protected override IEnumerator AttackLandedRoutine()
    {
        yield return new WaitForSeconds(attackInterval * 2 / 3);
        attackLandedCoroutineActive = false;
        attackCoroutineActive = false;
    }

    protected override bool GetNextStageCondition()
    {
        if (xInput > 0)
        {
            return transform.position.x >= targetPosition.x;
        }
        else if (xInput < 0)
        {
            return transform.position.x <= targetPosition.x;
        }
        else
        {
            return false;
        }
    }

    protected override void SetSubValues()
    {
        type = true;
        heightOffset = 0;
        health = 200;
        attackInterval = 1f;
        currentWaypointIndex = 0;
        moveSpeed = 20f;
        nextWaypointIndex = currentWaypointIndex + 1;
        transform.position = GameEngine.instance.waypoints[currentWaypointIndex].position;
        gunRotations = new float[GameEngine.instance.waypoints.Length - 1];
        locOffset = GameEngine.instance.HeightOffsetTranslateToIndex(heightOffset);
        mobLocations = GameEngine.instance.enemyLocations;
        mobsInsideZones = GameEngine.instance.enemiesInsideZones;
        GetGunRotations();
    }

    protected bool FindDirectionSensitiveRange(float loc1, float loc2)
    {
        float a = (loc2 - loc1) * transform.localScale.x;
        return a < range && a > 0;
    }
}
