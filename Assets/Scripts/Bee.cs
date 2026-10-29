using System;
using UnityEngine;

public class Bee : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private Transform beeVisualTransform;
    
    private Fly targetFly;
    private float velocityX;
    private float lastXPosition;
    private bool isPoppingOut = true;
    
    private void Start()
    {
        FindTargetFly();
    }

    private void Update()
    {
        if (isPoppingOut) return;
        
        Vector2 targetPosition;
        
        if (!targetFly)
        {
            FindTargetFly();
            
            targetPosition = Vector2.zero;
        }
        else
        {
            targetPosition = targetFly.transform.position;
        }
        
        lastXPosition = transform.position.x;
        
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
        
        velocityX = transform.position.x - lastXPosition;
        
        Debug.Log(velocityX);

        if (velocityX != 0f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(velocityX);
            transform.localScale = scale;
        }
    }

    private void FindTargetFly()
    {
        if (Fly.ActiveCount == 0)
        {
            targetFly = null;

            return;
        }
        
        Fly[] activeFlies = FindObjectsByType<Fly>(FindObjectsSortMode.None);

        float closestDistance = float.MaxValue;
        
        foreach (Fly fly in activeFlies)
        {
            float currentDistance = Vector2.Distance(Vector2.zero, fly.transform.position);
                    
            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                targetFly = fly;
            }
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public void DonePoppingOut()
    {
        isPoppingOut = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Fly fly = collision.transform.GetComponent<Fly>();
        
        bool isSpecial = fly.IsSpecial();

        fly.Die();

        ScoreData scoreData = new()
        {
            isBeeKill = true,
            isSpecialFlyKill = isSpecial,
            spawnPosition = transform.position
        };

        GameManager.Singleton.UpdateScore(scoreData);

        Die();
    }
}
