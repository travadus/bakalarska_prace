using UnityEngine;

/// <summary>
/// Procedural weather generation system utilizing Perlin noise to simulate dynamic, 
/// continuous, and deterministic environmental conditions.
/// </summary>
public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance;

    [Header("Current status")]
    public WeatherData CurrentWeather;

    [Header("Generator settings")]
    [SerializeField] private float windChangeSpeed = 0.5f;
    [SerializeField] private float cloudChangeSpeed = 0.3f;

    private float windSeed;
    private float cloudSeed;

    private void Awake()
    {
        Instance = this;

        windSeed = Random.Range(0f, 1000f);
        cloudSeed = Random.Range(0f, 1000f);
    }

    private void Start()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnTick += UpdateCurrentWeather;
            UpdateCurrentWeather(TimeSystem.Instance.CurrentDateTime);
        }
    }

    private void UpdateCurrentWeather(System.DateTime time)
    {
        CurrentWeather = CalculateWeatherForTime(time);
    }

    /// <summary>
    /// Deterministically calculates weather conditions for a specific timestamp.
    /// </summary>
    /// <param name="time">The target timestamp for the weather calculation.</param>
    /// <returns>WeatherData instance</returns>
    public WeatherData CalculateWeatherForTime(System.DateTime time)
    {
        WeatherData data = new WeatherData();

        // 1. BASE SUN INTENSITY
        // Calculates a parabolic trajectory during daylight hours (6:00 to 20:00).
        float baseSun = 0f;
        int hour = time.Hour;
        if (hour >= 6 && hour <= 20)
        {
            float t = (hour - 6) / 14.0f;
            baseSun = 4 * t * (1 - t);
        }

        // 2. PROCEDURAL CLOUDS & WIND
        // Generates natural looking variations using Perlin noise mapped to total elapsed hours.
        float totalHours = (float)(time - new System.DateTime(2025, 1, 1)).TotalHours;

        data.CloudDensity = Mathf.PerlinNoise(cloudSeed, totalHours * cloudChangeSpeed);
        data.WindIntensity = Mathf.PerlinNoise(windSeed, totalHours * windChangeSpeed);

        // 3. SUN INTENSITY ATTENUATION
        // Applies cloud cover attenuation to the base sun intensity using the formula: 
        data.SunIntensity = baseSun * (1.0f - (data.CloudDensity * 0.8f));

        return data;
    }
}