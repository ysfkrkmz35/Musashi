using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.UI;
using Musashi.Core.Combat;

/// <summary>
/// MUSASHI ULTIMATE COMBAT SETUP
///
/// FINAL SYSTEM:
/// - Double-tap to commit attack (W W = attack from up)
/// - Surprise Attack: Stance change → 0.4s window → double-tap = 2x damage!
/// - NO MEDITATION! Only passive regen (0.5/sec)
/// - Successful block REWARDS focus (+15 normal, +25 perfect!)
/// - Failed defense = -35 focus
/// - Executable < 20 focus = No defense!
/// - Smart AI: Learns patterns, feints, predicts
/// </summary>
public class MusashiCombatSetup : EditorWindow
{
    [MenuItem("Musashi/⚔️ ULTIMATE COMBAT SETUP", false, 0)]
    public static void Setup()
    {
        Debug.Log("═══════════════════════════════════════");
        Debug.Log("⚔️ MUSASHI ULTIMATE COMBAT SETUP");
        Debug.Log("═══════════════════════════════════════");

        if (!VerifyScene())
        {
            EditorUtility.DisplayDialog("Error",
                "Player and Enemy not found!\n\nAdd Player and Enemy to scene.",
                "OK");
            return;
        }

        CleanOldSystems();
        SetupCombatSystems();
        SetupVisualUI();
        SetupAnimator();
        SetupHealth();
        FinalizeSetup();

        Debug.Log("═══════════════════════════════════════");
        Debug.Log("✅ SETUP COMPLETE!");
        Debug.Log("═══════════════════════════════════════");
        Debug.Log("");
        Debug.Log("🎮 CONTROLS:");
        Debug.Log("   WASD = Select stance");
        Debug.Log("   Double-tap = Commit attack");
        Debug.Log("");
        Debug.Log("⚡ SURPRISE ATTACK:");
        Debug.Log("   Stance change → 0.4s → double-tap = 2x DAMAGE!");
        Debug.Log("   Cost: -30 focus");
        Debug.Log("   Failed: -20 extra penalty!");
        Debug.Log("");
        Debug.Log("🛡️ BLOCK REWARDS:");
        Debug.Log("   Normal block: +15 focus");
        Debug.Log("   Perfect block (surprise): +25 focus!");
        Debug.Log("");
        Debug.Log("💰 FOCUS ECONOMY:");
        Debug.Log("   Stance change: -5");
        Debug.Log("   Normal attack: -20");
        Debug.Log("   Surprise attack: -30");
        Debug.Log("   Failed defense: -35");
        Debug.Log("   Passive regen: 0.5/sec (VERY SLOW)");
        Debug.Log("   Executable < 20 focus");
        Debug.Log("");
        Debug.Log("🤖 SMART AI:");
        Debug.Log("   - Learns your patterns");
        Debug.Log("   - Predicts attacks (70%)");
        Debug.Log("   - Feint tactics (40%)");
        Debug.Log("   - Counter-attacks (60%)");
        Debug.Log("");
        Debug.Log("PRESS PLAY! ⚔️");

        EditorUtility.DisplayDialog("Setup Ready! ⚔️",
            "MUSASHI ULTIMATE COMBAT!\n\n" +
            "⚡ SURPRISE ATTACK:\n" +
            "Stance → 0.4s → 2x tap = 2x DAMAGE!\n\n" +
            "🛡️ BLOCK REWARDS:\n" +
            "• Normal: +15 focus\n" +
            "• Perfect: +25 focus!\n\n" +
            "💰 COSTS:\n" +
            "• Stance: -5\n" +
            "• Attack: -20\n" +
            "• Surprise: -30\n" +
            "• Failed: -35\n" +
            "• Regen: 0.5/sec\n\n" +
            "🤖 AI:\n" +
            "• Pattern learning\n" +
            "• Smart prediction\n" +
            "• Feint attacks\n\n" +
            "NO MEDITATION!\n" +
            "BLOCK TO GAIN FOCUS!\n\n" +
            "PLAY!",
            "Let's Fight! 🎌");
    }

    static bool VerifyScene()
    {
        return GameObject.Find("Player") != null && GameObject.Find("Enemy") != null;
    }

