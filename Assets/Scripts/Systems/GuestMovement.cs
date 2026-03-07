using UnityEngine;

public class GuestMovement : MonoBehaviour
{
    public float speed = 2f;

    public Transform centerPoint;
    public Transform exitPoint;
    public Transform barPoint;
    public Transform dancePoint;
    public Transform sofaPoint;

    public int drinkPrice = 10;

    public float minStayTime = 25f;
    public float maxStayTime = 60f;

    public float avoidanceRadius = 0.5f;
    public float avoidanceStrength = 1.5f;
    public float exitDespawnDistance = 0.6f;
    public LayerMask avoidanceLayerMask = ~0;
    public float avoidanceRecalculationInterval = 0.12f;
    public float needsTickInterval = 0.2f;

    public bool waitingForDrink = false;

    private Rigidbody2D rb;
    private Animator animator;
    private GuestBubbleSystem bubbles;

    private TimeSystem timeSystem;
    private BarQueueSystem barQueueSystem;
    private AmbienceSystem ambienceSystem;
    private PopularitySystem popularitySystem;

    private GuestNeeds needs;
    private GuestPersonality personality;
    private GuestDecisionSystem decisionSystem;

    private SocialPoint danceSocialPoint;
    private SocialPoint sofaSocialPoint;
    private SocialPoint currentSocialPoint;

    private Vector2 movement;
    private Vector2 targetPosition;

    private float stayTimer;
    private float stateTimer;

    private bool inBarQueue;
    private bool hasQueueTarget;
    private bool despawning;
    private bool socialBubbleShown;
    private readonly Collider2D[] avoidanceBuffer = new Collider2D[24];
    private ContactFilter2D avoidanceFilter;
    private float needsTickAccumulator;
    private float avoidanceRecalculationTimer;
    private Vector2 cachedAvoidance;
    private Vector2 lastAnimatorVelocity = new Vector2(float.MaxValue, float.MaxValue);

    private enum State
    {
        MovingToCenter,
        Wandering,
        GoingToBar,
        Dancing,
        Sitting,
        Leaving
    }

    private State currentState;

    public void Configure(TimeSystem timeSystemRef, BarQueueSystem barQueueSystemRef, AmbienceSystem ambienceSystemRef, PopularitySystem popularitySystemRef = null)
    {
        timeSystem = timeSystemRef;
        barQueueSystem = barQueueSystemRef;
        ambienceSystem = ambienceSystemRef;
        popularitySystem = popularitySystemRef;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bubbles = GetComponent<GuestBubbleSystem>();
        needs = GetComponent<GuestNeeds>();
        personality = GetComponent<GuestPersonality>();
        decisionSystem = GetComponent<GuestDecisionSystem>();

        if (needs == null)
            needs = gameObject.AddComponent<GuestNeeds>();

        if (personality == null)
            personality = gameObject.AddComponent<GuestPersonality>();

        if (decisionSystem == null)
            decisionSystem = gameObject.AddComponent<GuestDecisionSystem>();

        if (rb == null)
        {
            enabled = false;
            return;
        }

        if (timeSystem == null)
            timeSystem = FindObjectOfType<TimeSystem>();

        if (barQueueSystem == null)
            barQueueSystem = FindObjectOfType<BarQueueSystem>();

        if (ambienceSystem == null)
            ambienceSystem = FindObjectOfType<AmbienceSystem>();

        if (popularitySystem == null)
            popularitySystem = FindObjectOfType<PopularitySystem>();

        avoidanceFilter = new ContactFilter2D();
        avoidanceFilter.useTriggers = true;
        avoidanceFilter.useLayerMask = true;
        avoidanceFilter.SetLayerMask(avoidanceLayerMask);

        danceSocialPoint = dancePoint != null ? dancePoint.GetComponent<SocialPoint>() : null;
        sofaSocialPoint = sofaPoint != null ? sofaPoint.GetComponent<SocialPoint>() : null;

        currentState = State.MovingToCenter;

        stayTimer = Random.Range(minStayTime, maxStayTime);
        stateTimer = Random.Range(5f, 10f);
        needsTickInterval = Mathf.Max(0.02f, needsTickInterval);
        avoidanceRecalculationInterval = Mathf.Max(0.02f, avoidanceRecalculationInterval);
    }

    void OnDestroy()
    {
        LeaveCurrentSocialPoint();
        LeaveQueueIfAny();

        if (!despawning)
        {
            popularitySystem?.UnregisterGuest(personality);

            if (GameManager.Instance != null)
                GameManager.Instance.RemoveGuest();
        }
    }

    void Update()
    {
        if (timeSystem == null)
            return;

        if (!timeSystem.clubOpen && currentState != State.Leaving)
            BeginLeaving();

        if (!timeSystem.clubOpen || currentState == State.Leaving)
            return;

        stayTimer -= Time.deltaTime;
        stateTimer -= Time.deltaTime;

        needsTickAccumulator += Time.deltaTime;
        if (needsTickAccumulator >= needsTickInterval)
        {
            needs?.Tick(needsTickAccumulator);
            needsTickAccumulator = 0f;
        }

        if (needs != null && needs.IsExhausted)
        {
            BeginLeaving();
            return;
        }

        if (stayTimer <= 0f)
        {
            BeginLeaving();
            return;
        }

        if (stateTimer <= 0f)
        {
            if (currentState == State.GoingToBar || waitingForDrink)
            {
                stateTimer = 1f;
                return;
            }

            ChooseActivity();
            stateTimer = Random.Range(5f, 10f);
        }
    }

