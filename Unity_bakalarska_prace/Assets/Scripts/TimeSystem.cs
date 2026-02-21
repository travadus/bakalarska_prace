using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Core timekeeping engine responsible for simulated game time progression, 
/// time scaling, environment day/night synchronization, and tick broadcasting.
/// </summary>
public class TimeSystem : MonoBehaviour
{
    public static TimeSystem Instance { get; private set; }

    [Header("Time settings")]
    public float realSecondsPerTick = 1.0f;
    private const int GAME_MINUTES_PER_TICK = 10;

    [Header("Starting date (start need to be same as CSV doc)")]
    // Synchronized with the starting timestamp of external market datasets.
    public int startYear = 2015;
    public int startMonth = 1;
    public int startDay = 1;
    public int startHour = 6;

    public DateTime CurrentDateTime { get; private set; }
    public int TotalGameHours { get; private set; } = 0;

    [Header("Game speed")]
    private float timeMultiplier = 1f;
    private bool isPaused = false;

    [Header("Vizualization")]
    public Light sunLight;
    public TextMeshProUGUI timeText;

    private List<ITickable> tickableObjects = new List<ITickable>();

    public event Action<DateTime> OnTick;

    private float timer;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        CurrentDateTime = new DateTime(startYear, startMonth, startDay, startHour, 0, 0);
    }

    private void Start()
    {
        UpdateUI();
        UpdateSunPosition();
        OnTick?.Invoke(CurrentDateTime);
    }

    /// <summary>
    /// Accumulates scaled real-time and executes discrete game ticks.
    /// Safely handles high time multipliers by processing multiple ticks per frame if necessary.
    /// </summary>
    private void Update()
    {
        if (isPaused) return;

        timer += Time.deltaTime * timeMultiplier;

        while (timer >= realSecondsPerTick)
        {
            timer -= realSecondsPerTick;
            ProcessTick();
        }

        UpdateSunPosition();
    }

    public void PauseGame() { isPaused = true; }
    public void ResumeGame() { isPaused = false; timeMultiplier = 1f; }
    public void SetSpeedFast() { isPaused = false; timeMultiplier = 5f; }
    public void SetSpeedSuperFast() { isPaused = false; timeMultiplier = 15f; }

    /// <summary>
    /// Subscribes an object to the global tick broadcasting system.
    /// </summary>
    /// <param name="tickable">The object implementing the ITickable interface.</param>
    public void RegisterTickable(ITickable tickable)
    {
        if (!tickableObjects.Contains(tickable))
        {
            tickableObjects.Add(tickable);
        }
    }

    /// <summary>
    /// Unsubscribes an object from the global tick broadcasting system.
    /// </summary>
    /// <param name="tickable">The object implementing the ITickable interface.</param>
    public void UnregisterTickable(ITickable tickable)
    {
        if (tickableObjects.Contains(tickable))
        {
            tickableObjects.Remove(tickable);
        }
    }

    /// <summary>
    /// Advances the internal game time and notifies all registered listeners.
    /// </summary>
    private void ProcessTick()
    {
        int oldHour = CurrentDateTime.Hour;

        CurrentDateTime = CurrentDateTime.AddMinutes(GAME_MINUTES_PER_TICK);

        if (CurrentDateTime.Hour != oldHour)
        {
            TotalGameHours++;
        }

        OnTick?.Invoke(CurrentDateTime);

        for (int i = tickableObjects.Count - 1; i >= 0; i--)
        {
            tickableObjects[i].OnTick(CurrentDateTime);
        }

        UpdateUI();
    }

    /// <summary>
    /// Refreshes the user interface text with the current formatted date and time.
    /// </summary>
    private void UpdateUI()
    {
        if (timeText != null)
        {
            timeText.text = CurrentDateTime.ToString("dd.MM.yyyy | HH:mm");
        }
    }

    /// <summary>
    /// Calculates and applies the sun's rotational angle and light intensity 
    /// based on the current fractional progression of the simulated day.
    /// </summary>
    private void UpdateSunPosition()
    {
        if (sunLight == null) return;

        float totalMinutes = (CurrentDateTime.Hour * 60) + CurrentDateTime.Minute + (timer / realSecondsPerTick * GAME_MINUTES_PER_TICK);
        float dayPercentage = totalMinutes / 1440f;

        float sunAngle = (dayPercentage * 360f) - 90f;
        sunLight.transform.localRotation = Quaternion.Euler(sunAngle, 170f, 0);

        if (dayPercentage > 0.25f && dayPercentage < 0.75f)
            sunLight.intensity = 1f;
        else if (dayPercentage > 0.2f && dayPercentage < 0.8f)
            sunLight.intensity = 0.5f;
        else
            sunLight.intensity = 0.1f;
    }
}