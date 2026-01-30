using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeSystem : MonoBehaviour
{
    public static TimeSystem Instance { get; private set; }

    [Header("Nastavení èasu")]
    public float realSecondsPerTick = 1.0f; // 1 sekunda = 1 tik
    private const int GAME_MINUTES_PER_TICK = 10;

    [Header("Startovní Datum (musí sedìt s CSV)")]
    // Nastavíme start na rok 2015, protože tam zaèínají tvá data
    public int startYear = 2015;
    public int startMonth = 1;
    public int startDay = 1;
    public int startHour = 6;

    // Skuteèný C# objekt pro datum a èas
    public DateTime CurrentDateTime { get; private set; }

    [Header("Rychlost hry")]
    private float timeMultiplier = 1f; // 1f = normální rychlost
    private bool isPaused = false;

    [Header("Vizualizace")]
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

    private void Update()
    {
        // Pokud je pauza, nic nepoèítáme a vyskoèíme z metody
        if (isPaused) return;

        // 1. Poèítání èasu s ohledem na násobiè (timeMultiplier)
        timer += Time.deltaTime * timeMultiplier;

        // Pokud hru hodnì zrychlíme, mùže se stát, že v jednom framu
        // musíme provést více tikù najednou.
        while (timer >= realSecondsPerTick)
        {
            timer -= realSecondsPerTick;
            ProcessTick();
        }

        // 2. Plynulá rotace slunce
        UpdateSunPosition();
    }

    // --- OVLÁDÁNÍ RYCHLOSTI (Metody pro tlaèítka) ---
    public void PauseGame() { isPaused = true; }
    public void ResumeGame() { isPaused = false; timeMultiplier = 1f; }
    public void SetSpeedFast() { isPaused = false; timeMultiplier = 5f; }
    public void SetSpeedSuperFast() { isPaused = false; timeMultiplier = 15f; }

    public void RegisterTickable(ITickable tickable)
    {
        if (!tickableObjects.Contains(tickable))
        {
            tickableObjects.Add(tickable);
        }
    }

    public void UnregisterTickable(ITickable tickable)
    {
        if (tickableObjects.Contains(tickable))
        {
            tickableObjects.Remove(tickable);
        }
    }

    private void ProcessTick()
    {
        // Pøidáme minuty do DateTime objektu
        CurrentDateTime = CurrentDateTime.AddMinutes(GAME_MINUTES_PER_TICK);

        // Oznámíme všem systémùm (vèetnì MarketDataSystem), že je nový èas
        OnTick?.Invoke(CurrentDateTime);

        for (int i = tickableObjects.Count - 1; i >= 0; i--)
        {
            tickableObjects[i].OnTick(CurrentDateTime);
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (timeText != null)
        {
            timeText.text = CurrentDateTime.ToString("dd.MM.yyyy | HH:mm");
        }
    }

    private void UpdateSunPosition()
    {
        if (sunLight == null) return;

        // Výpoèet pro slunce (zùstává podobný, jen bereme data z DateTime)
        float totalMinutes = (CurrentDateTime.Hour * 60) + CurrentDateTime.Minute + (timer / realSecondsPerTick * GAME_MINUTES_PER_TICK);
        float dayPercentage = totalMinutes / 1440f;

        float sunAngle = (dayPercentage * 360f) - 90f;
        sunLight.transform.localRotation = Quaternion.Euler(sunAngle, 170f, 0);

        // Intenzita slunce (Noc/Den)
        if (dayPercentage > 0.25f && dayPercentage < 0.75f)
            sunLight.intensity = 1f;
        else if (dayPercentage > 0.2f && dayPercentage < 0.8f)
            sunLight.intensity = 0.5f;
        else
            sunLight.intensity = 0.1f;
    }
}