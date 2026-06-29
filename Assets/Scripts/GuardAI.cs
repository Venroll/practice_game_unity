using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class GuardAI : MonoBehaviour
{
    enum State { Patrol, Suspicious, Chase }
    State state = State.Patrol;

    [SerializeField] Transform[] patrolPoints;
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float viewRadius = 5f;
    [SerializeField][Range(0, 360)] float viewAngle = 90f;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] float suspicionTime = 3f;
    [SerializeField] float suspicionSpeed = 1.5f;   
    [SerializeField] float chaseSpeed = 3.5f;
    [SerializeField] float catchDist = 0.6f;
    [SerializeField] float turnSpeed = 180f; 

    Rigidbody2D rb;
    Transform player;
    float timer;
    int patrolIndex;
    float starterChaseSpeed;
    float starterPatrolSpeed;
    Vector2 moveDirection;
    Vector2 lastKnownPlayerPos;       
    bool reachedLastSeenPos;          
    LineRenderer visionLine;
    const int raysCount = 10;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        timer = suspicionTime;
        lastKnownPlayerPos = transform.position;
        starterChaseSpeed = chaseSpeed;
        starterPatrolSpeed = patrolSpeed;

        visionLine = GetComponent<LineRenderer>();
        if (visionLine == null)
            visionLine = gameObject.AddComponent<LineRenderer>();
        visionLine.positionCount = raysCount + 2;
        visionLine.loop = false;               
        visionLine.startWidth = 0.05f;
        visionLine.endWidth = 0.05f;
        visionLine.material = new Material(Shader.Find("Sprites/Default"));
        visionLine.material.color = new Color(1f, 0f, 0f, 0.3f); 
        visionLine.useWorldSpace = true;       
    }

    void Update()
    {
        if (state == State.Patrol && CanSeePlayer())
        {
            lastKnownPlayerPos = player.position;
            timer = suspicionTime;
            reachedLastSeenPos = false;
            state = State.Suspicious;
        }
        else if (state == State.Suspicious)
        {
            timer -= Time.deltaTime;
            if (CanSeePlayer())
            {
                state = State.Chase;
            }
            else if (timer <= 0f)
            {
                state = State.Patrol;
                timer = suspicionTime;
            }
        }
        else if (state == State.Chase && !CanSeePlayer())
        {
            lastKnownPlayerPos = player.position;
            timer = suspicionTime;
            reachedLastSeenPos = false;
            state = State.Suspicious;
        }
    }

    void FixedUpdate()
    {
        moveDirection = Vector2.zero;

        if (state == State.Patrol && patrolPoints.Length > 0)
        {
            Vector2 target = patrolPoints[patrolIndex].position;
            Vector2 newPos = Vector2.MoveTowards(rb.position, target, patrolSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
            moveDirection = (target - rb.position).normalized;
            if (Vector2.Distance(rb.position, target) < 0.1f)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            }
        }
        else if (state == State.Suspicious)
        {
            if (!reachedLastSeenPos)
            {
                Vector2 newPos = Vector2.MoveTowards(rb.position, lastKnownPlayerPos, suspicionSpeed * Time.fixedDeltaTime);
                rb.MovePosition(newPos);
                moveDirection = (lastKnownPlayerPos - rb.position).normalized;

                if (Vector2.Distance(rb.position, lastKnownPlayerPos) < 0.2f)
                {
                    reachedLastSeenPos = true;
                }
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, transform.eulerAngles.z + turnSpeed * Time.fixedDeltaTime);
            }
        }
        else if (state == State.Chase && player != null)
        {
            Vector2 playerPos = player.position;
            Vector2 newPos = Vector2.MoveTowards(rb.position, playerPos, chaseSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
            moveDirection = (playerPos - rb.position).normalized;
        }

        if (moveDirection != Vector2.zero)
        {
            float targetAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, turnSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        UpdateVisionCone();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Debug.Log("Obstacle in");
            chaseSpeed = 1f;
            patrolSpeed = 1f;
        }
        else if (other.CompareTag("Player"))
        {
            Debug.Log("Player in");
            Destroy(other.gameObject);
            FindAnyObjectByType<GameSystem>().ActivateLoseMenu();
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Debug.Log("Obstacle out");
            chaseSpeed = starterChaseSpeed;
            patrolSpeed = starterPatrolSpeed;
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;
        Vector2 dir = (player.position - transform.position).normalized;
        if (Vector2.Distance(transform.position, player.position) > viewRadius) return false;
        if (Vector2.Angle(transform.right, dir) > viewAngle / 2f) return false;

        float step = viewAngle / (raysCount - 1);
        for (int i = 0; i < raysCount; i++)
        {
            float angle = -viewAngle / 2f + step * i;
            Vector2 rayDir = Quaternion.Euler(0, 0, angle) * transform.right;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, viewRadius, obstacleMask);
            if (hit.collider == null)
            {
                RaycastHit2D playerHit = Physics2D.Raycast(transform.position, rayDir, viewRadius, LayerMask.GetMask("Player"));
                if (playerHit.collider != null && playerHit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    void UpdateVisionCone()
    {
        if (visionLine == null) return;

        Vector3[] points = new Vector3[raysCount + 2];
        points[0] = transform.position;
        float step = viewAngle / (raysCount - 1);

        for (int i = 0; i < raysCount; i++)
        {
            float angle = -viewAngle / 2f + step * i;
            Vector2 rayDir = Quaternion.Euler(0, 0, angle) * transform.right;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, viewRadius, obstacleMask);
            float dist = hit.collider != null ? hit.distance : viewRadius;
            points[i + 1] = transform.position + (Vector3)(rayDir * dist);
        }

        points[raysCount + 1] = transform.position;
        visionLine.SetPositions(points);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        float step = viewAngle / (raysCount - 1);
        for (int i = 0; i < raysCount; i++)
        {
            float angle = -viewAngle / 2f + step * i;
            Vector3 rayDir = Quaternion.Euler(0, 0, angle) * transform.right;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, rayDir * viewRadius);
        }
    }
}