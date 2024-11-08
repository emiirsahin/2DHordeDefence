using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using System;

public abstract class Agent : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 5f;

    protected Vector2 targetPosition;

    public float health = 50f;
    protected float range = 15f;
    protected float attackInterval = 1.5f;
    protected float damageSpriteChangeDuration = .3f;
    public int heightOffset;
    public int locOffset;
    protected float damage = 15f;

    public int currentWaypointIndex;
    public int nextWaypointIndex;
    protected int currentZone;

    protected bool isAttacking = false;
    protected bool alive = true;
    protected bool attackCoroutineActive = false;
    protected bool attackLandedCoroutineActive = false;
    protected bool type; // true means ally, false means enemy

    protected List<Agent>[,] mobLocations;
    public int[] mobsInsideZones;

    public SpriteRenderer spriteRenderer;
    public Color damageColor;
    public Color originalColor;

    protected Coroutine attackCoroutine;
    protected Coroutine attackLandedCoroutine;

    protected void Movement()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    protected abstract void SetSubValues(); // Set the start values for subclasses

    protected abstract void MovementStageUpdate(); // Update the movement stage depending on whether the instance is an enemy or ally

    protected abstract bool GetNextStageCondition(); // Get the condition for the unit to get to the next stage, NOT ZONE

    protected abstract bool TargetFinderX();

    protected abstract bool TargetFinderRepeatedLogic(int a);

    protected abstract IEnumerator AttackLandedRoutine();

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

    protected abstract void Attack();

    protected abstract IEnumerator AttackRoutine();

    public void ReceiveDamage(float takenDamage)
    {
        health -= takenDamage;

        StartCoroutine(ChangeColorForSeconds());

        if (health <= 0f && alive)
        {
            alive = false;
            Die();
        }
    }

    protected abstract void Die();

    protected bool CheckRange(float loc1, float loc2)
    {
        return Math.Abs(loc1 - loc2) < range;
    }

    protected IEnumerator ChangeColorForSeconds()
    {
        spriteRenderer.color = damageColor;

        yield return new WaitForSeconds(damageSpriteChangeDuration);

        spriteRenderer.color = originalColor;
    }

    protected void CancelAttackCoroutine()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    protected void CancelAttackLandedCoroutine()
    {
        if (attackLandedCoroutine != null)
        {
            StopCoroutine(attackLandedCoroutine);
            attackLandedCoroutine = null;
        }
    }
}