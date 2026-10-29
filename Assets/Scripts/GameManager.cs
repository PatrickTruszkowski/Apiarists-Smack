using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState
{
    Tutorial,
    WaveActive,
    Intermission,
    GameOver,
    Paused,
    None
}

public class ScoreData
{
    public List<Collider2D> hitFlyColliders;
    public bool playerHurt;
    public bool isBeeKill;
    public bool isSpecialFlyKill;
    public Vector2 spawnPosition;
}

public class SaveData
{
    public int highScore;
    public bool hasCompletionAchievement;
    public bool hasNoHitAchievement;
    public bool hasSharpSwatterAchievement;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton { get; private set; }
    public static int HighScore { get; set; }
    public static bool HasCompletionAchievement { get; set; } = false;
    public static bool HasNoHitAchievement { get; set; } = false;
    public static bool HasSharpSwatterAchievement { get; set; } = false;

    [SerializeField] private Player player;
    [SerializeField] private Transform flyPrefab;
    [SerializeField] private Transform flyBloodPrefab;
    [SerializeField] private Transform beePrefab;
    [SerializeField] private Transform addedPointsUIPrefab;
    [SerializeField] private Transform UITransform3D;
    [SerializeField] private Transform beeSpawnTransform;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject pausedUI;
    [SerializeField] private GameObject loadingScreenUI;
    [SerializeField] private GameObject scoreUI;
    [SerializeField] private GameObject waveUI;
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] private GameObject newHighScoreText;
    [SerializeField] private GameObject noHitAchievement;
    [SerializeField] private GameObject completionAchievement;
    [SerializeField] private GameObject sharpSwatterAchievement;
    [SerializeField] private GameObject musicGameObject;
    [SerializeField] private Button restartButtonGameOver;
    [SerializeField] private Button quitButtonGameOver;
    [SerializeField] private Button quitButtonPaused;
    [SerializeField] private Button restartButtonPaused;
    [SerializeField] private Button resumeButton;
    [SerializeField] private TextMeshProUGUI scoreCounterText;
    [SerializeField] private TextMeshProUGUI waveCounterText;
    [SerializeField] private Animator waveUIAnimator;
    [SerializeField] private TextMeshProUGUI gameOverScoreCounterText;
    [SerializeField] private TextMeshProUGUI gameOverHighScoreCounterText;
    [SerializeField] private TextMeshProUGUI gameOverWaveCounterText;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Color losingColor;
    [SerializeField] private Color winningColor;
    [SerializeField] private Color pointsHitColor;
    [SerializeField] private Color pointsMissColor;
    [SerializeField] private AudioSource scoreFlippingAudioSource;
    [SerializeField] private AudioSource fliesBuzzingAudioSource;
    [SerializeField] private float closeCallDistance = 3f;
    [SerializeField] private AudioClip flySplatAudioClip;

    private const string SHOWING_WAVE = "ShowingWave";
    private const float MAX_FLY_SPAWN_X = 10f;
    private const float MAX_FLY_SPAWN_Y = 5f;
    private const float MAX_WAVE_INTERMISSION_TIMER = 2f;
    private const int TOTAL_WAVES = 4;

    private readonly float[] flySpawnTimers =
    {
        1.5f, 1f, 0.75f, 0.5f
    };
    private readonly int[] flyWaveCounts =
    {
        20, 30, 40, 50
    };
    private readonly int[] specialFlySpawnChances =
    {
        0, 10, 15, 20
    };

    private AudioSource audioSource;
    private GameState currentGameState = GameState.Tutorial;
    private GameState previousGameState = GameState.None;
    private Vector3 normalScoreTextScale;
    private float flySpawnTimer;
    private int score;
    private float smoothScore;
    private int currentWave;
    private int currentFlyCount;
    private int currentSpecialFlySpawnChance;
    private float currentFlySpawnTimer;
    private float currentWaveIntermissionTimer;
    private float scoreFlippingNormalVolume;
    private bool hasGottenHit;
    private bool hasMissed;

    private void Awake()
    {
        Singleton = this;

        audioSource = GetComponent<AudioSource>();

        gameOverUI.SetActive(false);
        pausedUI.SetActive(false);
        loadingScreenUI.SetActive(false);
        scoreUI.SetActive(false);
        waveUI.SetActive(false);
        musicGameObject.SetActive(false);
        tutorialUI.SetActive(true);

        restartButtonGameOver.onClick.AddListener(RestartGame);
        quitButtonGameOver.onClick.AddListener(() =>
        {
            loadingScreenUI.SetActive(true);
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        });
        restartButtonPaused.onClick.AddListener(RestartGame);
        quitButtonPaused.onClick.AddListener(() =>
        {
            loadingScreenUI.SetActive(true);
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        });
        resumeButton.onClick.AddListener(Pause);

        Fly.ActiveCount = 0;
        AddedPointsUI.ActiveCount = 0;
        FlyBlood.ActiveCount = 0;

        currentFlySpawnTimer = flySpawnTimers[0];
        currentFlyCount = flyWaveCounts[0];
        currentSpecialFlySpawnChance = specialFlySpawnChances[0];

        normalScoreTextScale = scoreCounterText.transform.localScale;
        scoreFlippingNormalVolume = scoreFlippingAudioSource.volume;
    }

    private void Update()
    {
        UpdatePausing();
        UpdateGameState();
    }

    private void UpdatePausing()
    {
        if (currentGameState == GameState.Tutorial || currentGameState == GameState.GameOver || !Input.GetKeyDown(KeyCode.Escape)) return;

        Pause();
    }

    private void UpdateGameState()
    {
        switch (currentGameState)
        {
            case GameState.Tutorial:
                UpdateTutorialState();
                break;
            case GameState.WaveActive:
                UpdateWaveActiveState();
                break;
            case GameState.Intermission:
                UpdateIntermissionState();
                break;
            case GameState.Paused:
            case GameState.GameOver:
            case GameState.None:
                break;
        }

        UpdateUIEffects();
        UpdateAudioEffects();
    }

    private void UpdateTutorialState()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        tutorialUI.SetActive(false);
        scoreUI.SetActive(true);
        waveUI.SetActive(true);
        musicGameObject.SetActive(true);

        waveCounterText.text = (currentWave + 1).ToString();
        waveUIAnimator.SetBool(SHOWING_WAVE, true);

        currentWaveIntermissionTimer = MAX_WAVE_INTERMISSION_TIMER;

        Cursor.visible = false;

        currentGameState = GameState.Intermission;
    }

    private void UpdateWaveActiveState()
    {
        if (currentFlyCount == 0)
        {
            if (Fly.ActiveCount > 0) return;

            if (currentWave + 1 > TOTAL_WAVES)
            {
                GameOver(true);

                return;
            }

            waveCounterText.text = (currentWave + 1).ToString();
            waveUIAnimator.SetBool(SHOWING_WAVE, true);

            currentWaveIntermissionTimer = MAX_WAVE_INTERMISSION_TIMER;
            currentGameState = GameState.Intermission;

            return;
        }

        if (flySpawnTimer > 0f)
        {
            flySpawnTimer -= Time.deltaTime;
        }
        else
        {
            currentFlyCount = Mathf.Clamp(currentFlyCount - 1, 0, currentFlyCount);
            flySpawnTimer = currentFlySpawnTimer;

            int randomSide = Random.Range(0, 4);
            float randomX = 0f;
            float randomY = 0f;

            switch (randomSide)
            {
                case 0: // Left side

                    randomX = -MAX_FLY_SPAWN_X;
                    randomY = Random.Range(-MAX_FLY_SPAWN_Y, MAX_FLY_SPAWN_Y);

                    break;
                case 1: // Top side

                    randomX = Random.Range(-MAX_FLY_SPAWN_X, MAX_FLY_SPAWN_X);
                    randomY = MAX_FLY_SPAWN_Y;

                    break;
                case 2: // Right side

                    randomX = MAX_FLY_SPAWN_X;
                    randomY = Random.Range(-MAX_FLY_SPAWN_Y, MAX_FLY_SPAWN_Y);

                    break;
                case 3: // Bottom side

                    randomX = Random.Range(-MAX_FLY_SPAWN_X, MAX_FLY_SPAWN_X);
                    randomY = -MAX_FLY_SPAWN_Y;

                    break;
            }

            Vector3 randomPosition = new(randomX, randomY, 0f);

            int specialFlyChance = Random.Range(0, 101);

            Transform flyTransform = Instantiate(flyPrefab);

            flyTransform.GetComponentInChildren<Fly>().SetIsSpecial(specialFlyChance < currentSpecialFlySpawnChance, randomSide == 0 || randomSide == 2);

            flyTransform.position = randomPosition;
        }
    }

    private void UpdateIntermissionState()
    {
        if (currentWaveIntermissionTimer > 0f)
        {
            currentWaveIntermissionTimer -= Time.deltaTime;
        }
        else
        {
            currentWave++;

            waveCounterText.text = currentWave.ToString();

            int waveIndex = currentWave - 1;

            currentFlySpawnTimer = flySpawnTimers[waveIndex];
            currentFlyCount = flyWaveCounts[waveIndex];
            currentSpecialFlySpawnChance = specialFlySpawnChances[waveIndex];

            flySpawnTimer = 0f;

            waveUIAnimator.SetBool(SHOWING_WAVE, false);

            player.SetRestrictedInput(false);

            currentGameState = GameState.WaveActive;
        }
    }

    private void UpdateUIEffects()
    {
        const float scoreSmoothSpeed = 100f;
        const float smoothSpeedTextScale = 10f;

        smoothScore = Mathf.MoveTowards(smoothScore, score, scoreSmoothSpeed * Time.deltaTime);
        scoreCounterText.transform.localScale = Vector3.Lerp(scoreCounterText.transform.localScale, normalScoreTextScale, smoothSpeedTextScale * Time.deltaTime);

        scoreCounterText.text = smoothScore.ToString("F0");

        const float minimumScoreCounterDifference = 1f;

        scoreFlippingAudioSource.volume = Mathf.Abs(score - smoothScore) > minimumScoreCounterDifference ? scoreFlippingNormalVolume * Time.timeScale : 0f;
    }

    private void UpdateAudioEffects()
    {
        if (currentGameState == GameState.Paused || currentGameState == GameState.GameOver)
        {
            fliesBuzzingAudioSource.volume = 0f;
        }

        const float audioSmoothingSpeed = 4f;

        if (Fly.ActiveCount > 0f)
        {
            fliesBuzzingAudioSource.volume = Mathf.Lerp(fliesBuzzingAudioSource.volume, 0.5f, audioSmoothingSpeed * Time.deltaTime);
        }
        else
        {
            fliesBuzzingAudioSource.volume = Mathf.Lerp(fliesBuzzingAudioSource.volume, 0f, audioSmoothingSpeed * Time.deltaTime);
        }
    }

    private void RestartGame()
    {
        player.Reset();

        Fly[] leftOverFlies = FindObjectsByType<Fly>(FindObjectsSortMode.None);

        foreach (Fly fly in leftOverFlies)
        {
            fly.Die();
        }

        Bee[] leftOverBees = FindObjectsByType<Bee>(FindObjectsSortMode.None);

        foreach (Bee bee in leftOverBees)
        {
            bee.Die();
        }

        AddedPointsUI[] leftOverAddedPointsUIs = FindObjectsByType<AddedPointsUI>(FindObjectsSortMode.None);

        foreach (AddedPointsUI addedPointsUI in leftOverAddedPointsUIs)
        {
            addedPointsUI.Despawn();
        }

        FlyBlood[] leftOverFlyBlood = FindObjectsByType<FlyBlood>(FindObjectsSortMode.None);

        foreach (FlyBlood flyBlood in leftOverFlyBlood)
        {
            flyBlood.Despawn();
        }

        score = 0;
        smoothScore = 0;
        flySpawnTimer = 0f;

        currentFlySpawnTimer = flySpawnTimers[0];
        currentFlyCount = flyWaveCounts[0];
        currentSpecialFlySpawnChance = specialFlySpawnChances[0];

        currentWave = 1;

        hasGottenHit = false;
        hasMissed = false;

        gameOverUI.SetActive(false);
        scoreUI.SetActive(true);
        waveUIAnimator.SetBool(SHOWING_WAVE, false);
        waveUI.SetActive(true);
        pausedUI.SetActive(false);

        scoreCounterText.transform.localScale = normalScoreTextScale;

        scoreCounterText.text = "0";
        waveCounterText.text = "1";

        Time.timeScale = 1f;

        Cursor.visible = false;

        currentGameState = GameState.WaveActive;
    }

    public void UpdateScore(ScoreData scoreData)
    {
        Transform addedPointsUITransform = Instantiate(addedPointsUIPrefab, UITransform3D, false);

        addedPointsUITransform.position = scoreData.spawnPosition;

        AddedPointsUI addedPointsUI = addedPointsUITransform.GetComponent<AddedPointsUI>();

        int scoreGain = 0;

        if (scoreData.playerHurt)
        {
            hasGottenHit = true;

            scoreGain -= 50;

            addedPointsUI.pointsText.text = scoreGain.ToString();
            addedPointsUI.pointsText.color = pointsMissColor;
        }
        else if (scoreData.isBeeKill)
        {
            scoreGain += 50;

            addedPointsUI.pointsText.text = "+" + scoreGain;
            addedPointsUI.pointsText.color = pointsHitColor;

            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(flySplatAudioClip);

            Instantiate(flyBloodPrefab, scoreData.spawnPosition, Quaternion.Euler(Vector3.forward * Random.Range(0f, 359f)));
        }
        else
        {
            if (scoreData.hitFlyColliders == null || scoreData.hitFlyColliders.Count == 0)
            {
                scoreGain -= 10;

                addedPointsUI.pointsText.text = scoreGain.ToString();
                addedPointsUI.pointsText.color = pointsMissColor;

                hasMissed = true;
            }
            else
            {
                float closestDistance = float.MaxValue;

                foreach (Collider2D hitFlyCollider in scoreData.hitFlyColliders)
                {
                    float currentDistance = Vector2.Distance(Vector2.zero, hitFlyCollider.transform.position);

                    if (currentDistance < closestDistance)
                    {
                        closestDistance = currentDistance;
                    }
                }

                if (scoreData.hitFlyColliders.Count >= 2)
                {
                    Instantiate(beePrefab, beeSpawnTransform.position, beeSpawnTransform.rotation);
                }

                int closeCallMultiplier = closestDistance <= closeCallDistance ? 2 : 1;
                int specialFlyMutiplier = scoreData.isSpecialFlyKill ? 3 : 1;

                scoreGain += (int)Mathf.Pow(10, scoreData.hitFlyColliders.Count) * closeCallMultiplier * specialFlyMutiplier;

                addedPointsUI.pointsText.text = "+" + scoreGain;
                addedPointsUI.pointsText.color = pointsHitColor;

                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(flySplatAudioClip);

                Instantiate(flyBloodPrefab, scoreData.spawnPosition, Quaternion.Euler(Vector3.forward * Random.Range(0f, 359f)));
            }
        }

        score = Mathf.Clamp(score + scoreGain, 0, int.MaxValue);

        const float scaleOffset = 0.5f;

        scoreCounterText.transform.localScale += new Vector3(scaleOffset, scaleOffset, 0f);
    }

    public void GameOver(bool isWin)
    {
        currentGameState = GameState.GameOver;

        gameOverText.text = isWin ? "YOU WON" : "GAME OVER";
        gameOverText.color = isWin ? winningColor : losingColor;

        gameOverUI.SetActive(true);
        scoreUI.SetActive(false);
        waveUI.SetActive(false);

        if (isWin)
        {
            if (!HasCompletionAchievement)
            {
                HasCompletionAchievement = true;
                completionAchievement.SetActive(true);
            }

            if (!hasGottenHit && !HasNoHitAchievement)
            {
                HasNoHitAchievement = true;
                noHitAchievement.SetActive(true);
            }

            if (!hasMissed && !HasSharpSwatterAchievement)
            {
                HasSharpSwatterAchievement = true;
                sharpSwatterAchievement.SetActive(true);
            }
        }

        if (score > HighScore)
        {
            HighScore = score;

            newHighScoreText.SetActive(true);
        }
        else
        {
            newHighScoreText.SetActive(false);
        }

        gameOverScoreCounterText.text = score.ToString();
        gameOverHighScoreCounterText.text = HighScore.ToString();
        gameOverWaveCounterText.text = currentWave.ToString();

        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    private void Pause()
    {
        if (currentGameState != GameState.Paused)
        {
            pausedUI.SetActive(true);

            Time.timeScale = 0f;

            Cursor.visible = true;

            previousGameState = currentGameState;
            currentGameState = GameState.Paused;
        }
        else
        {
            pausedUI.SetActive(false);

            Cursor.visible = false;

            Time.timeScale = 1f;
            currentGameState = previousGameState;
        }
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }

    public GameState GetGameState()
    {
        return currentGameState;
    }
}
