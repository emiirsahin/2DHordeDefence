using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform gunPivot;
    private float[] gunRotations;
    private float attackDistance;
    private int waypointAmount;
    private int currentWaypointIndex;
    private int nextWaypointIndex;

    void Start()
    {
        waypointAmount = GameEngine.instance.waypoints.Length;
        currentWaypointIndex = 1;
        nextWaypointIndex = currentWaypointIndex + 1;
        transform.position = GameEngine.instance.waypoints[currentWaypointIndex].position;
        gunRotations = new float[waypointAmount - 1];
        GetGunRotations();
    }

    void Update()
    {
        MoveWithInput();
    }

    private void MoveWithInput()
    {
        float xInput = Input.GetAxisRaw("Horizontal");

        if (xInput != 0)
        {
            SetGunRotation(currentWaypointIndex, xInput);
            Vector2 targetPosition = GameEngine.instance.waypoints[xInput > 0 ? nextWaypointIndex : currentWaypointIndex].position;
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (xInput > 0 && transform.position.x >= targetPosition.x)
            {
                if (nextWaypointIndex < waypointAmount - 2)
                {
                    nextWaypointIndex++;
                    currentWaypointIndex++;
                }
            }
            else if (xInput < 0 && transform.position.x <= targetPosition.x)
            {
                if (currentWaypointIndex > 0)
                {
                    nextWaypointIndex--;
                    currentWaypointIndex--;
                }
            }

            transform.localScale = new Vector3(xInput, 1, 1);
        }
    }

    private void SetGunRotation(int rotationIndex, float direction)
    {
        gunPivot.localEulerAngles = new Vector3(gunPivot.localEulerAngles.x, gunPivot.localEulerAngles.y, direction * gunRotations[rotationIndex]);
    }

    private void GetGunRotations()
    {
        for (int i = 0; i < waypointAmount - 1; i++)
        {
            Vector3 positionA = GameEngine.instance.waypoints[i].position;
            Vector3 positionB = GameEngine.instance.waypoints[i + 1].position;
            Vector3 direction = positionB - positionA;
            gunRotations[i] = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
    }
}
