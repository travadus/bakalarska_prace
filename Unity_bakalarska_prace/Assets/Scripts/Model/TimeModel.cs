using System;
using UnityEngine;

// Èistá tøída, žádný MonoBehaviour
public class TimeModel
{
    // Konfigurace
    private const float REAL_SECONDS_PER_GAME_HOUR = 2f; // 1 herní hodina = 2 sekundy reál

    // Data (State)
    public int CurrentDay { get; private set; } = 1;
    public float CurrentHour { get; private set; } = 6f; // Zaèínáme v 6:00 ráno

    // Událost pro ostatní systémy (napø. Grid), že probìhla hodina
    public event Action<int, int> OnHourTick; // int Day, int Hour

    public void AdvanceTime(float deltaTime)
    {
        // Pøepoèet reálného èasu na herní
        float gameDelta = deltaTime / REAL_SECONDS_PER_GAME_HOUR;

        CurrentHour += gameDelta;

        // Kontrola pøeteèení dne (24h cyklus)
        if (CurrentHour >= 24f)
        {
            CurrentHour -= 24f;
            CurrentDay++;
        }

        // Detekce "Ticku" - pro zjednodušení budeme volat Tick každou celou hodinu
        // V reálné implementaci bys hlídal, jestli floor(oldTime) != floor(newTime)
        CheckForTick();
    }

    // Pomocná promìnná pro detekci zmìny hodiny
    private int _lastTickedHour = -1;

    private void CheckForTick()
    {
        int currentHourInt = Mathf.FloorToInt(CurrentHour);

        if (currentHourInt != _lastTickedHour)
        {
            _lastTickedHour = currentHourInt;
            // Spustíme event - tady se pozdìji napojí tvoje simulace sítì
            OnHourTick?.Invoke(CurrentDay, currentHourInt);
            Debug.Log($"[TICK] Den: {CurrentDay}, Hodina: {currentHourInt}:00");
        }
    }

    // Vrací normalizovaný èas 0.0 (pùlnoc) až 1.0 (další pùlnoc) pro grafiku
    public float GetNormalizedTime()
    {
        return CurrentHour / 24f;
    }
}