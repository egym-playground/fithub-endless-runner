using UnityEngine;
using TMPro; // Optional if you're using TextMeshPro

public class TextPopAnimationSimple : MonoBehaviour
{
    [Header("Animation Settings")]
    public float scaleUp = 1.5f;       // How big it gets
    public float duration = 0.5f;      // Time to scale up or down
    public bool loop = true;           // Whether the animation repeats

    private Vector3 originalScale;
    private bool scalingUp = true;

    void Start()
    {
        originalScale = transform.localScale;
        StartCoroutine(AnimateText());
    }

    System.Collections.IEnumerator AnimateText()
    {
        while (loop)
        {
            float elapsed = 0f;
            Vector3 targetScale = scalingUp ? originalScale * scaleUp : originalScale;

            Vector3 startScale = transform.localScale;
            while (elapsed < duration)
            {
                transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localScale = targetScale;
            scalingUp = !scalingUp;
        }
    }
}