    void FixedUpdate()
    {
        if (timeSystem == null)
            return;

        if (!timeSystem.clubOpen && currentState != State.Leaving)
            return;

        switch (currentState)
        {
            case State.MovingToCenter:
                if (centerPoint != null)
                    MoveTo(centerPoint.position, State.Wandering);
                else
                    currentState = State.Wandering;
                break;

            case State.Wandering:
                rb.linearVelocity = movement * speed;
                break;

            case State.GoingToBar:
                MoveToBar();
                break;

            case State.Dancing:
                MoveToSocialTarget(State.Dancing);
                break;

            case State.Sitting:
                MoveToSocialTarget(State.Sitting);
                break;

            case State.Leaving:
                MoveToExit();
                break;
        }

        UpdateAnimator();
    }

    void BeginLeaving()
    {
        LeaveCurrentSocialPoint();
        LeaveQueueIfAny();
        socialBubbleShown = false;
        currentState = State.Leaving;
    }

    void MoveToBar()
    {
        if (barPoint == null)
        {
            BeginLeaving();
            return;
        }

        if (waitingForDrink)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (inBarQueue && hasQueueTarget)
        {
            float queueDistance = Vector2.Distance(transform.position, targetPosition);
            if (queueDistance > 0.25f)
            {
                MoveTowards(targetPosition, speed, applyAvoidance: false);
                return;
            }

            rb.linearVelocity = Vector2.zero;

            if (barQueueSystem != null && !barQueueSystem.IsFirst(this))
                return;
        }

        MoveTowards(barPoint.position, speed);

        float distance = Vector2.Distance(transform.position, barPoint.position);
        if (distance < 0.4f)
        {
            rb.linearVelocity = Vector2.zero;
            waitingForDrink = true;
            bubbles?.Drink();
        }
    }

    void MoveToSocialTarget(State socialState)
    {
        float distance = Vector2.Distance(transform.position, targetPosition);

        if (distance > 0.3f)
            MoveTowards(targetPosition, speed * 0.85f);
        else
        {
            rb.linearVelocity = Vector2.zero;

            if (!socialBubbleShown)
            {
                if (socialState == State.Dancing)
                {
                    bubbles?.Dance();
                    needs?.OnDance();
                }
                else if (socialState == State.Sitting)
                {
                    bubbles?.Talk();
                    needs?.OnTalk();
                }

                socialBubbleShown = true;
            }
        }

        currentState = socialState;
    }

    public void ServeDrink()
    {
        if (!waitingForDrink)
            return;

        waitingForDrink = false;

        int drinkReturn = ambienceSystem != null ? ambienceSystem.GetCurrentDrinkReturn() : drinkPrice;

        if (GameManager.Instance != null)
            GameManager.Instance.AddMoney(drinkReturn);

        needs?.OnDrinkServed();

        bubbles?.ClearBubble();

        LeaveQueueIfAny();

        GoToDance();
        stateTimer = Random.Range(5f, 10f);
    }

    public void UpdateQueuePosition(Vector2 newPosition)
    {
        targetPosition = newPosition;
        inBarQueue = true;
        hasQueueTarget = true;

        if (currentState != State.Leaving)
            currentState = State.GoingToBar;
    }

    void MoveTo(Vector2 target, State nextState)
    {
        MoveTowards(target, speed);

        float distance = Vector2.Distance(transform.position, target);
        if (distance < 0.4f)
        {
            currentState = nextState;

            if (nextState == State.Wandering)
                ChangeDirection();
        }
    }

    void MoveTowards(Vector2 target, float moveSpeed, bool applyAvoidance = true)
    {
        Vector2 direction = target - (Vector2)transform.position;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        direction.Normalize();
        Vector2 desiredDirection = direction;

        if (applyAvoidance)
        {
            Vector2 avoidance = GetAvoidanceVector();
            Vector2 withAvoidance = direction + avoidance;
            if (withAvoidance.sqrMagnitude > 0.0001f)
                desiredDirection = withAvoidance.normalized;
        }

        rb.linearVelocity = desiredDirection * moveSpeed;
    }

    void ChangeDirection()
    {
        movement = Random.insideUnitCircle;
        if (movement.sqrMagnitude < 0.001f)
            movement = Vector2.right;
        else
            movement.Normalize();
    }

    void ChooseActivity()
    {
        if (decisionSystem == null)
        {
            float roll = Random.value;

            if (roll < 0.4f)
                GoToDance();
            else if (roll < 0.7f)
                GoToBar();
            else
                GoToSofa();

            return;
        }

        float stayProgress = 1f - Mathf.Clamp01(stayTimer / Mathf.Max(1f, maxStayTime));
        GuestIntent intent = decisionSystem.Decide(needs, personality, stayProgress);

        switch (intent)
        {
            case GuestIntent.GoToBar:
                GoToBar();
                break;

            case GuestIntent.GoToDance:
                GoToDance();
                break;

            case GuestIntent.GoToSocial:
                GoToSofa();
                break;

            case GuestIntent.LeaveClub:
                BeginLeaving();
                break;
        }
    }

