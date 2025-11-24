using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds simple slider-based health bars for the player and enemy at runtime.
/// Ensures a Canvas exists and wires the HealthBarUI component automatically.
/// </summary>
public class HealthUIBootstrapper : MonoBehaviour
{
    [System.Serializable]
    private class HealthBarConfig
    {
        public string label = "Health";
        public Health health;
        public bool followTarget = false;
        public Vector2 anchoredPosition = new Vector2(160f, -40f);
        public Vector2 size = new Vector2(220f, 24f);
        public Vector3 worldOffset = new Vector3(0f, 1.6f, 0f);
        public Color fullColor = Color.green;
        public Color lowColor = Color.red;
        [Range(0.05f, 0.9f)] public float lowThreshold = 0.3f;
        public bool hideWhenFull = false;
        public bool hideWhenDead = true;
    }

    [Header("Assignments")]
    [SerializeField] private HealthBarConfig playerConfig = new HealthBarConfig
    {
        label = "Player",
        anchoredPosition = new Vector2(180f, -40f),
        followTarget = false,
        worldOffset = new Vector3(0f, 1.4f, 0f)
    };

    [SerializeField] private HealthBarConfig enemyConfig = new HealthBarConfig
    {
        label = "Enemy",
        anchoredPosition = new Vector2(180f, -80f),
        followTarget = true,
        worldOffset = new Vector3(0f, 1.8f, 0f)
    };

    [Header("Canvas Settings")]
    [SerializeField] private string canvasName = "MusashiUIRoot";
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);

    private Canvas cachedCanvas;

    private void Awake()
    {
        TryAutoAssign(ref playerConfig.health, "Player");
        TryAutoAssign(ref enemyConfig.health, "Enemy");
    }

    private void Start()
    {
        cachedCanvas = EnsureCanvas();

        CreateBar(playerConfig);
        CreateBar(enemyConfig);
    }

    private Canvas EnsureCanvas()
    {
        var existing = GameObject.Find(canvasName);
        if (existing != null)
        {
            var existingCanvas = existing.GetComponent<Canvas>();
            if (existingCanvas != null) return existingCanvas;
        }

        var canvasGO = new GameObject(canvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private void CreateBar(HealthBarConfig config)
    {
        if (cachedCanvas == null || config == null || config.health == null)
            return;

        var root = new GameObject($"{config.label} Health Bar", typeof(RectTransform), typeof(CanvasRenderer), typeof(CanvasGroup));
        var rect = root.GetComponent<RectTransform>();
        rect.SetParent(cachedCanvas.transform, false);
        rect.sizeDelta = config.size;

        if (config.followTarget)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
        else
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = config.anchoredPosition;
        }

        var canvasGroup = root.GetComponent<CanvasGroup>();

        // Slider + visuals
        var slider = root.AddComponent<Slider>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.wholeNumbers = true;
        slider.transition = Selectable.Transition.None;
        slider.navigation = new Navigation { mode = Navigation.Mode.None };

        var background = CreateImage("Background", root.transform, new Vector2(0f, 0f), new Vector2(1f, 1f));
        background.color = new Color(0f, 0f, 0f, 0.5f);

        var fillAreaGO = new GameObject("Fill Area", typeof(RectTransform));
        var fillAreaRect = fillAreaGO.GetComponent<RectTransform>();
        fillAreaRect.SetParent(root.transform, false);
        fillAreaRect.anchorMin = new Vector2(0f, 0f);
        fillAreaRect.anchorMax = new Vector2(1f, 1f);
        fillAreaRect.offsetMin = new Vector2(4f, 4f);
        fillAreaRect.offsetMax = new Vector2(-4f, -4f);

        var fillImage = CreateImage("Fill", fillAreaGO.transform, new Vector2(0f, 0f), new Vector2(1f, 1f));
        fillImage.color = config.fullColor;

        slider.targetGraphic = background;
        slider.fillRect = fillImage.rectTransform;
        slider.handleRect = null;
        slider.minValue = 0f;
        slider.maxValue = config.health.GetMaxHP();
        slider.value = config.health.CurrentHP;

        var text = CreateText("Label", root.transform);
        text.text = $"{config.label}: {config.health.CurrentHP}/{config.health.GetMaxHP()}";

        var ui = root.AddComponent<HealthBarUI>();
        ui.Initialize(config.health, slider, text, canvasGroup, config.followTarget ? config.health.transform : null, config.worldOffset);
        ui.SetColors(config.fullColor, config.lowColor, config.lowThreshold);
        ui.SetVisibilityOptions(config.hideWhenFull, config.hideWhenDead);

        if (!config.followTarget)
        {
            ui.SetFollowTarget(null, config.worldOffset);
        }
    }

    private Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return go.GetComponent<Image>();
    }

    private Text CreateText(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var text = go.GetComponent<Text>();
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 18;
        text.color = Color.white;
        return text;
    }

    private void TryAutoAssign(ref Health target, string tag)
    {
        if (target != null) return;
        var tagged = GameObject.FindWithTag(tag);
        if (tagged != null)
            target = tagged.GetComponent<Health>();
    }
}