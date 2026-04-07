using UnityEngine;

/*
 * Destroys the GameObject after a set delay.
 * Used for one-shot effects like dust impacts.
 */
public class AutoDestroy : MonoBehaviour
{
    [Tooltip("How long before this GameObject is destroyed.")]
    public float delay = 0.5f;

    private void Start()
    {
        Destroy(gameObject, delay);
    }
}