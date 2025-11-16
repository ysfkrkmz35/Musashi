using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Health))]
public class PlayerController2D : MonoBehaviour
{
    // ===== Movement =====
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;

    // ===== Ground Check =====
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer = 0;

    // ===== Graphics (flip only visuals) =====
    [Header("Graphics")]
    [SerializeField] private Transform graphics; // child with SpriteRenderer + Animator

    // ===== Animation =====
    public enum AnimState { Idle, Run, Attack1, Hit, Dead }

    [Header("Animation Clips")]
    [SerializeField] private AnimationClip idleClip;
    [SerializeField] private AnimationClip runClip;
    [SerializeField] private AnimationClip attack1Clip;
    [SerializeField] private AnimationClip hitClip;
    [SerializeField] private AnimationClip deathClip;

    [Header("Playback Speeds")]
    [SerializeField] private float idleSpeed = 1f;
    [SerializeField] private float runSpeed = 1f;
    [SerializeField] private float attack1Speed = 1f;
    [SerializeField] private float hitSpeed = 1f;
    [SerializeField] private float deathSpeed = 1f;

    // ===== Sword & Attack =====
    [Header("Sword")]
    [SerializeField] private Collider2D swordHitbox;   // BoxCollider2D (IsTrigger)
    [SerializeField] private float attackCooldown = 0.20f;

    [Header("Attack Active Window (0..1)")]
    [Range(0f, 1f)] public float attackActiveStart = 0.20f;
    [Range(0f, 1f)] public float attackActiveEnd = 0.55f;

    // ===== Layers (optional runtime safety) =====
    [Header("Layer Names (optional)")]
    [SerializeField] private string characterLayerName = "Characters";
    [SerializeField] private string hitboxLayerName = "Hitbox";
    [SerializeField] private bool configureLayerMatrixAtRuntime = true;

    // ===== Private =====
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private Health health;

    private PlayableGraph graph;
    // 0=Idle,1=Run,2=Attack1,3=Hit,4=Death
    private AnimationMixerPlayable mixer;
    private AnimationClipPlayable idlePlayable, runPlayable, attackPlayable, hitPlayable, deathPlayable;

    private AnimState state = AnimState.Idle;
    private bool facingRight = true;
    private bool isGrounded;
    private float nextAttackAllowedTime;
    private float inputX; // -1..1
    private float hitEndTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<Health>();

        if (graphics == null && transform.childCount > 0) graphics = transform.GetChild(0);
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        if (swordHitbox) swordHitbox.enabled = false;

        // Physics stability
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

        if (configureLayerMatrixAtRuntime) SetupCollisionMatrix();

        // --- Playables ---
        graph = PlayableGraph.Create("Player2DGraph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        var output = AnimationPlayableOutput.Create(graph, "AnimOutput", animator);

        mixer = AnimationMixerPlayable.Create(graph, 5);
        output.SetSourcePlayable(mixer);

        if (idleClip) { idlePlayable = AnimationClipPlayable.Create(graph, idleClip); idlePlayable.SetSpeed(idleSpeed); graph.Connect(idlePlayable, 0, mixer, 0); }
        if (runClip) { runPlayable = AnimationClipPlayable.Create(graph, runClip); runPlayable.SetSpeed(runSpeed); graph.Connect(runPlayable, 0, mixer, 1); }
        if (attack1Clip) { attackPlayable = AnimationClipPlayable.Create(graph, attack1Clip); attackPlayable.SetSpeed(attack1Speed); attackPlayable.SetDuration(attack1Clip.length); graph.Connect(attackPlayable, 0, mixer, 2); }
        if (hitClip) { hitPlayable = AnimationClipPlayable.Create(graph, hitClip); hitPlayable.SetSpeed(hitSpeed); hitPlayable.SetDuration(hitClip.length); graph.Connect(hitPlayable, 0, mixer, 3); }
        if (deathClip) { deathPlayable = AnimationClipPlayable.Create(graph, deathClip); deathPlayable.SetSpeed(deathSpeed); deathPlayable.SetDuration(deathClip.length); graph.Connect(deathPlayable, 0, mixer, 4); }

        SetWeights(1, 0, 0, 0, 0);
        graph.Play();

        health.OnDamaged += OnDamaged;
        health.OnDied += OnDied;
    }

    private void OnDestroy()
    {
        if (health != null) { health.OnDamaged -= OnDamaged; health.OnDied -= OnDied; }
        if (graph.IsValid()) graph.Destroy();
    }

    private void Update()
    {
        if (state == AnimState.Dead) return;

        // Input
        float x = 0f;
        if (Gamepad.current != null) x = Gamepad.current.leftStick.ReadValue().x;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
        }
        inputX = Mathf.Clamp(x, -1f, 1f);

        bool jumpPressed =
            (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

        bool attackPressed =
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame);

        // Ground
        if (groundCheck) isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // States
        if (state == AnimState.Hit) { UpdateHit(); return; }
        if (state == AnimState.Attack1) { UpdateAttackPlayable(); return; }

