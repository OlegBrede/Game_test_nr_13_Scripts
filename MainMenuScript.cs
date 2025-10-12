using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

public partial class MainMenuScript : Node2D
{
    [Export] public string SceneToLoad = "res://Scenes/base_test_scene.tscn";
    [Export] Node2D GameSetupNode;
    [Export] Node2D OptionsNode;
    [Export] Node2D Creditsnode;
    [Export] Node2D TeamFillupBucket;
    [Export] public Node2D UnitSelectionCloseHook;
    [Export] public Node2D ThisTeaMsunitListRoot;
    [Export] Label LowPlaerCountWarnin;
    [Export] Label NoPawnsWarning;
    [Export] public Control UnitList;
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
    private void OnStartPressed() // tu tworzymy JSON i otwieramy gre
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
        // Sprawdzenie duplikatów nazw drużyn
        var duplicateNames = TeamQuerryConfinginfo
            .GroupBy(t => t.teamName)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicateNames.Count > 0)
        {
            GD.Print($"Wykryto drużyny o zduplikowanych nazwach: {string.Join(", ", duplicateNames)}");
            NoPawnsWarning.Call("ShowFadeWarning", $"TEAMS CANNOT SHARE NAMES, CHANGE {string.Join(", ", duplicateNames)}");
            return;
        }
        foreach (var team in TeamQuerryConfinginfo)
        {
            GD.Print("Team: " + team.teamName + " | AI: " + team.AI_Active + " | PawnCount: " + team.PawnCount);
            cfg.teams.Add(new GameMNGR_Script.TeamConfig
            {
                name = team.teamName,
                team_colour = team.TeamColorCoding,
                AI_Active = team.AI_Active,
                PawnCount = team.PawnCount,
                UnitsForThisTeam = team.USQA
            });
            if (team.USQA.Count == 0) { // tu też jest break na włączenie startu gry, ta wiem powinno być uniform ale jestem zmęczony, będzie to stwarzać problem to wtedy się na mnie złość 
                GD.Print($"Drużyna {team.teamName} nie ma pionków do gry");
                NoPawnsWarning.Call("ShowFadeWarning",$"TEAM {team.teamName} NEEDS AT LEAST ONE PAWN ");
                return;
            }
        }

        string json = JsonSerializer.Serialize(cfg);
        File.WriteAllText(SaveFilePath, json);

        GD.Print($"Zapisano JSON: {SaveFilePath}");
        // Przełącz scenę (menu zostanie usunięte z drzewa)
        GetTree().ChangeSceneToFile(SceneToLoad);
    }
}
