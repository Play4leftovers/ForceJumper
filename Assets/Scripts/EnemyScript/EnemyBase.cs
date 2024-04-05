using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    public UnitClassData unitData;
    [SerializeField] private Transform player;
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private NavMeshAgent EnemyNavMeshAgent;

    public GameObject[] ItemsDropped;

    [SerializeField] private AudioSource EnemyAudioSource;

    [SerializeField] private GameObject playerCharacter;
    [SerializeField] private GameObject enemyBody;

    [SerializeField] private Collider Proximity;

    public Collider EnemyCollider;

    [SerializeField] private ParticleSystem BloodParticles;

    [SerializeField] private GameObject DeathGore;

    [Range(0, 360)]
    public float viewAngle;

    public float coneRadius = 2.0f;
    public float coneLength = 5.0f;

    private LayerMask hitMask;
    private LayerMask blockedMask;

    public float rotationSpeed = 5f;

    private bool rotating = false;
    private Quaternion targetRotation;

    public List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();

    private bool dead;
    private bool InRange = false;
    private bool InLineOfSight;
    private bool CanMove = false;
    private Transform _targetPosition;

    private float MeleeRange = 5f;

    public float firingRate;
    public bool lockedOn;
    public bool canBeHarmed;
    public bool StartCheckingLoS;
    public bool stealthed;
    public float cooldown;
    public float soundVolume = 0.5f;
    private int EnemyDamage = 10;
    public float Health = 10f;
    private float refreshRate = 0.5f;
    public float NormalMovementSpeed = 9f;
    private float MovementSpeed = 9f;
    private bool isAttacking = false;

    void Start()
    {
        InvokeRepeating("CheckLoS", refreshRate, refreshRate);
    }

    void Update()
    {
        if (InLineOfSight)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                rotating = false;
                lockedOn = true;

                // Start Shooting
            }
        }
    }

    public IEnumerator ShootCoroutine()
    {
        while (lockedOn)
        {
            // pew pew
            yield return new WaitForSeconds(firingRate);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (Proximity)
        {
            StartCheckingLoS = true;
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (Proximity)
        {
            StartCheckingLoS = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (Proximity)
        {
            StartCheckingLoS = true;
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
