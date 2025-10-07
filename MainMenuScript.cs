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
    [Export] public Node2D ThisTeaMsunitListRoot;
    [Export] Label LowPlaerCountWarnin;
    private string SaveFilePath => ProjectSettings.GlobalizePath("user://teams.json");
    private int VisibleMenuScreenID;
    public int TeamCallInCount = 0;
    List<TeamFillupBarScript> TeamQuerryConfinginfo = new List<TeamFillupBarScript>();
    public override void _Ready()
    {
        ThisTeaMsunitListRoot.Visible = false;
        GameSetupNode.Visible = false;
        OptionsNode.Visible = false;
        Creditsnode.Visible = false;
        // Otwieranie Prefabów jednostek do selekcji w custom
    }
    public override void _Process(double delta)
    {
        if (TeamCallInCount < 2 && GameSetupNode.Visible == true) {
            LowPlaerCountWarnin.Visible = true;
        } else {
            LowPlaerCountWarnin.Visible = false;
        }
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
            if (TeamCallInCount >= 2) // to do .: dodaj więcej parametrów które blokują start. Chwilowo potrzebne blokady to .: - nie można nazwać dwóch innych drużyn tą samą nazwą 
            {
                OnStartPressed();
            }
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
    void Button_ACT5() // zamykanie okna wyboru jednostek dla danej drużyny
    {
        ThisTeaMsunitListRoot.Visible = false;
        GD.Print("ekszit paper doe");
    }
    List<PackedScene> LoadUnitPrefabs(string path) // to do, trza okomentować
    {
        var dir = DirAccess.Open(path);
        var prefabs = new List<PackedScene>();

        if (dir == null)
        {
            GD.PrintErr($"Nie można otworzyć folderu: {path}");
            return prefabs;
        }

        dir.ListDirBegin();
        string fileName = dir.GetNext();

        while (fileName != "")
        {
            if (!dir.CurrentIsDir() && fileName.EndsWith(".tscn"))
            {
                string fullPath = $"{path}/{fileName}";
                PackedScene scene = GD.Load<PackedScene>(fullPath);

                if (scene != null)
                {
                    prefabs.Add(scene);
                    GD.Print($"Załadowano prefab: {fileName}");
                }
            }
            fileName = dir.GetNext();
        }

        dir.ListDirEnd();
        return prefabs;
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
        CollectDescendants(TeamFillupBucket); // rekursywnie zbieramy wszyskie dzieci

        GD.Print("Znaleziono drużyn: " + TeamQuerryConfinginfo.Count);

        foreach (var team in TeamQuerryConfinginfo)
        {
            GD.Print("Team: " + team.teamName + " | AI: " + team.AI_Active + " | PawnCount: " + team.PawnCount);
            cfg.teams.Add(new GameMNGR_Script.TeamConfig
            {
                name = team.teamName,
                team_colour = team.TeamColorCoding,
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
