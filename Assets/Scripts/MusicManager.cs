using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class MusicManagerPro : MonoBehaviour
{
    // ===== Singleton =====
    public static MusicManagerPro Instance { get; private set; }

    // ===== Types =====
    [Serializable]
    public class Track
    {
        [Tooltip("Görünen ad (PlayByName ile çağırmak için)")]
        public string name;

        [Header("Ana Parça")]
        public AudioClip clip;

        [Header("Opsiyonel: Intro+Loop (gapless)")]
        [Tooltip("Önce intro çalar, biter bitmez loopClip sonsuza kadar döner.")]
        public AudioClip introClip;
        public AudioClip loopClip;

        [Header("Başlangıç Zamanı (sn)")]
        public float startTime = 0f;

        public bool IsIntroLoopMode => introClip && loopClip;
        public override string ToString() => string.IsNullOrEmpty(name) ? (clip ? clip.name : "(null)") : name;
    }

    [Serializable]
    public struct SceneMusic
    {
        public string sceneName;
        public string trackNameOrClipName; // Track.name ya da AudioClip.name
        public bool playOnLoad;
    }

    // ===== Inspector =====
    [Header("Playlist")]
    public List<Track> playlist = new List<Track>();
    public bool shuffle = false;
    public bool loopPlaylist = true;

    [Header("Fade & Volume")]
    [Range(0f, 10f)] public float defaultCrossfade = 1.5f;
    [Range(0f, 1f)] public float startVolume = 0.8f;
    [Tooltip("Fade eğrisi (x: zaman 0-1, y: ses 0-1)")]
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0,0,1,1);

    [Header("AudioMixer (Opsiyonel)")]
    public AudioMixer mixer;
    [Tooltip("Mixer’daki exposed parametre (örn. MusicVolume)")]
    public string mixerVolumeParam = "MusicVolume"; // dB
    [Tooltip("SFX zamanı ducking için hedef snapshot (örn. MusicDucked)")]
    public AudioMixerSnapshot duckSnapshot;
    [Tooltip("Normal hâl için snapshot (örn. MusicNormal)")]
    public AudioMixerSnapshot normalSnapshot;
    [Tooltip("Snapshot geçiş süresi (sn)")]
    public float snapshotBlend = 0.2f;

    [Header("Auto Play")]
    public bool autoPlayOnStart = true;

    [Header("Scene → Music Mapping (opsiyonel)")]
    public List<SceneMusic> sceneMap = new List<SceneMusic>();

    [Header("Diğer")]
    public bool persistPlaybackPosition = true;
    public bool showDebugHotkey = true;
    public KeyCode debugToggleKey = KeyCode.F9;

    // ===== Events =====
    public event Action<Track> OnTrackWillChange;
    public event Action<Track> OnTrackChanged;

    // ===== Internals =====
    private AudioSource a, b, active, idle;
    private int currentIndex = -1;
    private Coroutine crossfadeCo;
    private const string VPref = "MusicManagerPro.Volume01";
    private const string PosPrefPrefix = "MusicManagerPro.Pos.";  // + trackKey
    private bool isIntroPhase = false;
    private float cachedLinearVolume;

    // ===== Lifecycle =====
    private void Awake()
    {
        // Singleton
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Double-source
        a = gameObject.AddComponent<AudioSource>();
        b = gameObject.AddComponent<AudioSource>();
        foreach (var s in new[] { a, b })
        {
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            s.loop = false; // Loop'u parça/introLoop mantığıyla biz yönetiyoruz
        }
        active = a; idle = b;

        // Volume restore
        float vol = PlayerPrefs.GetFloat(VPref, startVolume);
        SetVolume(vol);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (autoPlayOnStart && playlist.Count > 0)
        {
            if (shuffle) currentIndex = UnityEngine.Random.Range(0, playlist.Count);
            else currentIndex = 0;
            PlayIndex(currentIndex, 0f);
        }
    }

    private void Update()
    {
        if (showDebugHotkey && Input.GetKeyDown(debugToggleKey))
            debugPanel = !debugPanel;

        // Intro -> Loop geçişi
        if (isIntroPhase && active && !active.isPlaying)
        {
            // Intro bitti, loop’a kesintisiz geç
            var t = playlist[currentIndex];
            isIntroPhase = false;
            if (t.loopClip)
            {
                active.clip = t.loopClip;
                active.time = 0f;
                active.loop = true;
                active.Play();
            }
        }

        // Track bitişi (introLoop değilse)
        if (!isIntroPhase && active && !active.loop && !active.isPlaying)
            AutoAdvance();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ===== Scene mapping =====
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var map = sceneMap.FirstOrDefault(m => string.Equals(m.sceneName, scene.name, StringComparison.Ordinal));
        if (!string.IsNullOrEmpty(map.sceneName) && map.playOnLoad)
        {
            PlayByName(map.trackNameOrClipName);
        }
    }

    // ===== Public API =====

    public void PlayByName(string trackNameOrClipName, float fade = -1f)
    {
        if (string.IsNullOrEmpty(trackNameOrClipName)) return;

        int idx = playlist.FindIndex(t =>
            t != null &&
            (string.Equals(t.name, trackNameOrClipName, StringComparison.Ordinal) ||
             (t.clip && string.Equals(t.clip.name, trackNameOrClipName, StringComparison.Ordinal)) ||
             (t.loopClip && string.Equals(t.loopClip.name, trackNameOrClipName, StringComparison.Ordinal)) ||
             (t.introClip && string.Equals(t.introClip.name, trackNameOrClipName, StringComparison.Ordinal))
            ));

        if (idx < 0)
        {
            Debug.LogWarning($"[MusicManagerPro] '{trackNameOrClipName}' bulunamadı.");
            return;
        }
        PlayIndex(idx, fade);
    }

    public void PlayIndex(int index, float fade = -1f)
    {
        if (playlist.Count == 0) return;
        index = Mathf.Clamp(index, 0, playlist.Count - 1);

        var track = playlist[index];
        if (track == null || (track.clip == null && !track.IsIntroLoopMode))
        {
            Debug.LogWarning("[MusicManagerPro] Track/Clip eksik.");
            return;
        }

        currentIndex = index;
        OnTrackWillChange?.Invoke(track);

        // Crossfade başlat
        float f = fade < 0 ? defaultCrossfade : fade;
        if (crossfadeCo != null) StopCoroutine(crossfadeCo);
        crossfadeCo = StartCoroutine(CoCrossfade(track, f));
    }

    public void Next(float fade = -1f)
    {
        if (playlist.Count == 0) return;
        if (shuffle)
        {
            int next;
            if (playlist.Count == 1) next = 0;
            else { do next = UnityEngine.Random.Range(0, playlist.Count); while (next == currentIndex); }
            PlayIndex(next, fade);
        }
        else
        {
            int next = currentIndex + 1;
            if (next >= playlist.Count)
            {
                if (!loopPlaylist) return;
                next = 0;
            }
            PlayIndex(next, fade);
        }
    }

    public void Previous(float fade = -1f)
    {
        if (playlist.Count == 0) return;
        if (shuffle)
        {
            int prev = UnityEngine.Random.Range(0, playlist.Count);
            PlayIndex(prev, fade);
        }
        else
        {
            int prev = currentIndex - 1;
            if (prev < 0)
            {
                if (!loopPlaylist) return;
                prev = playlist.Count - 1;
            }
            PlayIndex(prev, fade);
        }
    }

    public void Stop(float fade = -1f)
    {
        float f = fade < 0 ? defaultCrossfade : fade;
        StartCoroutine(CoFadeOutStop(f));
        currentIndex = -1;
    }

    public void Pause() => active?.Pause();
    public void Resume() => active?.UnPause();

    public void TogglePause()
    {
        if (active == null) return;
        if (active.isPlaying) Pause(); else Resume();
    }

    /// <summary> 0..1 lineer volume (Mixer varsa dB’ye dönüşür) </summary>
    public void SetVolume(float linear01)
    {
        linear01 = Mathf.Clamp01(linear01);
        cachedLinearVolume = linear01;
        PlayerPrefs.SetFloat(VPref, linear01);

        if (mixer && !string.IsNullOrEmpty(mixerVolumeParam))
        {
            float dB = Mathf.Log10(Mathf.Clamp(linear01, 0.0001f, 1f)) * 20f;
            mixer.SetFloat(mixerVolumeParam, dB);
        }
        else
        {
            if (a) a.volume = linear01;
            if (b) b.volume = linear01;
        }
    }

    public float GetVolume()
    {
        if (mixer && !string.IsNullOrEmpty(mixerVolumeParam))
        {
            float dB;
            if (mixer.GetFloat(mixerVolumeParam, out dB))
                return Mathf.Pow(10f, dB / 20f);
        }
        return cachedLinearVolume > 0 ? cachedLinearVolume : startVolume;
    }

    public void SetPitch(float pitch)
    {
        pitch = Mathf.Clamp(pitch, -3f, 3f);
        if (a) a.pitch = pitch;
        if (b) b.pitch = pitch;
    }

    /// <summary> SFX sırasında müziği bastır. Mixer snapshot varsa onu karıştırır; yoksa geçici olarak volume indirir. </summary>
    public void DuckForSeconds(float seconds, float fallbackLinearVolume = 0.5f)
    {
        if (duckSnapshot && normalSnapshot)
        {
            duckSnapshot.TransitionTo(snapshotBlend);
            StopCoroutine(nameof(CoUnduck));
            StartCoroutine(CoUnduck(seconds));
        }
        else
        {
            // Mixer yoksa basit lineer düşür/yükselt
            StopCoroutine(nameof(CoDuckFallback));
            StartCoroutine(CoDuckFallback(seconds, fallbackLinearVolume));
        }
    }

    // ===== Internals =====

    string TrackKey(Track t)
    {
        // kalıcı konum anahtarı
        if (!string.IsNullOrEmpty(t.name)) return t.name;
        if (t.clip) return t.clip.name;
        if (t.loopClip) return t.loopClip.name;
        if (t.introClip) return t.introClip.name;
        return "UnknownTrack";
    }

    IEnumerator CoCrossfade(Track target, float fade)
    {
        // aktif -> idle swap mantığı
        // target intro/loop ise intro’dan başlat
        isIntroPhase = target.IsIntroLoopMode;

        // Kaldığı yerden devam (opsiyonel)
        float startTime = Mathf.Max(0f, target.startTime);
        if (persistPlaybackPosition)
        {
            string k = PosPrefPrefix + TrackKey(target);
            startTime = PlayerPrefs.GetFloat(k, startTime);
        }

        // Idle hazırlığı
        idle.Stop();
        idle.loop = !target.IsIntroLoopMode; // intro-loop modunda loop=false (intro); normalde tek klip loop=false (playlist yönetiyor)
        idle.clip = target.IsIntroLoopMode ? target.introClip : (target.clip ? target.clip : target.loopClip);
        idle.time = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, idle.clip ? idle.clip.length - 0.02f : 0f));
        idle.Play();

        // Fade
        float startVol = GetVolume();
        float activeStart = mixer ? startVol : (active.isPlaying ? startVol : 0f);
        float idleStart = mixer ? startVol : 0f;

        if (crossfadeCo != null) StopCoroutine(crossfadeCo);

        float t = 0f;
        while (t < fade)
        {
            t += Time.unscaledDeltaTime;
            float u = fade <= 0 ? 1f : Mathf.Clamp01(t / fade);
            float k = fadeCurve.Evaluate(u);

            if (!mixer)
            {
                if (active) active.volume = Mathf.Lerp(activeStart, 0f, k);
                if (idle)   idle.volume   = Mathf.Lerp(idleStart,   startVol, k);
            }
            yield return null;
        }

        // Swap
        active.Stop();
        var tmp = active; active = idle; idle = tmp;

        // Aktif parça değişti
        OnTrackChanged?.Invoke(target);

        // Intro biterse loop’a geçişi Update() takip eder

        // Bitiş izleme
        StartCoroutine(CoWatchAndSavePosition(target));
    }

    IEnumerator CoWatchAndSavePosition(Track playing)
    {
        while (active && active.isPlaying)
        {
            if (persistPlaybackPosition && playing != null)
            {
                string k = PosPrefPrefix + TrackKey(playing);
                PlayerPrefs.SetFloat(k, active.time);
            }
            yield return null;
        }
    }

    IEnumerator CoFadeOutStop(float fade)
    {
        float startVol = GetVolume();
        if (!mixer)
        {
            float t = 0f;
            while (t < fade)
            {
                t += Time.unscaledDeltaTime;
                float u = fade <= 0 ? 1f : Mathf.Clamp01(t / fade);
                float k = fadeCurve.Evaluate(u);
                if (a) a.volume = Mathf.Lerp(startVol, 0f, k);
                if (b) b.volume = Mathf.Lerp(startVol, 0f, k);
                yield return null;
            }
        }
        else
        {
            if (fade > 0) yield return new WaitForSecondsRealtime(fade);
        }

        a.Stop();
        b.Stop();

        if (!mixer)
            SetVolume(startVol); // sesi geri yükle
    }

    void AutoAdvance()
    {
        if (currentIndex < 0 || playlist.Count == 0) return;

        if (shuffle) Next();
        else
        {
            int next = currentIndex + 1;
            if (next < playlist.Count) PlayIndex(next);
            else if (loopPlaylist) PlayIndex(0);
        }
    }

    IEnumerator CoUnduck(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        normalSnapshot.TransitionTo(snapshotBlend);
    }

    IEnumerator CoDuckFallback(float seconds, float fallbackLinear)
    {
        float orig = GetVolume();
        SetVolume(Mathf.Clamp01(fallbackLinear));
        yield return new WaitForSecondsRealtime(seconds);
        SetVolume(orig);
    }

    // ===== Debug UI =====
    bool debugPanel = false;
    private void OnGUI()
    {
        if (!debugPanel) return;

        var r = new Rect(10, 10, 320, 260);
        GUILayout.BeginArea(r, "MusicManagerPro", GUI.skin.window);
        GUILayout.Label($"Track: {(currentIndex>=0 && currentIndex<playlist.Count ? playlist[currentIndex].ToString() : "(none)")}");

        float v = GUILayout.HorizontalSlider(GetVolume(), 0f, 1f);
        if (Mathf.Abs(v - GetVolume()) > 0.0001f) SetVolume(v);
        GUILayout.Label($"Vol: {GetVolume():0.00}");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("◀ Prev")) Previous();
        if (GUILayout.Button("Next ▶")) Next();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Pause/Resume")) TogglePause();
        if (GUILayout.Button("Stop")) Stop();
        GUILayout.EndHorizontal();

        shuffle = GUILayout.Toggle(shuffle, "Shuffle");
        loopPlaylist = GUILayout.Toggle(loopPlaylist, "Loop Playlist");

        GUILayout.Label("Crossfade");
        defaultCrossfade = GUILayout.HorizontalSlider(defaultCrossfade, 0f, 6f);
        GUILayout.Label($"{defaultCrossfade:0.00}s");

        GUILayout.EndArea();
    }
}
