using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance;

    [Header("Aktuální stav")]
    public WeatherData CurrentWeather;

    [Header("Nastavení Generátoru")]
    [SerializeField] private float windChangeSpeed = 0.5f;   // Jak rychle se mìní vítr
    [SerializeField] private float cloudChangeSpeed = 0.3f;  // Jak rychle se hýbou mraky

    // "Seeds" jsou èísla, která urèí unikátní prùbìh pro každou novou hru
    private float windSeed;
    private float cloudSeed;

    private void Awake()
    {
        Instance = this;
        // Vygenerujeme náhodný svìt pøi startu
        windSeed = Random.Range(0f, 1000f);
        cloudSeed = Random.Range(0f, 1000f);
    }

    private void Start()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnTick += UpdateCurrentWeather;
            // Inicializace hned na zaèátku
            UpdateCurrentWeather(TimeSystem.Instance.CurrentDateTime);
        }
    }

    // Volá se každou hodinu -> aktualizuje "CurrentWeather"
    private void UpdateCurrentWeather(System.DateTime time)
    {
        CurrentWeather = CalculateWeatherForTime(time);
    }

    // --- JÁDRO SYSTÉMU (TOTO JE TA VÌŠTECKÁ KOULE) ---
    // Tato metoda je "èistá". Když do ní pošleš stejný èas, vrátí vždy stejný výsledek.
    // Díky tomu mùžeme pøedpovídat budoucnost.
    public WeatherData CalculateWeatherForTime(System.DateTime time)
    {
        WeatherData data = new WeatherData();

        // 1. SLUNCE (Základní parabola podle hodiny)
        float baseSun = 0f;
        int hour = time.Hour;
        if (hour >= 6 && hour <= 20)
        {
            float t = (hour - 6) / 14.0f;
            baseSun = 4 * t * (1 - t); // Parabola 0 -> 1 -> 0
        }

        // 2. MRAKY (Perlin Noise)
        // Používáme TotalHours, aby se mraky mìnily plynule den za dnem
        // (time.Ticks by bylo moc velké èíslo, pøevedeme na hodiny)
        float totalHours = (float)(time - new System.DateTime(2025, 1, 1)).TotalHours;

        // PerlinNoise vrací 0.0 až 1.0
        data.CloudDensity = Mathf.PerlinNoise(cloudSeed, totalHours * cloudChangeSpeed);

        // 3. VÍTR (Jiný Perlin Noise)
        data.WindIntensity = Mathf.PerlinNoise(windSeed, totalHours * windChangeSpeed);

        // 4. VÝPOÈET REÁLNÉHO SLUNCE
        // Pokud jsou mraky, slunce svítí ménì.
        // Vzorec: Slunce * (1 - Mraky). 
        // Pøíklad: Slunce 1.0 (poledne) * (1 - 0.8 mraky) = 0.2 (skoro tma)
        data.SunIntensity = baseSun * (1.0f - (data.CloudDensity * 0.8f));
        // * 0.8f znamená, že ani pøi 100% mracích není úplná tma, trochu svìtla projde.

        return data;
    }
}