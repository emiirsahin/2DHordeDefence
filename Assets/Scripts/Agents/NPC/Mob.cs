using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using System;

public abstract class Mob : Agent
{
    protected Agent target;
    protected List<int> localAttackLaneRandomizer;

    void Update()
    {
        MoveWithInput();
        ZoneBorderDetection();
    }

    protected void MoveWithInput()
    {
        if (!isAttacking)
        {
            if (attackCoroutineActive && !attackLandedCoroutineActive)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutineActive = false;
                TargetReset();
            }
            Movement();
            isAttacking = TargetFinderX();

            if (GetNextStageCondition())
            {
                MovementStageUpdate();
                targetPosition = GetOffsetApplied(nextWaypointIndex);
            }
        }
        else
        {
            if (target != null && !CheckRange(target.transform.position.x, transform.position.x))
            {
                TargetReset();
            }
            if (target == null)
            {
                isAttacking = TargetFinderX();
            }
            else
            {
                if (!attackCoroutineActive)
                {
                    attackCoroutine = StartCoroutine(AttackRoutine());
                }
            }
        }
    }

    /*protected void MobFacesTarget() Make the mob face its target WIP
    {
        if (type)
        {
            if (target.transform.position.x < transform.position.x)
            {
                return true;
            }
            else {
                return false;
            }
        }
        else
        {
            if (target.transform.position.x > transform.position.x)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }*/

    protected override bool TargetFinderX()
    {
        if (mobsInsideZones[currentZone] > 0)
        {
            return TargetFinderRepeatedLogic(currentZone);
        }
        else if (type ? currentZone + 1 < GameEngine.instance.zoneAmount : currentZone > 0 && mobsInsideZones[type ? currentZone + 1 : currentZone - 1] > 0)
        {
            return TargetFinderRepeatedLogic(type ? currentZone + 1 : currentZone - 1);
        }
        else if (type ? currentZone > 0 : currentZone + 1 > GameEngine.instance.zoneAmount && mobsInsideZones[type ? currentZone - 1 : currentZone + 1] > 0)
        {
            return TargetFinderRepeatedLogic(type ? currentZone - 1 : currentZone + 1);
        }
        else
        {
            return false;
        }
    }

    protected override bool TargetFinderRepeatedLogic(int currentZone)
    {
        List<Agent> mobs = mobLocations[currentZone, locOffset].FindAll(item => CheckRange(item.transform.position.x, transform.position.x)); //

        if (mobs.Count > 0)
        {
            SetTarget(mobs[Random.Range(0, mobs.Count)]);
            return true;
        }

        List<int> tmp = new List<int>(localAttackLaneRandomizer);

        for (int i = 0; i < GameEngine.instance.numberOfLanes; i++)
        {
            int a = Random.Range(0, tmp.Count);
            int b = tmp[a];
            tmp.RemoveAt(a);

            if (b == locOffset)
                continue;

            mobs = mobLocations[currentZone, b].FindAll(item => CheckRange(item.transform.position.x, transform.position.x));

            if (mobs.Count > 0)
            {
                SetTarget(mobs[Random.Range(0, mobs.Count)]);
                return true;
            }
        }
        return false;
    }

    protected override void Attack()
    {
        if (target != null)
        {
            target.ReceiveDamage(damage);
        }
    }

    protected override IEnumerator AttackRoutine()
    {
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

    protected override void Die()
    {
        GameEngine.instance.RemoveMobFromZone(this, currentZone, heightOffset, type);
        Destroy(gameObject);
    }

    protected Vector3 GetOffsetApplied(int index)
    {
        Vector3 a = new Vector3();
        a = GameEngine.instance.waypoints[index].position;
        a.y += heightOffset;
        return a;
    }

    protected void SetTarget(Agent temp)
    {
        target = temp;
    }

    protected void TargetReset()
    {
        target = null;
    }
}