    static void CleanOldSystems()
    {
        var player = GameObject.Find("Player");
        var enemy = GameObject.Find("Enemy");

        // Remove ALL old components
        if (player != null)
        {
            DestroyImmediate(player.GetComponent<PlayerDuelControllerDirectional>());
            DestroyImmediate(player.GetComponent<DirectionalCombatSystem>());
        }

        if (enemy != null)
        {
            DestroyImmediate(enemy.GetComponent<EnemyDuelControllerDirectional_V2>());
            DestroyImmediate(enemy.GetComponent<EnemyDuelControllerDirectional>());
            DestroyImmediate(enemy.GetComponent<DirectionalCombatSystem>());
            DestroyImmediate(enemy.GetComponent<AttackTelegraphSystem>());
        }

        Debug.Log("[Clean] ✅ Old systems removed");
    }

    static void SetupCombatSystems()
    {
        // PLAYER
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var stance = GetOrAdd<StanceBasedCombatSystem>(player);
            stance.failedDefensePenalty = 35f;
            stance.executionThreshold = 20f;
            EditorUtility.SetDirty(stance);

            var controller = GetOrAdd<PlayerStanceController>(player);
            controller.focusMax = 100f;
            controller.passiveRegenRate = 0.5f;
            controller.attackCost = 20f;
            controller.stanceChangeCost = 5f;
            controller.failedDefenseCost = 35f;
            controller.surpriseAttackCost = 30f;
            controller.successfulBlockReward = 15f;
            controller.perfectBlockReward = 25f;
            controller.surpriseAttackWindow = 0.4f;
            controller.surpriseAttackDamageMultiplier = 2.0f;
            controller.surpriseAttackPenalty = 20f;
            controller.doubleTapWindow = 0.5f;
            controller.attackCooldown = 0.8f;
            controller.lockedX = 0f;
            EditorUtility.SetDirty(controller);
        }

        // ENEMY - SMART AI
        var enemy = GameObject.Find("Enemy");
        if (enemy != null)
        {
            var stance = GetOrAdd<StanceBasedCombatSystem>(enemy);
            stance.failedDefensePenalty = 35f;
            stance.executionThreshold = 20f;
            EditorUtility.SetDirty(stance);

            var controller = GetOrAdd<EnemyStanceController>(enemy);
            if (player != null) controller.player = player.transform;
            controller.focusMax = 100f;
            controller.passiveRegenRate = 0.5f;
            controller.attackCost = 20f;
            controller.stanceChangeCost = 5f;
            controller.failedDefenseCost = 35f;
            controller.successfulBlockReward = 15f;
            controller.perfectBlockReward = 25f;
            controller.thinkInterval = new Vector2(1f, 3f);
            controller.attackCooldown = 1f;
            controller.baseAggression = 0.5f;
            controller.predictionSkill = 0.7f;
            controller.feintChance = 0.4f;
            controller.counterAttackChance = 0.6f;
            controller.lockedX = 2.5f;
            EditorUtility.SetDirty(controller);
        }

