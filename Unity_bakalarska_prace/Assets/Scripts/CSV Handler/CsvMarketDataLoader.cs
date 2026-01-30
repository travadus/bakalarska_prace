using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class CsvMarketDataLoader
{
    public List<EnergyDataEntry> LoadData(TextAsset csvFile)
    {
        List<EnergyDataEntry> data = new List<EnergyDataEntry>();

        if (csvFile == null)
        {
            Debug.LogError("CsvMarketDataLoader: No file");
            return data;
        }

        string[] lines = csvFile.text.Split('\n');

        // Zaèínáme od 1, abychom pøeskoèili hlavièku
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

    private EnergyDataEntry? ParseLine(string line)
    {
        string[] columns = line.Split(',');

        // Ochrana proti špatným øádkùm
        if (columns.Length < 5) return null;

        try
        {
            // Sloupec 2: Datum (2015-01-01 00:00:00)
            DateTime dt = DateTime.Parse(columns[2]);

            // Sloupec 4: Cena (24.2) - pozor na CultureInfo kvùli teèce
            float price = float.Parse(columns[4], CultureInfo.InvariantCulture);

            return new EnergyDataEntry(dt, price);
        }
        catch (Exception)
        {
            // Tady bychom mohli logovat chybu, ale pøi 80 000 øádcích to mùže zpomalit editor.
            // Prozatím jen pøeskoèíme vadný øádek.
            return null;
        }
    }
}