    void GoToBar()
    {
        if (barPoint == null)
        {
            BeginLeaving();
            return;
        }

        LeaveCurrentSocialPoint();

        bool joinedQueue = false;

        if (barQueueSystem != null && barQueueSystem.TryJoinQueue(this, out Vector2 queuePosition))
        {
            targetPosition = queuePosition;
            joinedQueue = true;
        }
        else
        {
            LeaveQueueIfAny();
            targetPosition = barPoint.position;
        }

        inBarQueue = joinedQueue;
        hasQueueTarget = joinedQueue;
        socialBubbleShown = false;
        waitingForDrink = false;
        currentState = State.GoingToBar;
    }

    void GoToDance()
    {
        LeaveQueueIfAny();
        Vector2 fallback = dancePoint != null ? (Vector2)dancePoint.position : (Vector2)transform.position;
        AssignSocialTarget(danceSocialPoint, fallback);
        socialBubbleShown = false;
        currentState = State.Dancing;
    }

    void GoToSofa()
    {
        LeaveQueueIfAny();
        Vector2 fallback = sofaPoint != null ? (Vector2)sofaPoint.position : (Vector2)transform.position;
        AssignSocialTarget(sofaSocialPoint, fallback);
        socialBubbleShown = false;
        currentState = State.Sitting;
    }

    void AssignSocialTarget(SocialPoint socialPoint, Vector2 fallbackPoint)
    {
        LeaveCurrentSocialPoint();

        if (socialPoint != null && socialPoint.HasSpace())
        {
            socialPoint.Enter();
            currentSocialPoint = socialPoint;
            targetPosition = socialPoint.GetRandomPosition();
            return;
        }

        targetPosition = GetRandomOffset(fallbackPoint);
    }

    void LeaveCurrentSocialPoint()
    {
        if (currentSocialPoint == null)
            return;

        currentSocialPoint.Leave();
        currentSocialPoint = null;
    }

    void LeaveQueueIfAny()
    {
        if (barQueueSystem != null && inBarQueue)
            barQueueSystem.LeaveQueue(this);

        inBarQueue = false;
        hasQueueTarget = false;
    }

    Vector2 GetRandomOffset(Vector2 point)
    {
        return point + Random.insideUnitCircle * 1.2f;
    }

    Vector2 AvoidOthers()
    {
        Vector2 separation = Vector2.zero;
        int hitCount = Physics2D.OverlapCircle(transform.position, avoidanceRadius, avoidanceFilter, avoidanceBuffer);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D col = avoidanceBuffer[i];
            if (col == null)
                continue;

            if (col.gameObject == gameObject)
                continue;

            GuestMovement otherGuest = null;
            if (col.attachedRigidbody != null)
                otherGuest = col.attachedRigidbody.GetComponent<GuestMovement>();

            if (otherGuest == null)
                otherGuest = col.GetComponent<GuestMovement>();

            if (otherGuest == null)
                continue;

            Vector2 diff = (Vector2)transform.position - (Vector2)col.transform.position;
            float distanceSqr = diff.sqrMagnitude;

            if (distanceSqr > 0.000001f)
                separation += diff / distanceSqr;
        }

        return separation * avoidanceStrength;
    }

    Vector2 GetAvoidanceVector()
    {
        if (avoidanceRecalculationTimer > 0f)
        {
            avoidanceRecalculationTimer -= Time.fixedDeltaTime;
            return cachedAvoidance;
        }

        cachedAvoidance = AvoidOthers();
        avoidanceRecalculationTimer = avoidanceRecalculationInterval;
        return cachedAvoidance;
    }

    void UpdateAnimator()
    {
        if (animator == null)
            return;

        Vector2 velocity = rb.linearVelocity;
        if ((velocity - lastAnimatorVelocity).sqrMagnitude <= 0.0001f)
            return;

        animator.SetFloat("MoveX", velocity.x);
        animator.SetFloat("MoveY", velocity.y);
        animator.SetFloat("Speed", velocity.sqrMagnitude);
        lastAnimatorVelocity = velocity;
    }

    void MoveToExit()
    {
        if (exitPoint == null)
        {
            Despawn();
            return;
        }

        Vector2 toExit = (Vector2)exitPoint.position - (Vector2)transform.position;
        if (toExit.sqrMagnitude <= exitDespawnDistance * exitDespawnDistance)
        {
            Despawn();
            return;
        }

        rb.linearVelocity = toExit.normalized * speed;
    }

    void Despawn()
    {
        if (despawning)
            return;

        despawning = true;
        rb.linearVelocity = Vector2.zero;

        popularitySystem?.UnregisterGuest(personality);

        if (GameManager.Instance != null)
            GameManager.Instance.RemoveGuest();

        Destroy(gameObject);
    }
}
