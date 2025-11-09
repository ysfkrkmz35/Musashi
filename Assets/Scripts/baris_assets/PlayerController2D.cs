using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
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

    // ===== Animation Clips (assign your .anim made from sprite sheets) =====
    public enum AnimState { Idle, Run, Attack1 }

    [Header("Animation Clips")]
    [SerializeField] private AnimationClip idleClip;
    [SerializeField] private AnimationClip runClip;
    [SerializeField] private AnimationClip attack1Clip;

    [Header("Playback Speeds")]
    [SerializeField] private float idleSpeed = 1f;
    [SerializeField] private float runSpeed = 1f;
    [SerializeField] private float attack1Speed = 1f;

    // ===== Sword & Attack =====
    [Header("Sword")]
    [SerializeField] private Collider2D swordHitbox;   // BoxCollider2D (IsTrigger)
    [Tooltip("Seconds of cooldown between attack button presses.")]
    [SerializeField] private float attackCooldown = 0.20f;

    [Header("Attack Active Window (normalized 0..1 on Attack1 clip)")]
    [Range(0f, 1f)] public float attackActiveStart = 0.20f;
    [Range(0f, 1f)] public float attackActiveEnd = 0.55f;

    // ===== Private (runtime) =====
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;

    private PlayableGraph graph;
    private AnimationMixerPlayable mixer;        // 0=Idle, 1=Run, 2=Attack1
    private AnimationClipPlayable idlePlayable;
    private AnimationClipPlayable runPlayable;
    private AnimationClipPlayable attackPlayable;

    private AnimState state = AnimState.Idle;
    private bool facingRight = true;
    private bool isGrounded;
    private float nextAttackAllowedTime;
    private float inputX; // -1..1

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; // ensure updates even offscreen
        if (swordHitbox) swordHitbox.enabled = false;

        // --- Build PlayableGraph (no Animator Controller needed) ---
        graph = PlayableGraph.Create("Player2DGraph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var output = AnimationPlayableOutput.Create(graph, "AnimOutput", animator);

        // Create a 3-input mixer (Idle, Run, Attack1)
        mixer = AnimationMixerPlayable.Create(graph, 3);
        output.SetSourcePlayable(mixer);

        if (idleClip)
        {
            idlePlayable = AnimationClipPlayable.Create(graph, idleClip);
            idlePlayable.SetApplyFootIK(false);
            idlePlayable.SetSpeed(idleSpeed);
            graph.Connect(idlePlayable, 0, mixer, 0);
            mixer.SetInputWeight(0, 0f);
        }

        if (runClip)
        {
            runPlayable = AnimationClipPlayable.Create(graph, runClip);
            runPlayable.SetApplyFootIK(false);
            runPlayable.SetSpeed(runSpeed);
            graph.Connect(runPlayable, 0, mixer, 1);
            mixer.SetInputWeight(1, 0f);
        }

        if (attack1Clip)
        {
            attackPlayable = AnimationClipPlayable.Create(graph, attack1Clip);
            attackPlayable.SetApplyFootIK(false);
            attackPlayable.SetSpeed(attack1Speed);
            attackPlayable.SetDuration(attack1Clip.length); // treat as one-shot
            graph.Connect(attackPlayable, 0, mixer, 2);
            mixer.SetInputWeight(2, 0f);
        }

        graph.Play();
        SetState(AnimState.Idle, instant: true);
    }

    private void OnDestroy()
    {
        if (graph.IsValid()) graph.Destroy();
    }

    private void Update()
    {
        // ===== INPUT (Input System, no actions asset) =====
        float x = 0f;

        if (Gamepad.current != null)
            x = Gamepad.current.leftStick.ReadValue().x;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
        }

        inputX = Mathf.Clamp(x, -1f, 1f);

        bool jumpPressed =
            (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

        // ✅ Attack on Left Mouse Button (also keep keyboard J + gamepad West as backups)
        bool attackPressed =
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame);

        // ===== GROUND CHECK =====
        if (groundCheck)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // ===== MOVE / JUMP =====
        if (jumpPressed && isGrounded) Jump();

        // Face direction if input provided
        if (inputX > 0.05f) Face(true);
        else if (inputX < -0.05f) Face(false);

        // ===== STATE / CLIPS =====
        switch (state)
        {
            case AnimState.Idle:
                if (Mathf.Abs(inputX) > 0.05f) SetState(AnimState.Run);
                if (attackPressed && CanAttack()) StartAttack();
                break;

            case AnimState.Run:
                if (Mathf.Abs(inputX) <= 0.05f) SetState(AnimState.Idle);
                if (attackPressed && CanAttack()) StartAttack();
                break;

            case AnimState.Attack1:
                UpdateAttackPlayable();
                break;
        }
    }

    private void FixedUpdate()
    {
        // Use linearVelocity (Unity 6)
        Vector2 v = rb.linearVelocity;
        v.x = inputX * moveSpeed;
        rb.linearVelocity = v;
    }

    private void Jump()
    {
        // Crisp jump (cancel downward fall)
        Vector2 v = rb.linearVelocity;
        if (v.y < 0f)
        {
            v.y = 0f;
            rb.linearVelocity = v;
        }

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // ====== Animation State Management (via mixer weights) ======
    private void SetState(AnimState next, bool instant = false)
    {
        state = next;

        switch (state)
        {
            case AnimState.Idle:
                SetWeights(idle: 1f, run: 0f, attack: 0f);
                if (!idlePlayable.IsNull()) idlePlayable.SetTime(0);
                if (swordHitbox) swordHitbox.enabled = false;
                break;

            case AnimState.Run:
                SetWeights(idle: 0f, run: 1f, attack: 0f);
                if (swordHitbox) swordHitbox.enabled = false;
                break;

            case AnimState.Attack1:
                SetWeights(idle: 0f, run: 0f, attack: 1f);
                if (!attackPlayable.IsNull())
                {
                    attackPlayable.SetTime(0);
                    attackPlayable.SetSpeed(attack1Speed);
                }
                if (swordHitbox) swordHitbox.enabled = false; // start OFF
                break;
        }
    }

    private void SetWeights(float idle, float run, float attack)
    {
        if (mixer.IsNull()) return;
        mixer.SetInputWeight(0, idleClip ? idle : 0f);
        mixer.SetInputWeight(1, runClip ? run : 0f);
        mixer.SetInputWeight(2, attack1Clip ? attack : 0f);
    }

    private bool CanAttack()
    {
        return Time.time >= nextAttackAllowedTime && !attackPlayable.IsNull();
    }

    private void StartAttack()
    {
        nextAttackAllowedTime = Time.time + attackCooldown;
        SetState(AnimState.Attack1);
        // Debug.Log("Attack start"); // uncomment if you want to verify input
    }

    private void UpdateAttackPlayable()
    {
        if (attackPlayable.IsNull()) { SetState(AnimState.Idle); return; }

        double t = attackPlayable.GetTime();                  // current time (seconds)
        double dur = Mathf.Max(0.0001f, attack1Clip.length);  // clip duration
        float norm = Mathf.Clamp01((float)(t / dur));         // normalized 0..1

        // Toggle hitbox while in active window
        if (swordHitbox)
            swordHitbox.enabled = (norm >= attackActiveStart && norm <= attackActiveEnd);

        // End of clip -> return to locomotion
        if (t >= dur)
        {
            if (swordHitbox) swordHitbox.enabled = false;
            if (Mathf.Abs(inputX) > 0.05f) SetState(AnimState.Run);
            else SetState(AnimState.Idle);
        }
    }

    private void Face(bool faceRight)
    {
        if (facingRight == faceRight) return;
        facingRight = faceRight;

        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    private void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
