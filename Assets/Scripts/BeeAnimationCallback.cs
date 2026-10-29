using UnityEngine;

public class BeeAnimationCallback : MonoBehaviour
{
    private Bee bee;

    private void Start()
    {
        bee = GetComponentInParent<Bee>();
    }

    public void DonePoppingOut()
    {
        bee.DonePoppingOut();
    }
}
