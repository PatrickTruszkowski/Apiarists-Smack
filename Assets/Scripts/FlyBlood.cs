using UnityEngine;

public class FlyBlood : MonoBehaviour
{
    public static uint ActiveCount = 0;

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
