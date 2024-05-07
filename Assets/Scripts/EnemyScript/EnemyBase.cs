using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Data")]
    public UnitClassData unitData;

    public List<Node> currentPath = new List<Node>();

    [Header("Stats and Attributes")]

    [SerializeField] private int Level;

    [SerializeField] private int Health = 0;
    [SerializeField] private int Damage = 0;
    [SerializeField] private int Intelligence = 0;
    [SerializeField] private int MovementSpeed = 0;
    [SerializeField] private int Accuracy = 0;
    [SerializeField] private int Reflexes = 0;
    [SerializeField] private int AttackSpeed = 0;

    [Header("Vision and Field of View")]

    [SerializeField] private Collider Proximity;

    [Range(0, 360)]
    public float viewAngle;
    public float baseViewAngle;
    public Vector3 OffsetToGround = new Vector3(0, 2, 0);

    public float lockOnThreshold = 2f;

    public float coneRadius = 2.0f;
    public float coneLength = 5.0f;

    public bool lockedOn;
    public bool canBeHarmed;
    public bool StartCheckingLoS;

    public LayerMask hitMask;
    public LayerMask blockedMask;
    public Quaternion targetRotation;

    public float rotationSpeed = 5f;
    private bool rotating = false;
    private bool coroutineStarted = false;

    [Header("Logistics")]

    public Transform player;
    public GameObject playerCharacter;
    public Collider EnemyCollider;
    public Node PositionInGrid;

    private bool TrackPlayer;
    private bool dead;
    private bool InRange = false;
    private bool InLineOfSight;
    private bool CanMove = false;

    public float refreshRate = 0.05f;

    private bool isAttacking = false;

    public enum UnitType
    {
        Static,
        Mobile
    }

    public enum UnitState
    {
        Waiting,
        Patrolling,
        Chasing,
    }

    public UnitType UnitMobilityType;

    public UnitState CurrentUnitState;
    private bool shooting;

    void Start()
    {
        SetupEnemyProperties();
        SetUpLevelScaling();

        if (UnitMobilityType == UnitType.Mobile)
        {
            Invoke("FindClosestGridTile", 1.0f);
        }
    }
    void Update()
    {
        switch (CurrentUnitState)
        {
            case UnitState.Waiting:
                coroutineStarted = false;
                CurrentUnitState = UnitState.Patrolling;
                Patrol();
                break;
            case UnitState.Patrolling:
                coroutineStarted = false;
                Patrol();
                CheckLoS();
                RotateTowardsTarget();
                break;
            case UnitState.Chasing:
                if (player != null && !coroutineStarted)
                {
                    StopCoroutine(PatrolCoroutine());
                    StartCoroutine(ChasePlayer());
                    coroutineStarted = true;
                }
                CheckLoS();
                RotateTowardsTarget();
                break;
        }

        if (InLineOfSight && CurrentUnitState != UnitState.Chasing)
        {
            FindClosestGridTile();
            CurrentUnitState = UnitState.Chasing;
        }
    }

    void RotateTowardsTarget()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(transform.rotation, targetRotation) < lockOnThreshold)
        {
            rotating = false;
            lockedOn = true;

            if (!shooting)
            {
                StartCoroutine(ShootCoroutine());
                shooting = true;
            }
        }
        else
        {
            rotating = true;
            lockedOn = false;
            shooting = false;
        }
    }

    void SetupEnemyProperties()
    {
        Health = unitData.defaultHealth;
        Damage = unitData.defaultPower;
        Intelligence = unitData.defaultIntelligence;
        MovementSpeed = unitData.defaultSpeed;
        Accuracy = unitData.defaultAim;
        Reflexes = unitData.defaultReflexes;
        AttackSpeed = unitData.defaultAttackSpeed;

        rotationSpeed = Reflexes;
        coneRadius = Intelligence * 10;
        viewAngle = baseViewAngle + (Intelligence * 4);
    }

    void SetUpLevelScaling()
    {
        for (int i = 1; i <= Level; i++)
        {
            Health = CalculateStatWithGrowth(Health, unitData.healthGrowth);
            Damage = CalculateStatWithGrowth(Damage, unitData.powerGrowth);
            Intelligence = CalculateStatWithGrowth(Intelligence, unitData.intelligenceGrowth);
            MovementSpeed = CalculateStatWithGrowth(MovementSpeed, unitData.speedGrowth);
            Accuracy = CalculateStatWithGrowth(Accuracy, unitData.aimGrowth);
            Reflexes = CalculateStatWithGrowth(Reflexes, unitData.reflexesGrowth);
            AttackSpeed = CalculateStatWithGrowth(AttackSpeed, unitData.attackSpeedGrowth);
        }
    }

    private void Patrol()
    {
        if (PositionInGrid != null)
        {
            GameObject startNode = PositionInGrid.gameObject;

            // Check if currentPath is empty or if the enemy is close enough to the last node
            if (currentPath.Count == 0 || Vector3.Distance(transform.position, currentPath[currentPath.Count - 1].transform.position + OffsetToGround) <= 0f)
            {
                GameObject endNode = Grid.Instance.allTiles[Random.Range(0, Grid.Instance.allTiles.Count)].gameObject;

                if (endNode.GetComponent<Node>().walkable)
                {
                    currentPath = AStarPathfinding.FindPath(startNode, endNode, Grid.Instance.gridReference);
                    StartCoroutine(PatrolCoroutine());
                }
            }
        }
    }

    public IEnumerator PatrolCoroutine()
    {
        if (PositionInGrid != null && currentPath.Count > 0 && CurrentUnitState == UnitState.Patrolling)
        {
            foreach (Node node in currentPath)
            {
                if (node.walkable)
                {
                    Vector3 targetPosition = node.transform.position + OffsetToGround;
                    while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, targetPosition, unitData.defaultSpeed * Time.deltaTime);
                        if (InLineOfSight)
                        {
                            CurrentUnitState = UnitState.Chasing;
                            break;
                        }
                        yield return null;
                    }

                    // Ensure that the position is exactly at the target position
                    transform.position = targetPosition;

                    PositionInGrid = node;
                }
            }
        }
    }

    public IEnumerator ChasePlayer()
    {
        while (CurrentUnitState == UnitState.Chasing)
        {
            // Continuously update the path
            GetPlayerTile();

            List<Node> nodes = new List<Node>(currentPath);

            Node[] nodesArray = nodes.ToArray();
            for (int i = 0; i < currentPath.Count; i++)
            {
                Node node = currentPath[i];
                if (!node.walkable)
                    continue;

                Vector3 targetPosition = node.transform.position + OffsetToGround;

                yield return StartCoroutine(MoveTowardsPosition(targetPosition));

                Node[] currentPathArray = currentPath.ToArray();
                if (!currentPathArray.SequenceEqual(nodesArray))
                {
                    break;
                }
            }

            yield return new WaitForSeconds(refreshRate);
        }
    }

    private IEnumerator MoveTowardsPosition(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(transform.position, targetPosition);
        float moveSpeed = unitData.defaultSpeed;

        // Keep moving until the distance to the target position is less than a small threshold
        while (distance > 0.1f)
        {
            // Calculate the interpolation factor based on distance and movement speed
            float interpolationFactor = moveSpeed * Time.deltaTime / distance;
            transform.position = Vector3.Lerp(transform.position, targetPosition, interpolationFactor);

            distance = Vector3.Distance(transform.position, targetPosition);

            yield return null;
        }

        // Ensure the unit reaches the exact target position
        transform.position = targetPosition;

        // Update the grid position after reaching the target position
        FindClosestGridTile();
        GetPlayerTile();
    }


    void FindClosestGridTile()
    {
        Transform closestTile = null;
        float closestDistance = Mathf.Infinity;

        foreach (Node tile in Grid.Instance.allTiles)
        {
            float distance = Vector3.Distance(transform.position, tile.gameObject.transform.position);

            if (distance < closestDistance)
            {
                closestTile = tile.gameObject.transform;
                closestDistance = distance;
                PositionInGrid = tile;
            }
        }

        if (closestTile != null)
        {
            transform.position = closestTile.position + OffsetToGround;
            
        }
    }

    void GetPlayerTile()
    {
        Transform closestTile = null;
        float closestDistance = Mathf.Infinity;

        foreach (Node tile in Grid.Instance.allTiles)
        {
            float distance = Vector3.Distance(player.transform.position, tile.gameObject.transform.position);

            if (distance < closestDistance)
            {
                closestTile = tile.gameObject.transform;
                closestDistance = distance;
            }
        }

        if (closestTile != null)
        {
            GameObject StartNode = PositionInGrid.gameObject;
            GameObject EndNode = closestTile.gameObject;

            currentPath = AStarPathfinding.FindPath(StartNode, EndNode, Grid.Instance.gridReference);
        }
    }

    // Rolls a number between 0.1 - 1.0. If the rolled number is less or equal to the growth rate, the unit receives a point in that respective stat. Else, nothing happens.
    private int CalculateStatWithGrowth(int baseStat, float growthRate)
    {
        float randomValue = Random.value;

        if (randomValue <= growthRate)
        {
            return baseStat + 1;
        }

        return baseStat;
    }

    public IEnumerator ShootCoroutine()
    {
        while (lockedOn && StartCheckingLoS)
        {
            GameObject Bullet = Instantiate(unitData.Projectile, transform.position, Quaternion.identity);

            Vector3 directionToPlayer = (player.position - Bullet.transform.position).normalized;

            Rigidbody BulletRB = Bullet.GetComponent<Rigidbody>();

            BulletRB.AddForce(directionToPlayer * 25, ForceMode.Impulse);

            Debug.Log("Shot bullet");

            yield return new WaitForSeconds(5f / AttackSpeed);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCheckingLoS = true;
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCheckingLoS = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCheckingLoS = false;

            if (!unitData.PermanentlyChasePlayer)
            {
                CurrentUnitState = UnitState.Patrolling;
            }

            shooting = false;
            lockedOn = false;
        }
    }

    public void CheckLoS()
    {
        if (StartCheckingLoS)
        {
            Collider[] cone = Physics.OverlapSphere(transform.position, coneRadius, hitMask);

            if (cone.Length != 0 && !CanMove)
            {
                foreach (var hitCollider in cone)
                {
                    var target = hitCollider.GetComponent<PlayerMovementPhysics>();
                    if (target == null) continue;

                    Transform targetTransform = hitCollider.transform;
                    Vector3 targetDirection = (targetTransform.position - transform.position + OffsetToGround).normalized;
                    float angleToTarget = Vector3.Angle(transform.forward, targetDirection);

                    if (angleToTarget < viewAngle / 2 || CurrentUnitState == UnitState.Chasing)
                    {
                        float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
                        RaycastHit hit;
                        if (!Physics.Raycast(transform.position, targetDirection, out hit, distanceToTarget, blockedMask) || CurrentUnitState == UnitState.Chasing)
                        {
                            InLineOfSight = true;
                            targetRotation = Quaternion.LookRotation(targetDirection);
                            player = target.transform;
                            return;
                        }
                    }
                }

                InLineOfSight = false;
            }
        }
    }
}
