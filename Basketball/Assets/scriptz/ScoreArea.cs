using System.Collections;
using UnityEngine;

public class ScoreArea : MonoBehaviour
{
    public GameObject effectObject;
    public float effectDuration = 2f; // Trvanie efektu v sekundách

    void Start()
    {
        effectObject.SetActive(false); // Efekt je na zaèiatku vypnutý
    }

    void OnTriggerEnter(Collider otherCollider)
    {
        if (otherCollider.GetComponent<Ball>() != null)
        {
            ActivateEffect(); // Aktivuje efekt, keï lopta prejde cez kôš
        }
    }

    private void ActivateEffect()
    {
        effectObject.SetActive(true); // Aktivuj efekt
        StartCoroutine(DeactivateEffectAfterDelay()); // Spusti èasovaè na vypnutie efektu
    }

    private IEnumerator DeactivateEffectAfterDelay()
    {
        yield return new WaitForSeconds(effectDuration); // Poèkaj urèitý èas
        effectObject.SetActive(false); // Vypni efekt
    }
}
