using UnityEngine;
using TMPro; // Nezapomeò importovat TextMeshPro

public class TimeView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light directionalLight; // Slunce
    [SerializeField] private TextMeshProUGUI clockText; // UI text hodin

    [Header("Settings")]
    [SerializeField] private Gradient skyColorGradient; // Volitelné: barva svìtla dle èasu

    public void UpdateVisuals(float normalizedTime, int day, float hour)
    {
        // 1. Rotace slunce
        // 0.0 = Pùlnoc, 0.25 = 6:00 (Východ), 0.5 = Poledne, 0.75 = 18:00 (Západ)
        // Rotujeme kolem osy X. -90 je pùlnoc, 90 je poledne, 270 je pùlnoc.
        // Jednoduchý vzorec: (normalizedTime * 360) - 90
        float sunAngle = (normalizedTime * 360f) - 90f;

        if (directionalLight != null)
        {
            directionalLight.transform.localRotation = Quaternion.Euler(sunAngle, 170f, 0);

            // Volitelné: Zmìna barvy/intenzity svìtla
            if (skyColorGradient != null)
            {
                directionalLight.color = skyColorGradient.Evaluate(normalizedTime);
            }

            // Vypnutí stínù v noci (volitelné pro výkon/vzhled)
            if (normalizedTime < 0.2f || normalizedTime > 0.8f)
                directionalLight.intensity = 0; // Tma
            else
                directionalLight.intensity = 1; // Den
        }

        // 2. UI Text
        if (clockText != null)
        {
            // Formátování èasu na "00:00"
            int h = Mathf.FloorToInt(hour);
            int m = Mathf.FloorToInt((hour - h) * 60);
            clockText.text = $"Den {day} | {h:00}:{m:00}";
        }
    }
}