using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

public partial class MainMenuScript : Node2D
{
    [Export] public string SceneToLoad = "res://Scenes/base_test_scene.tscn";
    [Export] Node2D GameSetupNode;
    [Export] Node2D OptionsNode;
    [Export] Node2D Creditsnode;
    [Export] Node2D TeamFillupBucket;
    private string SaveFilePath => ProjectSettings.GlobalizePath("user://teams.json");
    private int VisibleMenuScreenID;
    List<TeamFillupBarScript> TeamQuerryConfinginfo = new List<TeamFillupBarScript>();
    public override void _Ready()
    {
        GameSetupNode.Visible = false;
        OptionsNode.Visible = false;
        Creditsnode.Visible = false;
    }
    public override void _Process(double delta)
    {
        switch (VisibleMenuScreenID)
        {
            case 1:
                GameSetupNode.Visible = true;
                OptionsNode.Visible = false;
                Creditsnode.Visible = false;
                break;
                case 2:
                GameSetupNode.Visible = false;
                OptionsNode.Visible = true;
                Creditsnode.Visible = false;
                break;
                case 3:
                GameSetupNode.Visible = false;
                OptionsNode.Visible = false;
                Creditsnode.Visible = true;
                break;
            default:
                GameSetupNode.Visible = false;
                OptionsNode.Visible = false;
                Creditsnode.Visible = false;
                break;
        }
    }
    void Button_ACT0()
    {
        if (VisibleMenuScreenID == 0)
        {
            VisibleMenuScreenID = 1;
        }
        else
        {
            VisibleMenuScreenID = 0;
        }
    }
    void Button_ACT1()
    {
        if (SceneToLoad != null)
        {
            OnStartPressed();
        }
        else
        {
            GD.Print("Nie można załadować sceny");
        }
    }
    void Button_ACT2()
    {
        //GD.Print("Przycisk opcje");
        if (VisibleMenuScreenID == 0)
        {
            VisibleMenuScreenID = 2;
        }
        else
        {
            VisibleMenuScreenID = 0;
        }
    }
    void Button_ACT3()
    {
        //GD.Print("Przycisk credits");
        if (VisibleMenuScreenID == 0)
        {
            VisibleMenuScreenID = 3;
        }
        else
        {
            VisibleMenuScreenID = 0;
        }
    }
    void Button_ACT4()
    {
        GD.Print("Exiting");
        GetTree().Quit();
    }
    void CollectDescendants(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is TeamFillupBarScript teamScript)
            {
                TeamQuerryConfinginfo.Add(teamScript);
                CollectDescendants(child); // rekurencja
            }
        }
    }
    private void OnStartPressed()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
            GD.Print("Stary plik JSON usunięty.");
        }
        var cfg = new GameMNGR_Script.GameConfig();
        TeamQuerryConfinginfo.Clear(); // zawsze najpierw czyścimy listę
        CollectDescendants(TeamFillupBucket);

        GD.Print("Znaleziono drużyn: " + TeamQuerryConfinginfo.Count);

        foreach (var team in TeamQuerryConfinginfo)
        {
            GD.Print("Team: " + team.teamName + " | AI: " + team.AI_Active + " | PawnCount: " + team.PawnCount);
            cfg.teams.Add(new GameMNGR_Script.TeamConfig
            {
                name = team.teamName,
                AI_Active = team.AI_Active,
                PawnCount = team.PawnCount // zakładam, że masz pole/prop w node
            });
        }

        string json = JsonSerializer.Serialize(cfg);
        File.WriteAllText(SaveFilePath, json);

        GD.Print($"Zapisano JSON: {SaveFilePath}");
        // Przełącz scenę (menu zostanie usunięte z drzewa)
        GetTree().ChangeSceneToFile(SceneToLoad);
    }
}
