using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Data")]
    public UnitClassData unitData;

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

    [Header("Logistics")]

    public Transform player;
    public GameObject playerCharacter;
    public Collider EnemyCollider;

    private bool dead;
    private bool InRange = false;
    private bool InLineOfSight;
    private bool CanMove = false;

    private float refreshRate = 0.5f;

    private bool isAttacking = false;

    void Start()
    {
        //InvokeRepeating("CheckLoS", refreshRate, refreshRate);

        SetupEnemyProperties();
        SetUpLevelScaling();
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

    void Update()
    {
        CheckLoS();

        if (InLineOfSight)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetRotation) < lockOnThreshold)
            {
                rotating = false;
                lockedOn = true;

                Debug.Log("ROTATING TO TARGET");

                // Start Shooting
            }
            else
            {
                rotating = true;
                lockedOn = false;
            }
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
        while (lockedOn)
        {
            // pew pew
            yield return new WaitForSeconds(AttackSpeed);
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
            lockedOn = false;
        }
    }

    public void CheckLoS()
    {
        if (StartCheckingLoS)
        {
            Collider[] cone = Physics.OverlapSphere(transform.position, coneRadius);

            if (cone.Length != 0 && !CanMove)
            {
                foreach (var hitCollider in cone)
                {
                    var target = hitCollider.GetComponent<PlayerMovement>();
                    Transform targetTransform = hitCollider.GetComponent<Transform>();
                    Vector3 targetDirection = (targetTransform.position - transform.position).normalized;
                    if (Vector3.Angle(transform.forward, targetDirection) < viewAngle / 2 && target || Vector3.Angle(transform.forward, targetDirection) < viewAngle / 2 && target)
                    {

                        float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);

                        if (!Physics.Raycast(transform.position, targetDirection, distanceToTarget, blockedMask))
                        {
                            InLineOfSight = true;
                            targetRotation = Quaternion.LookRotation(targetDirection);
                        }
                        else
                        {
                            InLineOfSight = false;
                        }

                    }
                }
            }
        }
    }
}
