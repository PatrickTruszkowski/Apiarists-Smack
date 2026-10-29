using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Shaker visualShaker;
    [SerializeField] private Vector2 swatterHitSize;
    [SerializeField] private ContactFilter2D contactFilter2D;
    [SerializeField] private Sprite[] hiveSprites;
    [SerializeField] private GameObject[] flagArray;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform swatterTransform;
    [SerializeField] private Animator swatterAnimator;
    [SerializeField] private Animator hiveAnimator;
    [SerializeField] private AudioClip swatterSmackAudioClip;
    [SerializeField] private AudioClip hiveImpactAudioClip;

    private const int MAX_HEALTH = 5;
    private const string SMACK = "Smack";
    private const string LOW_HEALTH = "LowHealth";

    private AudioSource audioSource;
    private Vector2 smackPosition;
    private int health;
    private bool isInputRestricted = true;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        health = MAX_HEALTH;
    }

    private void Update()
    {
        if (GameManager.Singleton.GetGameState() == GameState.Paused ||
            GameManager.Singleton.GetGameState() == GameState.GameOver) return;

        // Update input every frame
        UpdateInput();
        UpdateSwatterPosition();
    }

    private void UpdateInput()
    {
        // Get the mouse position on screen and convert it to world position
        Vector2 mousePosition = Input.mousePosition;

        smackPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        // Check if left mouse button was pressed this frame
        if (isInputRestricted) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.Space)) return;

        swatterAnimator.SetTrigger(SMACK);

        audioSource.volume = 1f;
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(swatterSmackAudioClip);

        CheckHit();
    }

    private void UpdateSwatterPosition()
    {
        if (GameManager.Singleton.GetGameState() == GameState.Tutorial)
        {
            swatterTransform.gameObject.SetActive(false);

            return;
        }

        swatterTransform.gameObject.SetActive(true);

        swatterTransform.position = smackPosition;
    }

    private void CheckHit()
    {
        // Initialize arguments
        float angle = 0f;
        List<Collider2D> hitFlyColliderList = new();

        // Do a box check for flies
        int hits = Physics2D.OverlapBox(smackPosition, swatterHitSize, angle, contactFilter2D, hitFlyColliderList);

        Vector2 totalPosition = Vector2.zero;

        bool isSpecial = false;

        // Smack each fly that was hit
        foreach (Collider2D flyCollider in hitFlyColliderList)
        {
            if (flyCollider.gameObject == null || flyCollider == null) continue;

            if (flyCollider.TryGetComponent(out Fly fly))
            {
                isSpecial = fly.IsSpecial();
                fly.Die();
            }

            totalPosition += (Vector2)flyCollider.transform.position;
        }

        ScoreData scoreData = new()
        {
            isSpecialFlyKill = isSpecial,
            spawnPosition = smackPosition
        };

        if (totalPosition == Vector2.zero)
        {
            GameManager.Singleton.UpdateScore(scoreData);

            return;
        }

        Vector2 averagePosition = totalPosition / hits;

        scoreData.hitFlyColliders = hitFlyColliderList;

        // Add points for smack
        GameManager.Singleton.UpdateScore(scoreData);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(smackPosition, swatterHitSize);
    }

    public void Reset()
    {
        health = MAX_HEALTH;
        isInputRestricted = false;

        // Reset hive sprite to full health sprite
        spriteRenderer.sprite = hiveSprites[health];

        foreach (GameObject flag in flagArray)
        {
            flag.SetActive(false);
        }

        hiveAnimator.SetBool(LOW_HEALTH, false);

        visualShaker.StopShake();
    }

    public void Hurt()
    {
        spriteRenderer.sprite = hiveSprites[health - 1];
        flagArray[health - 1].SetActive(true);

        health = Mathf.Clamp(health - 1, 0, 5);

        ScoreData scoreData = new()
        {
            playerHurt = true
        };

        GameManager.Singleton.UpdateScore(scoreData);

        if (health == 0)
        {
            GameManager.Singleton.GameOver(false);
        }

        hiveAnimator.SetBool(LOW_HEALTH, health == 1);

        audioSource.volume = 0.5f;
        audioSource.pitch = 1f + 0.1f * (MAX_HEALTH - health);
        audioSource.PlayOneShot(hiveImpactAudioClip);

        visualShaker.Shake();
    }

    public void SetRestrictedInput(bool isInputRestricted)
    {
        this.isInputRestricted = isInputRestricted;
    }
}
