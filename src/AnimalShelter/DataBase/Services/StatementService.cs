using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using AnimalShelter.Models;

namespace AnimalShelter.DataBase.Services;

public class StatementService
{
    public List<RacunStavka> ImportFromFile(string filePath)
    {
        var stavke = new List<RacunStavka>();
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Datoteka nije pronađena: {filePath}");

        var lines = File.ReadAllLines(filePath);
        RacunStavka? current = null;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("Id:"))
            {
                current = new RacunStavka();
                var idStr = line.Substring(3).Trim();
                if (int.TryParse(idStr, out var id))
                    current.Id = id;
            }
            else if (line.StartsWith("Datum:") && current != null)
            {
                var dateStr = line.Substring(7).Trim();
                if (DateTime.TryParse(dateStr, out var date))
                    current.Datum = date;
            }
            else if (line.StartsWith("Iznos:") && current != null)
            {
                var amountStr = line.Substring(6).Trim().Replace('.', ',');
                if (decimal.TryParse(amountStr, out var amount))
                    current.Iznos = amount;
            }
            else if (line.StartsWith("Banka:") && current != null)
            {
                current.Banka = line.Substring(6).Trim();
                stavke.Add(current);
                current = null; // završena stavka
            }
        }

        return stavke;
    }
}