[System.Serializable]
public struct WeatherData
{
    public float SunIntensity;  // Výsledné slunce (po odeètení mrakù)
    public float WindIntensity; // Síla vìtru
    public float CloudDensity;  // Hustota mrakù (0 = jasno, 1 = zataženo)
}