        // Locomotion
        if (jumpPressed && isGrounded) Jump();
        if (inputX > 0.05f) Face(true); else if (inputX < -0.05f) Face(false);
        SetState(Mathf.Abs(inputX) > 0.05f ? AnimState.Run : AnimState.Idle);

        if (attackPressed && CanAttack()) StartAttack();
    }

    private void FixedUpdate()
    {
        if (state == AnimState.Idle || state == AnimState.Run)
        {
            var v = rb.linearVelocity; v.x = inputX * moveSpeed; rb.linearVelocity = v;
        }
        else
        {
            var v = rb.linearVelocity; v.x = 0f; rb.linearVelocity = v; // lock during Attack/Hit/Dead
        }
    }

    private void Jump()
    {
        var v = rb.linearVelocity;
        if (v.y < 0f) { v.y = 0f; rb.linearVelocity = v; }
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // ===== Animation state =====
    private void SetState(AnimState next)
    {
        if (state == next) return;
        state = next;

        switch (state)
        {
            case AnimState.Idle: SetWeights(1, 0, 0, 0, 0); if (swordHitbox) swordHitbox.enabled = false; if (!idlePlayable.IsNull()) idlePlayable.SetTime(0); break;
            case AnimState.Run: SetWeights(0, 1, 0, 0, 0); if (swordHitbox) swordHitbox.enabled = false; break;
            case AnimState.Attack1: SetWeights(0, 0, 1, 0, 0); if (!attackPlayable.IsNull()) { attackPlayable.SetTime(0); attackPlayable.SetSpeed(attack1Speed); } if (swordHitbox) swordHitbox.enabled = false; break;
            case AnimState.Hit: SetWeights(0, 0, 0, 1, 0); if (!hitPlayable.IsNull()) { hitPlayable.SetTime(0); hitEndTime = Time.time + Mathf.Max(0.05f, (float)hitPlayable.GetAnimationClip().length / Mathf.Max(0.01f, hitSpeed)); } if (swordHitbox) swordHitbox.enabled = false; break;
            case AnimState.Dead: SetWeights(0, 0, 0, 0, 1); if (!deathPlayable.IsNull()) deathPlayable.SetTime(0); if (swordHitbox) swordHitbox.enabled = false; foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false; break;
        }
    }

    private void SetWeights(float idle, float run, float attack, float hit, float death)
    {
        if (mixer.IsNull()) return;
        mixer.SetInputWeight(0, idleClip ? idle : 0f);
        mixer.SetInputWeight(1, runClip ? run : 0f);
        mixer.SetInputWeight(2, attack1Clip ? attack : 0f);
        mixer.SetInputWeight(3, hitClip ? hit : 0f);
        mixer.SetInputWeight(4, deathClip ? death : 0f);
    }

    // ===== Attack =====
    private bool CanAttack() => (Time.time >= nextAttackAllowedTime) && !attackPlayable.IsNull();

    private void StartAttack()
    {
        nextAttackAllowedTime = Time.time + attackCooldown;
        SetState(AnimState.Attack1);
    }

    private void UpdateAttackPlayable()
    {
        if (attackPlayable.IsNull()) { SetState(Mathf.Abs(inputX) > 0.05f ? AnimState.Run : AnimState.Idle); return; }

        double t = attackPlayable.GetTime();
        double dur = Mathf.Max(0.0001f, attack1Clip.length);
        float norm = Mathf.Clamp01((float)(t / dur));

        if (swordHitbox) swordHitbox.enabled = (norm >= attackActiveStart && norm <= attackActiveEnd);

        if (t >= dur)
        {
            if (swordHitbox) swordHitbox.enabled = false;
            SetState(Mathf.Abs(inputX) > 0.05f ? AnimState.Run : AnimState.Idle);
        }
    }

    // ===== Hit / Death =====
    private void OnDamaged(int current, int max)
    {
        if (state == AnimState.Dead) return;
        // interrupt into Hit
        SetState(AnimState.Hit);
        var v = rb.linearVelocity; v.x = 0f; if (v.y > 0f) v.y *= 0.5f; rb.linearVelocity = v;
    }

    private void UpdateHit()
    {
        if (Time.time >= hitEndTime)
            SetState(Mathf.Abs(inputX) > 0.05f ? AnimState.Run : AnimState.Idle);
    }

    private void OnDied() { SetState(AnimState.Dead); }

    // ===== Facing =====
    private void Face(bool faceRight)
    {
        if (graphics == null) graphics = transform; // fallback
        if (facingRight == faceRight) return;
        facingRight = faceRight;

        var s = graphics.localScale;
        s.x = Mathf.Abs(s.x) * (faceRight ? 1f : -1f);
        graphics.localScale = s;
    }

    // ===== Utilities =====
    private void SetupCollisionMatrix()
    {
        int characters = LayerMask.NameToLayer(characterLayerName);
        int hitbox = LayerMask.NameToLayer(hitboxLayerName);
        if (characters >= 0) Physics2D.IgnoreLayerCollision(characters, characters, true);
        if (hitbox >= 0)
        {
            Physics2D.IgnoreLayerCollision(hitbox, hitbox, true);             // optional
            if (characters >= 0) Physics2D.IgnoreLayerCollision(hitbox, characters, false); // keep hits
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck) Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
