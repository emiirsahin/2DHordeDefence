using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class Mob : MonoBehaviour
{
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected float moveSpeed = 5f;

    protected Mob target;

    protected Vector2 targetPosition;

    protected float health = 50f;
    protected float range = 3f;
    protected float attackInterval = 1.5f;
    [HideInInspector] public int heightOffset;
    protected float damage = 15f;

    protected int currentWaypointIndex;
    protected int nextWaypointIndex;
    protected int currentZone;

    protected bool isAttacking = false;
    protected bool alive = true;
    protected bool attackCoroutineActive = false;
    protected bool type; // true means ally, false means enemy

    protected abstract List<Mob>[,] TargetArray;

    void Update()
    {
        MoveWithInput();
        ZoneBorderDetection();
    }

    protected void MoveWithInput()
    {
        if (!isAttacking)
        {
            Movement();
            isAttacking = TargetFinder();

            if (GetNextStageCondition())
            {
                MovementStageUpdate();
                targetPosition = GetOffsetApplied(nextWaypointIndex);
            }
        }
        else
        {
            if (target == null)
            {
                isAttacking = TargetFinder();
            }
            else
            {
                if (!attackCoroutineActive)
                {
                    StartCoroutine(AttackRoutine());
                }
            }
        }
    }

    protected void Movement()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    protected abstract void SetSubValues(); // Set the start values for subclasses

    protected abstract void MovementStageUpdate(); // Update the movement stage depending on whether the instance is an enemy or ally

    protected abstract bool GetNextStageCondition(); // Get the condition for the unit to get to the next stage, NOT ZONE

    protected void ZoneBorderDetection()
    {
        if (currentZone != 0 && transform.position.x < GameEngine.instance.zoneBorders[currentZone - 1]) // Do not check if the leftmost zone
        {
            GameEngine.instance.MoveMobToZone(this, currentZone, currentZone - 1, heightOffset, type);
            currentZone--;
        }
        else if (currentZone != GameEngine.instance.zoneAmount - 1 && transform.position.x > GameEngine.instance.zoneBorders[currentZone]) // Do not check if the rightmost zone
        {
            GameEngine.instance.MoveMobToZone(this, currentZone, currentZone + 1, heightOffset, type);
            currentZone++;
        }
    }

    protected bool TargetFinder()
    {
        int locOffset = GameEngine.instance.HeightOffsetTranslateToIndex(heightOffset);

        if (type)
        {
            if (GameEngine.instance.enemiesInsideZones[currentZone] > 0)
            {
                Mob enemy = GameEngine.instance.enemyLocations[currentZone, locOffset].Find(item => CheckRange(item.transform.position.x, transform.position.x));

                if (enemy != null)
                {
                    SetTarget(enemy);
                    return true;
                }

                for (int i = 0; i < GameEngine.instance.numberOfLanes; i++)
                {
                    if (i == locOffset)
                        continue;

                    enemy = GameEngine.instance.enemyLocations[currentZone, i].Find(item => CheckRange(item.transform.position.x, transform.position.x));

                    if (enemy != null)
                    {
                        SetTarget(enemy);
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (GameEngine.instance.alliesInsideZones[currentZone] > 0)
            {
                Mob ally = GameEngine.instance.allyLocations[currentZone, locOffset].Find(item => CheckRange(item.transform.position.x, transform.position.x));

                if (ally != null)
                {
                    SetTarget(ally);
                    return true;
                }

                for (int i = 0; i < GameEngine.instance.numberOfLanes; i++)
                {
                    if (i == locOffset)
                        continue;

                    ally = GameEngine.instance.allyLocations[currentZone, i].Find(item => CheckRange(item.transform.position.x, transform.position.x));

                    if (ally != null)
                    {
                        SetTarget(ally);
                        return true;
                    }
                }
                return false; 
            }
            else
            {
                return false;
            }
        }
    }

    protected void Attack()
    {
        if (target != null)
        {
            target.ReceiveDamage(damage);
        }
    }

    protected IEnumerator AttackRoutine()
    {
        attackCoroutineActive = true;
        yield return new WaitForSeconds(attackInterval / 3);
        Attack();
        yield return new WaitForSeconds(attackInterval * 2 / 3);
        attackCoroutineActive = false;
    }

    protected void ReceiveDamage(float takenDamage)
    {
        health -= takenDamage;
        if (health <= 0f && alive)
        {
            alive = false;
            Die();
        }
    }

    private void Die()
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

    protected void SetTarget(Mob temp)
    {
        target = temp;
    }

    protected bool CheckRange(float loc1, float loc2)
    {
        return Math.Abs(loc1 - loc2) < range;
    }
}