using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public partial class PawnSpawnerScript : Node2D
{
    GameMNGR_Script gameMNGR_Script;
    
    private string SaveFilePath => ProjectSettings.GlobalizePath("user://teams.json");
    PackedScene PawnScene;
    Node2D PawnBucketRef;
    RandomNumberGenerator RNGGEN = new RandomNumberGenerator();
    [Export] RngNameToolScript rngNameToolScript;
    [Export] Node2D Spawn1;
    [Export] Node2D Spawn2;
    [Export] Node2D Spawn3;
    [Export] Node2D Spawn4;
    [Export] Node2D Spawn5;
    [Export] Node2D Spawn6;
    [Export] Node2D Spawn7;
    [Export] Node2D Spawn8;
    private float ThisSpecificPawnsRadius = 0;
    private float ThisSpecificSpawnWidth = 1000;
    private float ThisSpecificSpawnHeight = 1000;
    private float []ASS = {0,0}; // Avalabule Spawn Space (x,y)

    public override void _Ready()
    {
        RNGGEN.Randomize();
    }

    void SpawnSelectedPawns()
    {
        rngNameToolScript.LoadNames("res://Mem Bank/RNGNameList.json"); // pierdole
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");
        PawnBucketRef = GetTree().Root.GetNode<Node2D>("BaseTestScene/UnitsBucket");
        if (!File.Exists(SaveFilePath))
        {
            GD.Print("Brak pliku JSON!");
            return;
        }
        string json = File.ReadAllText(SaveFilePath);
        var cfg = JsonSerializer.Deserialize<GameMNGR_Script.GameConfig>(json);

        foreach (var team in cfg.teams)
        {
            bool iniLastPawnSpawnAllowence = false;
            GD.Print($"Drużyna: {team.name}");
            foreach (var TeamsPawn in team.UnitsForThisTeam)
            {
                bool iniFirstPawnRadius = false;
                for (int i = 0; i < TeamsPawn.Count; i++)
                {
                    if (iniLastPawnSpawnAllowence == false)
                    {
                        PawnScene = GD.Load<PackedScene>(TeamsPawn.ScenePath);
                        Node2D Pawn = PawnScene.Instantiate<Node2D>();
                        PawnBucketRef.AddChild(Pawn);
                        PawnBaseFuncsScript PawnScript = Pawn as PawnBaseFuncsScript;
                        // potrzebne do przypisania pionkowi pozycji
                        if (iniFirstPawnRadius == false)
                        {
                            ThisSpecificPawnsRadius = PawnScript.ObjętośćPionka;
                            GD.Print($"Prekalkulowana objętość Pionka wynosi {ThisSpecificPawnsRadius}");
                            ASS[0] = ThisSpecificPawnsRadius;
                            ASS[1] = ThisSpecificPawnsRadius;
                            iniFirstPawnRadius = true;
                        }
                        // ustawienie parametrów pionka 
                        Pawn.Call("SetTeam", team.name, team.team_colour);
                        Pawn.Call("ActivateCollision");
                        Pawn.Call("DeleteUnusedControlNodes", team.AI_Active);
                        
                        int Gender = RNGGEN.RandiRange(0, 1);
                        string Category;
                        string Surname;
                        if (Gender == 1)
                        {
                            Category = "male";
                            Surname = rngNameToolScript.GetRandomName("surname");
                        }
                        else
                        {
                            Category = "female";
                            Surname = rngNameToolScript.GetRandomName("surname");
                            char lastChar = Surname[Surname.Length - 1];
                            // jeśli kończy się na "i" – zamień na "a"
                            if (lastChar == 'i')
                            {
                                Surname = Surname.Substring(0, Surname.Length - 1) + "a";
                            }
                            if (lastChar == 'y')
                            {
                                Surname = Surname.Substring(0, Surname.Length - 1) + "a";
                            }
                        }
                        Pawn.Call("Namechange", rngNameToolScript.GetRandomName(Category) + " " + Surname);
                        if (team.Spawn_ID != 9) // dziewięć to numer dowolnego układania pionków
                        {
                            if (ASS[0] > ThisSpecificSpawnWidth)
                            {
                                GD.Print($"Doszło do resetu bo {ASS[0]} przekroczyło {ThisSpecificSpawnWidth}");
                                ASS[0] = ThisSpecificPawnsRadius;
                                ASS[1] = ASS[1] + (ThisSpecificPawnsRadius * 2);
                            }
                            Pawn.GlobalPosition = new Vector2(SpawnPointPos(team.Spawn_ID).X + ASS[0], SpawnPointPos(team.Spawn_ID).Y + ASS[1]);
                            ASS[0] = ASS[0] + (PawnScript.ObjętośćPionka * 2);// przesunięcie w prawo
                            GD.Print($"ASS[0] to {ASS[0]} ASS[1] to {ASS[1]}");
                        }
                        else
                        {
                            GD.Print("Ten gracz wybrał spawn manualny musi poczekać na rozstawianie, TO DO .: zaprogramuj fazę rozstawiania");
                        }
                        if (ThisSpecificSpawnHeight < ASS[1] + ThisSpecificPawnsRadius) // nieefektywne bo kalkulacja czy pionek tu wogóle może być nie powinna uwzględniać całego procesu układania pionka na miejsce
                        // chyba że znajdę sposób na to jak przywołać danego pionmka z poowrotem na planszę to wtedy będzie miało to sens 
                        {
                            GD.Print("Więcej pionków się nie zmieści");
                            iniLastPawnSpawnAllowence = true;
                            Pawn.QueueFree();
                        }
                        //GD.Print($"This pawns global pos .: {Pawn.GlobalPosition}");
                    }
                    // inicjalizacja pionka
                }
            }
        }
    }
    Vector2 SpawnPointPos(int ChosenSpawnIDNum)
    {
        switch (ChosenSpawnIDNum)
        {
            case 1:
                ThisSpecificSpawnWidth = SpawnWidthCalcFunc(Spawn1)[0]; // ponieważ Vector2 topLeft = Kwadrat.Position; nie zwraca lewego górnego z jakiegoś powodu ?
                ThisSpecificSpawnHeight = SpawnWidthCalcFunc(Spawn1)[1];
                return Spawn1.GlobalPosition;
            case 2:
                ThisSpecificSpawnWidth = SpawnWidthCalcFunc(Spawn2)[0];
                ThisSpecificSpawnHeight = SpawnWidthCalcFunc(Spawn2)[1];
                return Spawn2.GlobalPosition;
            case 3:
                ThisSpecificSpawnWidth = SpawnWidthCalcFunc(Spawn3)[0];
                ThisSpecificSpawnHeight = SpawnWidthCalcFunc(Spawn3)[1];
                return Spawn3.GlobalPosition;
            case 4:
                ThisSpecificSpawnWidth = SpawnWidthCalcFunc(Spawn4)[0];
                ThisSpecificSpawnHeight = SpawnWidthCalcFunc(Spawn4)[1];
                return Spawn4.GlobalPosition;
            case 5:
                ThisSpecificSpawnWidth = SpawnWidthCalcFunc(Spawn5)[0];
                ThisSpecificSpawnHeight = SpawnWidthCalcFunc(Spawn5)[1];
                return Spawn5.GlobalPosition;
            case 6:
                ThisSpecificSpawnWidth = SpawnWidthCalcFunc(Spawn6)[0];
                ThisSpecificSpawnHeight = SpawnWidthCalcFunc(Spawn6)[1];
                return Spawn6.GlobalPosition;
            case 7:
                ThisSpecificSpawnWidth = SpawnWidthCalcFunc(Spawn7)[0];
                ThisSpecificSpawnHeight = SpawnWidthCalcFunc(Spawn7)[1];
                return Spawn7.GlobalPosition;
            case 8:
                ThisSpecificSpawnWidth = SpawnWidthCalcFunc(Spawn8)[0];
                ThisSpecificSpawnHeight = SpawnWidthCalcFunc(Spawn8)[1];
                return Spawn8.GlobalPosition;
            default:
                GD.Print("spawn nie został ustalony");
                return new Vector2(0, 0);
        }
    }
    private float []SpawnWidthCalcFunc(Node2D SpawnNode)
    {
        Area2D SpawnArea2D = SpawnNode.GetNode<Area2D>("AllowedSpawnArea");
        CollisionShape2D SpawnShape = SpawnArea2D.GetNode<CollisionShape2D>("CollisionShape2D");
        RectangleShape2D rect = (RectangleShape2D)SpawnShape.Shape;
        float width = rect.Size.X; // mnożysz bo rozmiar kwadratu ustalany jest od granicy ze środkiem, czyli rect = 5x5 jest 10x10 bo krawędź 5 jest o 5 daleka od 0 (środku/a)
        float height = rect.Size.Y;
        return new float[] { width, height };
    }
}
