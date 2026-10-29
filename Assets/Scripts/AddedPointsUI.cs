using TMPro;
using UnityEngine;

public class AddedPointsUI : MonoBehaviour
{
    public static uint ActiveCount = 0;
    
    public TextMeshProUGUI pointsText;

    private void Start()
    {
        ActiveCount++;
    }

    private void Update()
    {
        if (GameManager.Singleton.GetGameState() == GameState.GameOver)
        {
            Despawn();
        }
    }
    
    public void Despawn()
    {
        ActiveCount--;
        Destroy(gameObject);
    }
}
