using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject creditsScreen;
    [SerializeField] private Image noHitAchievementBee;
    [SerializeField] private Image noHitAchievementSticker;
    [SerializeField] private Image completionAchievementSticker;
    [SerializeField] private Image completionAchievementFly;
    [SerializeField] private Image sharpSwatterAchievementSticker;
    [SerializeField] private Image sharpSwatterAchievementSwatter;
    [SerializeField] private Color achievementColor;

    private void Start()
    {
        LoadSavedGameData();

        mainMenuScreen.SetActive(true);
        loadingScreen.SetActive(false);
        creditsScreen.SetActive(false);

        if (GameManager.HasNoHitAchievement)
        {
            noHitAchievementBee.color = achievementColor;
            noHitAchievementSticker.color = Color.white;
        }

        if (GameManager.HasCompletionAchievement)
        {
            completionAchievementFly.color = achievementColor;
            completionAchievementSticker.color = Color.white;
        }

        if (GameManager.HasSharpSwatterAchievement)
        {
            sharpSwatterAchievementSwatter.color = achievementColor;
            sharpSwatterAchievementSticker.color = Color.white;
        }

        playButton.onClick.AddListener(() =>
        {
            loadingScreen.SetActive(true);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        });
        creditsButton.onClick.AddListener(() =>
        {
            mainMenuScreen.SetActive(false);
            creditsScreen.SetActive(true);
        });
        backButton.onClick.AddListener(() =>
        {
            creditsScreen.SetActive(false);
            mainMenuScreen.SetActive(true);
        });
        quitButton.onClick.AddListener(Application.Quit);
    }

    private void OnApplicationQuit()
    {
        SaveGameData();
    }

    private void LoadSavedGameData()
    {
        string savedDataFilePath = Path.Combine(Application.persistentDataPath, "SavedGameData.json");

        if (!File.Exists(savedDataFilePath))
        {
            return;
        }

        string saveDataJson = File.ReadAllText(savedDataFilePath);

        if (string.IsNullOrEmpty(saveDataJson))
        {
            return;
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(saveDataJson);

        GameManager.HighScore = saveData.highScore;
        GameManager.HasCompletionAchievement = saveData.hasCompletionAchievement;
        GameManager.HasNoHitAchievement = saveData.hasNoHitAchievement;
        GameManager.HasSharpSwatterAchievement = saveData.hasSharpSwatterAchievement;
    }

    public void SaveGameData()
    {
        SaveData saveData = new()
        {
            highScore = GameManager.HighScore,
            hasCompletionAchievement = GameManager.HasCompletionAchievement,
            hasNoHitAchievement = GameManager.HasNoHitAchievement,
            hasSharpSwatterAchievement = GameManager.HasSharpSwatterAchievement
        };

        string saveDataJson = JsonUtility.ToJson(saveData);
        string savedDataFilePath = Path.Combine(Application.persistentDataPath, "SavedGameData.json");

        File.WriteAllText(savedDataFilePath, saveDataJson);
    }
}
