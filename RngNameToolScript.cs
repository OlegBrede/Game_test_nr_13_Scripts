using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public partial class RngNameToolScript : Node2D
{
    private Godot.Collections.Dictionary<string, Godot.Collections.Array> namesData;

    public override void _Ready()
    {
        
    }

    public void LoadNames(string path)
    {
        //GD.Print($"[Debug] Próba otwarcia pliku: '{path}'");

        var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"[Debug] Nie udało się otworzyć pliku: {path} (plik nie istnieje?)");
            return;
        }

        string jsonText = file.GetAsText();
        file.Close();
        
        // Wypisz pierwsze 500 znaków, żeby szybko zobaczyć zawartość
        //GD.Print("[Debug] Pierwsze 500 znaków pliku:");
        //GD.Print(jsonText.Length > 500 ? jsonText.Substring(0, 500) + "..." : jsonText);

        var json = new Godot.Json();
        var err = json.Parse(jsonText);
        if (err != Error.Ok)
        {
            GD.PrintErr($"[Debug] Błąd parsowania JSON: {json.GetErrorMessage()}");
            return;
        }

        // Parsing się udał — pokaż co jest w json.Data
        var raw = json.Data.AsGodotDictionary();
        //GD.Print($"[Debug] Surowe klucze JSON: {string.Join(", ", raw.Keys)}");

        // rzutuj i sprawdź konkretne klucze
        namesData = new Godot.Collections.Dictionary<string, Godot.Collections.Array>();
        foreach (var k in raw.Keys)
        {
            try
            {
                string key = k.ToString();
                var arr = (Godot.Collections.Array)raw[key];
                namesData[key] = arr;
                //GD.Print($"[Debug] Klucz '{key}' — typ array, długość = {arr.Count}");
            }
            catch (Exception e)
            {
                GD.PrintErr($"[Debug] Klucz '{k}' nie jest array albo rzutowanie nie powiodło się: {e.Message}");
            }
        }

        // test: czy podstawowe kategorie istnieją
        string[] wanted = new string[] { "male", "female", "surname" };
        foreach (var cat in wanted)
        {
            if (!namesData.ContainsKey(cat))
            {
                GD.PrintErr($"[Debug] Brak kategorii '{cat}' w JSON!");
            }
            else
            {
                //GD.Print($"[Debug] Kategoria '{cat}' OK, elementów: {namesData[cat].Count}");
            }
        }
        
    }

    public string GetRandomName(string category)
    {
        if (namesData == null || !namesData.ContainsKey(category))
        {
            GD.PrintErr($"namesData jest null lub nie ma kategorii {category}");
            return "Error";
        }

        var list = namesData[category];
        int idx = (int)(GD.Randi() % (ulong)list.Count);
        return list[idx].ToString();
    }
    
}
