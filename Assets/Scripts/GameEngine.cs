using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class GameEngine : MonoBehaviour
{
    public static GameEngine instance; // Singleton Instance

    public Transform[] waypoints; // Movement waypoints
    [HideInInspector] public int waypointAmount; // Number of waypoints
    public int zoneAmount; // Number of play area subdivisions

    public Transform beginning; // Start waypoint
    public Transform end;       // End waypoint

    [SerializeField] private GameObject enemy;  // Enemy prefab
    [SerializeField] private GameObject ally;   // Ally prefab
    [SerializeField] private GameObject testObject; // Zone border visualizing prefab
    [SerializeField] private GameObject player;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private GameObject playerInstance;

    public Button allyButton;   // Spawn ally button
    public Button enemyButton;  // Spawn enemy button
    public Button manyButton;   // Spawn many, performance test button
    public int manyMobAmount = 50;  // Spawn many mob amount (each)

    [HideInInspector] public List<Agent>[,] enemyLocations; // Array of zone lists containing enemies in each zone
    [HideInInspector] public List<Agent>[,] allyLocations;  // Array of zone lists containing allies in each zone
    [HideInInspector] public float[] zoneBorders;   // Array holding the borders of the zones
    [HideInInspector] public int[] enemiesInsideZones;
    [HideInInspector] public int[] alliesInsideZones;
    [HideInInspector] public int numberOfLanes = 5;

    private int[] heightOffsets = { 4, -2, 4, 2, -4, -2, 4, 0, -4, 2,
                                    -2, 4, 0, -4, 2, 0, 2, -4, -2, 0,
                                    4, -4, -2, 4, 2, 0, -2, 0, -4, 2,
                                    4, -4, 2, -2, 0, 4, -4, 2, -2, 0,
                                    -4, 2, 0, 4, -2, 0, 2, -4, 4, -2 };

    private int heightOffsetIndex = 0;   

    void Awake()
    {
        if (instance == null)   // Singleton requirement
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        waypointAmount = waypoints.Length;
        allyButton.onClick.AddListener(SpawnAlly); // Initialize button on-clicks
        enemyButton.onClick.AddListener(SpawnEnemy);
        manyButton.onClick.AddListener(SpawnMobs);
    }

    void Start()
    {
        InitializeZones();
        enemyLocations = new List<Agent>[zoneAmount, numberOfLanes];
        enemiesInsideZones = new int[zoneAmount];

        allyLocations = new List<Agent>[zoneAmount, numberOfLanes];
        alliesInsideZones = new int[zoneAmount];

        for (int i = 0; i < zoneAmount; i++)
        {
            for (int j = 0; j < numberOfLanes; j++)
            {
                enemyLocations[i, j] = new List<Agent>();
                allyLocations[i, j] = new List<Agent>();
            }
            enemiesInsideZones[i] = 0;
            alliesInsideZones[i] = 0;
        }
        InitializePlayer();
    }

    private void InitializePlayer()
    {
        playerInstance = Instantiate(player, new Vector3(0, 0, 0), Quaternion.identity);
        virtualCamera.Follow = playerInstance.transform;
        Agent playerInstanceAgent = playerInstance.GetComponent<Agent>();
        AddMobToZone(playerInstanceAgent, 0, 0, true);
    }

    private void InitializeZones()
    {
        int borderAmount = zoneAmount - 1;
        zoneBorders = new float[borderAmount];

        float distance = (GameEngine.instance.end.position.x - GameEngine.instance.beginning.position.x) / zoneAmount;

        for (int i = 0; i < borderAmount; i++)
        {
            zoneBorders[i] = GameEngine.instance.beginning.position.x + (i + 1) * distance;
            DisplayZoneBorders(i);
        }
    }

    private void DisplayZoneBorders(int index)
    {
        Instantiate(testObject, new Vector3(zoneBorders[index], 0, 0), Quaternion.identity);
    }

    private void SpawnEnemy()
    {
        GameObject newEnemy = Instantiate(enemy, new Vector3(end.position.x, end.position.y, 0), Quaternion.identity);
        Agent newEnemyMob = newEnemy.GetComponent<Agent>();
        newEnemyMob.heightOffset = heightOffsets[heightOffsetIndex % 50];
        AddMobToZone(newEnemyMob, zoneAmount - 1, heightOffsets[heightOffsetIndex % 50], false);
        heightOffsetIndex++;
    }

    private void SpawnAlly()
    {
        GameObject newAlly = Instantiate(ally, new Vector3(beginning.position.x, beginning.position.y, 0), Quaternion.identity);
        Agent newAllyMob = newAlly.GetComponent<Agent>();
        newAllyMob.heightOffset = heightOffsets[heightOffsetIndex % 50];
        AddMobToZone(newAllyMob, 0, heightOffsets[heightOffsetIndex % 50], true);
        heightOffsetIndex++;
    }

    public void MoveMobToZone(Agent mob, int curZoneIndex, int nextZoneIndex, int heightOffset, bool type) // Height offset is still a literal offset here, not an index, index is applied inside Add and Remove Mob methods.
    {
        AddMobToZone(mob, nextZoneIndex, heightOffset, type);
        RemoveMobFromZone(mob, curZoneIndex, heightOffset, type);
    }

    public void AddMobToZone(Agent mob, int nextZoneIndex, int heightOffset, bool type)
    {
        if (type)
        {
            allyLocations[nextZoneIndex, HeightOffsetTranslateToIndex(heightOffset)].Add(mob);
            alliesInsideZones[nextZoneIndex]++; // Currently mobs are only spawned at spawn points, will need to be changed if other spawn points are added, a separate method is likely the best course. SAME GOES FOR ENEMY.
        }
        else
        {
            enemyLocations[nextZoneIndex, HeightOffsetTranslateToIndex(heightOffset)].Add(mob);
            enemiesInsideZones[nextZoneIndex]++;
        }
    }

    public void RemoveMobFromZone(Agent mob, int curZoneIndex, int heightOffset, bool type)
    {
        if (type)
        {
            allyLocations[curZoneIndex, HeightOffsetTranslateToIndex(heightOffset)].Remove(mob);
            alliesInsideZones[curZoneIndex]--;
        }
        else
        {
            enemyLocations[curZoneIndex, HeightOffsetTranslateToIndex(heightOffset)].Remove(mob);
            enemiesInsideZones[curZoneIndex]--;
        }
    }

    private void SpawnMobs()
    {
        for (int i = 0; i < manyMobAmount; i++)
        {
            SpawnAlly();
            SpawnEnemy();
        }
    }

    public int HeightOffsetTranslateToIndex(int offset)
    {
        return offset / 2 + 2;
    }
}
