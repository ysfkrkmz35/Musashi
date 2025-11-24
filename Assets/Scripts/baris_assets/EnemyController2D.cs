using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Health))]
public class EnemyController2D : MonoBehaviour
{
    public enum State { Idle, Run, Attack, Hit, Dead }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float stopDistance = 0.1f;

    [Header("AI")]
    [SerializeField] private float aggroRange = 8f;
    [SerializeField] private float attackRange = 1.15f;
    [SerializeField] private float attackCooldown = 0.8f;
    [Tooltip("Mirror the weapon hitbox automatically when the enemy flips direction.")]
    [SerializeField] private bool mirrorWeaponHitbox = true;
    [SerializeField, Tooltip("Horizontal distance (local units) for the weapon hitbox when facing right.")] private float weaponHitboxForwardOffset = 0.75f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer = 0;

    [Header("Animation Clips (Fantasy Warrior)")]
    [SerializeField] private AnimationClip idleClip;
    [SerializeField] private AnimationClip runClip;       // or "walk" if that�s what the pack uses
    [SerializeField] private AnimationClip attackClip;
    [SerializeField] private AnimationClip hitClip;
    [SerializeField] private AnimationClip deathClip;     // optional

    [Header("Playback Speeds")]
    [SerializeField] private float idleSpeed = 1f;
    [SerializeField] private float runSpeed = 1f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float hitSpeed = 1f;
    [SerializeField] private float deathSpeed = 1f;

    [Header("Attack Hitbox")]
    [SerializeField] private Collider2D weaponHitbox;     // BoxCollider2D (IsTrigger), child
    [Tooltip("Normalized window on attack clip when the blade is active.")]
    [Range(0f, 1f)] public float attackActiveStart = 0.25f;
    [Range(0f, 1f)] public float attackActiveEnd = 0.55f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private Health health;
    private Transform player;

    private PlayableGraph graph;
    private AnimationMixerPlayable mixer; // 0 idle, 1 run, 2 attack, 3 hit, 4 death
    private AnimationClipPlayable idlePl, runPl, atkPl, hitPl, deathPl;

    private State state = State.Idle;
    private bool facingRight = true;
    private bool grounded;
    private float nextAttackTime;
    private Transform weaponHitboxTransform;
    private Vector3 weaponHitboxDefaultLocalPos;
    private Vector3 weaponHitboxDefaultLocalScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<Health>();

        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        if (weaponHitbox)
        {
            weaponHitbox.enabled = false;
            weaponHitboxTransform = weaponHitbox.transform;
            weaponHitboxDefaultLocalPos = weaponHitboxTransform.localPosition;
            weaponHitboxDefaultLocalScale = weaponHitboxTransform.localScale;
            if (weaponHitboxForwardOffset <= 0f)
                weaponHitboxForwardOffset = Mathf.Abs(weaponHitboxDefaultLocalPos.x);
        }

        // Build Playables graph
        graph = PlayableGraph.Create("EnemyGraph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        var output = AnimationPlayableOutput.Create(graph, "AnimOut", animator);

        mixer = AnimationMixerPlayable.Create(graph, 5);
        output.SetSourcePlayable(mixer);

        if (idleClip) { idlePl = AnimationClipPlayable.Create(graph, idleClip); idlePl.SetSpeed(idleSpeed); graph.Connect(idlePl, 0, mixer, 0); }
        if (runClip) { runPl = AnimationClipPlayable.Create(graph, runClip); runPl.SetSpeed(runSpeed); graph.Connect(runPl, 0, mixer, 1); }
        if (attackClip) { atkPl = AnimationClipPlayable.Create(graph, attackClip); atkPl.SetSpeed(attackSpeed); graph.Connect(atkPl, 0, mixer, 2); }
        if (hitClip) { hitPl = AnimationClipPlayable.Create(graph, hitClip); hitPl.SetSpeed(hitSpeed); graph.Connect(hitPl, 0, mixer, 3); }
        if (deathClip) { deathPl = AnimationClipPlayable.Create(graph, deathClip); deathPl.SetSpeed(deathSpeed); graph.Connect(deathPl, 0, mixer, 4); }

        SetWeights(1, 0, 0, 0, 0);
        graph.Play();

        // hook Health events
        health.OnDamaged += OnDamaged;
        health.OnDied += OnDied;

        // try find player by tag
        var p = GameObject.FindWithTag("Player");
        if (p) player = p.transform;
    }

    private void OnDestroy()
    {
        if (graph.IsValid()) graph.Destroy();
        if (health != null)
        {
            health.OnDamaged -= OnDamaged;
            health.OnDied -= OnDied;
        }
    }