        Debug.Log("[Combat] ✅ Systems configured - ULTIMATE MOD!");
    }

    static void SetupVisualUI()
    {
        var canvas = FindOrCreateCanvas();
        CreateStanceIndicatorUI(canvas.transform);
        CreateFocusBars(canvas.transform);
        Debug.Log("[UI] ✅ Visual UI created");
    }

    static Canvas FindOrCreateCanvas()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null) return canvas;

        var obj = new GameObject("Canvas");
        canvas = obj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        obj.AddComponent<CanvasScaler>();
        obj.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static void CreateStanceIndicatorUI(Transform canvas)
    {
        GameObject uiObj = GameObject.Find("StanceIndicatorUI");
        if (uiObj == null)
        {
            uiObj = new GameObject("StanceIndicatorUI");
            uiObj.transform.SetParent(canvas, false);
        }

        var ui = GetOrAdd<StanceIndicatorUI>(uiObj);
        CreateIndicatorSet(canvas, "PlayerIndicators", new Vector2(-300, 0), ui, true);
        CreateIndicatorSet(canvas, "EnemyIndicators", new Vector2(300, 0), ui, false);
        EditorUtility.SetDirty(ui);
    }

    static void CreateIndicatorSet(Transform parent, string name, Vector2 pos, StanceIndicatorUI ui, bool isPlayer)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(200, 200);

        var up = CreateArrow(panel.transform, "Up", new Vector2(0, 60), "↑");
        var down = CreateArrow(panel.transform, "Down", new Vector2(0, -60), "↓");
        var left = CreateArrow(panel.transform, "Left", new Vector2(-60, 0), "←");
        var right = CreateArrow(panel.transform, "Right", new Vector2(60, 0), "→");

        var upDef = CreateArrow(panel.transform, "UpDef", new Vector2(0, 90), "▲");
        var downDef = CreateArrow(panel.transform, "DownDef", new Vector2(0, -90), "▼");
        var leftDef = CreateArrow(panel.transform, "LeftDef", new Vector2(-90, 0), "◄");
        var rightDef = CreateArrow(panel.transform, "RightDef", new Vector2(90, 0), "►");

        if (isPlayer)
        {
            ui.playerAttackUp = up; ui.playerAttackDown = down;
            ui.playerAttackLeft = left; ui.playerAttackRight = right;
            ui.playerDefenseUp = upDef; ui.playerDefenseDown = downDef;
            ui.playerDefenseLeft = leftDef; ui.playerDefenseRight = rightDef;
        }
        else
        {
            ui.enemyAttackUp = up; ui.enemyAttackDown = down;
            ui.enemyAttackLeft = left; ui.enemyAttackRight = right;
            ui.enemyDefenseUp = upDef; ui.enemyDefenseDown = downDef;
            ui.enemyDefenseLeft = leftDef; ui.enemyDefenseRight = rightDef;
        }
    }

    static Image CreateArrow(Transform parent, string name, Vector2 pos, string symbol)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(40, 40);

        var img = obj.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.3f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        var textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = textRt.offsetMax = Vector2.zero;

        var text = textObj.AddComponent<Text>();
        text.text = symbol;
        text.fontSize = 30;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        return img;
    }

    static void CreateFocusBars(Transform canvas)
    {
        GameObject barObj = GameObject.Find("FocusBar");
        if (barObj == null)
        {
            barObj = new GameObject("FocusBar");
            barObj.transform.SetParent(canvas, false);
        }

        var focusBar = GetOrAdd<FocusBar>(barObj);

        // Player bar (left)
        var playerSlider = CreateFocusSlider(canvas, "PlayerFocusSlider", new Vector2(-400, 250), Color.cyan);
        focusBar.playerFocus = playerSlider;

        // Enemy bar (right)
        var enemySlider = CreateFocusSlider(canvas, "EnemyFocusSlider", new Vector2(400, 250), Color.red);
        focusBar.enemyFocus = enemySlider;

        EditorUtility.SetDirty(focusBar);
    }

    static Slider CreateFocusSlider(Transform parent, string name, Vector2 pos, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(200, 20);

        var slider = obj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        // Create background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(obj.transform, false);
        var bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Create fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(obj.transform, false);
        var fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.offsetMin = fillAreaRt.offsetMax = Vector2.zero;

        // Create fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = color;

        slider.fillRect = fillRt;
        slider.targetGraphic = fillImg;

        return slider;
    }

    static void SetupAnimator()
    {
        string path = "Assets/MusashiDuelController.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);

        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("attackLight", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("attackHeavy", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("attackDirection", AnimatorControllerParameterType.Int);
            controller.AddParameter("hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("die", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("executionAttack", AnimatorControllerParameterType.Trigger);
            AssetDatabase.SaveAssets();
        }

        var player = GameObject.Find("Player");
        var enemy = GameObject.Find("Enemy");

        if (player != null)
        {
            var anim = GetOrAdd<Animator>(player);
            anim.runtimeAnimatorController = controller;
            EditorUtility.SetDirty(anim);
        }

        if (enemy != null)
        {
            var anim = GetOrAdd<Animator>(enemy);
            anim.runtimeAnimatorController = controller;
            EditorUtility.SetDirty(anim);
        }

        Debug.Log("[Animator] ✅ Configured");
    }

    static void SetupHealth()
    {
        var player = GameObject.Find("Player");
        var enemy = GameObject.Find("Enemy");

        if (player != null)
        {
            var health = GetOrAdd<HealthB>(player);
            health.team = Team.Player;
            health.maxHP = 100f;
            health.currentHP = 100f;
            EditorUtility.SetDirty(health);
        }

        if (enemy != null)
        {
            var health = GetOrAdd<HealthB>(enemy);
            health.team = Team.Enemy;
            health.maxHP = 100f;
            health.currentHP = 100f;
            EditorUtility.SetDirty(health);
        }

        Debug.Log("[Health] ✅ Configured");
    }

    static void FinalizeSetup()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(1.25f, 2f, -10f);
            cam.transform.rotation = Quaternion.identity;
            EditorUtility.SetDirty(cam.transform);
        }

        Debug.Log("[Final] ✅ Complete");
    }

    static T GetOrAdd<T>(GameObject obj) where T : Component
    {
        var comp = obj.GetComponent<T>();
        if (comp == null) comp = obj.AddComponent<T>();
        return comp;
    }
}
