using Godot;
using System;
using System.IO;
using System.Text.Json;

public partial class SoundControlScript : Control
{
    public float SFX = 50;
    public float Music = 50;
    public float Voices = 50;
    //private string SaveFilePath => ProjectSettings.GlobalizePath("user://SavedSoundSettings.json");
    [Export] Node2D ParentOfVisibility;
    [Export] HScrollBar[] HSB;
    [Export] Label[] labels;

    public override void _Process(double delta)
    {
        if (ParentOfVisibility.Visible == true)
        {
            labels[0].Text = $"Music {HSB[0].Value}%";
            labels[1].Text = $"SFX {HSB[1].Value}%";
            labels[2].Text = $"Voices {HSB[2].Value}%";
        }
    }
    void Button_ACT1()
    {
        /*
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
            GD.Print("Stary plik JSON z ustwieniami dźwiękowymi usunięty.");
        }
        */
        Music = (float)HSB[0].Value;
        SFX = (float)HSB[1].Value;
        Voices = (float)HSB[2].Value;
        //string json = JsonSerializer.Serialize();
        //File.WriteAllText(SaveFilePath, json);
        //GD.Print($"Sound changes saved to JSON: {SaveFilePath}");
    }
}
