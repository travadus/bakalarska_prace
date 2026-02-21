using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Handles the extraction and parsing of energy market data from CSV files.
/// </summary>
public class CsvMarketDataLoader
{
    /// <summary>
    /// Reads a CSV text asset and parses the remaining lines into data entries.
    /// </summary>
    /// <param name="csvFile">The raw CSV TextAsset containing market data.</param>
    /// <returns>A list of parsed EnergyDataEntry objects.</returns>
    public List<EnergyDataEntry> LoadData(TextAsset csvFile)
    {
        List<EnergyDataEntry> data = new List<EnergyDataEntry>();

        if (csvFile == null)
        {
            Debug.LogError("CsvMarketDataLoader: No file");
            return data;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            EnergyDataEntry? entry = ParseLine(line);
            if (entry.HasValue)
            {
                data.Add(entry.Value);
            }
        }

        Debug.Log($"CsvMarketDataLoader: Loaded {data.Count} results.");
        return data;
    }

    /// <summary>
    /// Parses a single CSV row into an EnergyDataEntry.
    /// Fails silently on parsing errors to maintain performance over large datasets.
    /// </summary>
    /// <param name="line">A single row from the CSV.</param>
    /// <returns>A nullable EnergyDataEntry, returning null if parsing fails or data is malformed.</returns>
    private EnergyDataEntry? ParseLine(string line)
    {
        string[] columns = line.Split(',');

        if (columns.Length < 5) return null;

        try
        {
            DateTime dt = DateTime.Parse(columns[2]);

            float price = float.Parse(columns[4], CultureInfo.InvariantCulture);

            return new EnergyDataEntry(dt, price);
        }
        catch (Exception)
        {
            return null;
        }
    }
}