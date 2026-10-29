using UnityEngine;

public class Fly : MonoBehaviour
{
    public static uint ActiveCount = 0;
    
    [SerializeField] private Transform root;
    [SerializeField] private Animator specialFlyMovementAnimator;
    [SerializeField] private RuntimeAnimatorController normalFlyAnimator;
    [SerializeField] private RuntimeAnimatorController specialFlyAnimator;

    private readonly float[] waveSpeeds =
    {
        3f, 3.5f, 4f, 4.5f
    };
    
    private const string VERTICAL = "Vertical";
    private const string HORIZONTAL = "Horizontal";

    private Animator animator;
    private float movementSpeed;
    private bool isSpecial = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    private void Start()
    {
        ActiveCount++;

        if (transform.position.x < 0f) transform.localScale = new(transform.localScale.x * -1f, transform.localScale.y, 0f);

        int waveIndex = GameManager.Singleton.GetCurrentWave() - 1;
        
        movementSpeed = waveSpeeds[waveIndex];
        
        animator.speed = 1f + (float)waveIndex / (waveSpeeds.Length - 1);
    }

    private void Update()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        // Move towards the center where the hive is until it reaches it
        root.position = Vector3.MoveTowards(root.position, Vector3.zero, movementSpeed * Time.deltaTime);
    }

    public void Die()
    {
        ActiveCount--;

        Destroy(transform.root.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.GetComponent<Player>().Hurt();
        Die();
    }

    public bool IsSpecial()
    {
        return isSpecial;
    }

    public void SetIsSpecial(bool isSpecial, bool vertical)
    {
        this.isSpecial = isSpecial;
        
        if (!isSpecial) return;
        
        animator.runtimeAnimatorController = specialFlyAnimator;

        if (vertical)
        {
            specialFlyMovementAnimator.SetBool(VERTICAL, true);
            specialFlyMovementAnimator.SetBool(HORIZONTAL, false);
        }
        else
        {
            specialFlyMovementAnimator.SetBool(VERTICAL, false);
            specialFlyMovementAnimator.SetBool(HORIZONTAL, true);
        }
    }
}
