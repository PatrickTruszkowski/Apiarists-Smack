using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private AudioSource audioSource;
    private Vector2 normalLocalScale;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        normalLocalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        audioSource.PlayOneShot(audioSource.clip);
        
        transform.localScale = new Vector2(transform.localScale.x + 0.1f, transform.localScale.y + 0.1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = new Vector2(transform.localScale.x - 0.1f, transform.localScale.y - 0.1f);
    }

    private void OnDisable()
    {
        transform.localScale = normalLocalScale;
    }
}
