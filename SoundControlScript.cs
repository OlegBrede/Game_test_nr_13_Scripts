using Godot;
using System;
using System.IO;
using System.Text.Json;

public partial class SoundControlScript : Control
{
    [Serializable]
    public class SoundSettingsData
    {
        public float music { get; set; }
        public float sfx { get; set; }
        public float voices { get; set; }
    }
    public float SFX = 50;
    public float Music = 50;
    public float Voices = 50;
    private string SaveFilePath => ProjectSettings.GlobalizePath("user://SavedSoundSettings.json");
    [Export] Node2D ParentOfVisibility;
    [Export] HScrollBar[] HSB;
    [Export] Label[] labels;
    public override void _Ready()
    {
        LoadSoundSettings();
    }
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
        Music = (float)HSB[0].Value;
        SFX = (float)HSB[1].Value;
        Voices = (float)HSB[2].Value;

        SoundSettingsData data = new SoundSettingsData
        {
            music = Music,
            sfx = SFX,
            voices = Voices
        };

        string json = JsonSerializer.Serialize(
            data,
            new JsonSerializerOptions { WriteIndented = true }
        );

        File.WriteAllText(SaveFilePath, json);

        GD.Print($"Sound settings saved to: {SaveFilePath}");
    }
    void LoadSoundSettings()
    {
        if (!File.Exists(SaveFilePath))
        {
            GD.Print("No sound settings file found, using defaults.");
            return;
        }

        string json = File.ReadAllText(SaveFilePath);
        SoundSettingsData data = JsonSerializer.Deserialize<SoundSettingsData>(json);

        Music = data.music;
        SFX = data.sfx;
        Voices = data.voices;

        HSB[0].Value = Music;
        HSB[1].Value = SFX;
        HSB[2].Value = Voices;

        GD.Print("Sound settings loaded from JSON.");
    }
}