    private void Update()
    {
        if (!player)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) player = p.transform;
        }

        if (groundCheck)
            grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        switch (state)
        {
            case State.Idle:
            case State.Run:
                LocomotionUpdate();
                TryAttack();
                break;

            case State.Attack:
                UpdateAttack();
                break;

            case State.Hit:
                UpdateHit();
                break;

            case State.Dead:
                // do nothing
                break;
        }
    }

    private void FixedUpdate()
    {
        if (state == State.Idle || state == State.Run)
        {
            // horizontal move is set by LocomotionUpdate via desiredX
            rb.linearVelocity = new Vector2(desiredX * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            // lock horizontal during attack/hit/death
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    // ---------- Locomotion & Facing ----------
    private float desiredX;

    private void LocomotionUpdate()
    {
        desiredX = 0f;

        if (player)
        {
            float dx = player.position.x - transform.position.x;
            float ax = Mathf.Abs(dx);

            if (ax <= stopDistance)
            {
                desiredX = 0f;
            }
            else if (ax <= aggroRange && ax > attackRange)
            {
                desiredX = Mathf.Sign(dx); // chase
            }

            if (dx > 0.05f) Face(true);
            else if (dx < -0.05f) Face(false);
        }

        if (Mathf.Abs(desiredX) > 0.05f) SetState(State.Run);
        else SetState(State.Idle);
    }

private void Face(bool faceRight)
    {
        if (facingRight == faceRight) return;
        facingRight = faceRight;
        
        // Flip sprite renderer
        if (sr != null)
        {
            sr.flipX = !faceRight;
        }

        if (mirrorWeaponHitbox)
            UpdateWeaponHitboxOrientation(faceRight);
    }

    private void UpdateWeaponHitboxOrientation(bool faceRight)
    {
        if (weaponHitboxTransform == null) return;

        var pos = weaponHitboxDefaultLocalPos;
        float offset = weaponHitboxForwardOffset > 0f ? weaponHitboxForwardOffset : Mathf.Abs(weaponHitboxDefaultLocalPos.x);
        pos.x = offset * (faceRight ? 1f : -1f);
        weaponHitboxTransform.localPosition = pos;

        var scale = weaponHitboxDefaultLocalScale;
        scale.x = Mathf.Abs(scale.x) * (faceRight ? 1f : -1f);
        weaponHitboxTransform.localScale = scale;
    }

    // ---------- Attack ----------
    private bool CanAttackNow()
    {
        if (!player || atkPl.IsNull()) return false;
        if (Time.time < nextAttackTime) return false;

        float ax = Mathf.Abs(player.position.x - transform.position.x);
        return ax <= attackRange;
    }

    private void TryAttack()
    {
        if (CanAttackNow())
        {
            nextAttackTime = Time.time + attackCooldown;
            SetState(State.Attack);
            if (!atkPl.IsNull()) atkPl.SetTime(0);
            if (weaponHitbox) weaponHitbox.enabled = false; // start OFF
        }
    }

    private void UpdateAttack()
    {
        if (atkPl.IsNull()) { SetState(State.Idle); return; }

        double t = atkPl.GetTime();
        double dur = Mathf.Max(0.0001f, attackClip.length);
        float norm = Mathf.Clamp01((float)(t / dur));

        if (weaponHitbox)
            weaponHitbox.enabled = (norm >= attackActiveStart && norm <= attackActiveEnd);

        if (t >= dur)
        {
            if (weaponHitbox) weaponHitbox.enabled = false;
            SetState(State.Idle);
        }
    }

    // ---------- Hit / Damage ----------
    private float hitEndTime;

    private void OnDamaged(int current, int max)
    {
        if (state == State.Dead) return;

        if (!hitPl.IsNull())
        {
            SetState(State.Hit);
            hitPl.SetTime(0);
            hitEndTime = Time.time + Mathf.Max(0.05f, (float)hitPl.GetAnimationClip().length / hitSpeed);
        }
        else
        {
            // no hit clip -> just flinch briefly
            hitEndTime = Time.time + 0.15f;
        }
    }

    private void UpdateHit()
    {
        if (Time.time >= hitEndTime)
        {
            SetState(State.Idle);
        }
    }

    private void OnDied()
    {
        SetState(State.Dead);
        if (!deathPl.IsNull())
        {
            deathPl.SetTime(0);
        }

        // disable combat & collisions
        if (weaponHitbox) weaponHitbox.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;

        // optional: destroy after death animation
        if (!deathPl.IsNull())
            Destroy(gameObject, deathPl.GetAnimationClip().length / Mathf.Max(0.01f, deathSpeed));
        else
            Destroy(gameObject, 0.25f);
    }

    // ---------- State & Weights ----------
    private void SetState(State next)
    {
        if (state == next) return;
        state = next;

        switch (state)
        {
            case State.Idle: SetWeights(1, 0, 0, 0, 0); break;
            case State.Run: SetWeights(0, 1, 0, 0, 0); break;
            case State.Attack: SetWeights(0, 0, 1, 0, 0); break;
            case State.Hit: SetWeights(0, 0, 0, 1, 0); break;
            case State.Dead: SetWeights(0, 0, 0, 0, 1); break;
        }
    }

    private void SetWeights(float idle, float run, float atk, float hit, float death)
    {
        if (mixer.IsNull()) return;
        mixer.SetInputWeight(0, idleClip ? idle : 0f);
        mixer.SetInputWeight(1, runClip ? run : 0f);
        mixer.SetInputWeight(2, attackClip ? atk : 0f);
        mixer.SetInputWeight(3, hitClip ? hit : 0f);
        mixer.SetInputWeight(4, deathClip ? death : 0f);
    }

    private void OnDrawGizmosSelected()
    {
        // simple viz
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if (groundCheck) Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
