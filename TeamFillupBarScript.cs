using Godot;
using System;
using System.Runtime;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

public partial class TeamFillupBarScript : Control
{
    [Export] NodeCompButtonUni1FuncScript Button;
    PackedScene TeamAddPrefab;
    [Export] public bool AI_Active = false;
    public string teamName = "Team 1";
    public int PawnCount = 0; // TO DO .: zamienić to tak by liczba pionków wywodziła się z USQA
    public Color TeamColorCoding = new Color(1,1,1);
    string urchildname = "Team_Fillup";
    MainMenuScript MenuScript;
    public List<GameMNGR_Script.UnitSelection> USQA = new List<GameMNGR_Script.UnitSelection>(); // unit selection querry answers (czyli jakie jednostki wybrał gracz)
    [Export] Control ColorPanel;
    [Export] Control SpawnPointPanel;
    [Export] LineEdit TeamNameEdit;
    [Export] TextureButton Colorpickerbutton;
    [Export] CheckBox TeamIsBot;
    [Export] Node2D TFBFAUC; // ThisFuckinButtonForAcceptingUserChoice
    [Export] Node2D TempPawnHolder;
    PackedScene UnitMenuSelectionPath;
    Label SpawnPosNumLabel;
    private int TeamToMenuID = 0; // to jest numerator potrzebny by nie stworzyć więcej niż osiem drużyn
    public int SpawnPosID = 0; // to jest pozycja spawn'u dla danego team 
    private bool colorpickeractive = false;
    public override void _Ready()
    {
        SpawnPosNumLabel = GetNode<Label>("SpawnPosNum");
        MenuScript = GetTree().Root.GetNode<MainMenuScript>("MainMenu");
        TFBFAUC.Visible = false;
        TFBFAUC.GlobalPosition = MenuScript.UnitSelectionCloseHook.GlobalPosition;
        USQA.Clear();
        GD.Print("wyczyszczono starą selekcję pionków");
        ColorPanel.Visible = false;
        SpawnPointPanel.Visible = false;
        TeamAddPrefab = GD.Load<PackedScene>("res://Prefabs/team_fillup_Bar_Prefab.tscn");
        if (GetChildCount() == 0)
        {
            Button.Call("OnChangeButtonLabel", "-", 360);
        }
        else
        {
            Button.Call("OnChangeButtonLabel", "+", 360);
        }
        MenuScript.TeamCallInCount++;
        TeamToMenuID = MenuScript.TeamCallInCount;
        SpawnPosNumLabel.Text = TeamToMenuID.ToString();
        MenuScript.TeamDeploymentZoneChangeAnnouncment(TeamToMenuID,GetInstanceId()); // to już ustawia zewnętrznie porządek listy
        TeamNameEdit.PlaceholderText = "Team " + MenuScript.TeamCallInCount;
        //GD.Print("Liczba drużyn .: " + TeamToMenuID);
    }
    public override void _Process(double delta)
    {
        if (string.IsNullOrWhiteSpace(TeamNameEdit.Text))
        { // trzeba sprawdzić czy jest coś w boksie
            teamName = TeamNameEdit.PlaceholderText;
        }
        else
        {
            teamName = TeamNameEdit.Text;
        }
        TeamColorCoding = Colorpickerbutton.SelfModulate;// głupie ale jak działa to chuj
        AI_Active = TeamIsBot.ButtonPressed;
        SpawnPosNumLabel.Text = SpawnPosID.ToString();
    }
    public void ReciveUnits(int Pcount,float PRad,string PPath) // dostaje unity od UnitMenuSelectionBoxScript (trza powielić dodanie tam)
    {
        GameMNGR_Script.UnitSelection unit = new GameMNGR_Script.UnitSelection();
        unit.ScenePath = PPath;
        unit.Count = Pcount;
        unit.ThisSpecificPawnsRadius = PRad;
        var existingUnit = USQA.Find(u => u.ScenePath == PPath);
        if (existingUnit != null)
        {
            existingUnit.Count = Pcount;
            GD.Print($"Zaktualizowano {PPath}: Count = {existingUnit.Count}");
        }
        else
        {
            USQA.Add(unit);
            GD.Print($"Dodano {Pcount} {unit.ScenePath} do drużyny {teamName}");
        }
        GD.Print($"dodano {Pcount} {unit.ScenePath} do drużyny {teamName}");
        //GD.Print($"PawnCount dla {teamName} jest teraz {PawnCount} powinien być 0 "); // chujowy kod, zły chujowy kod
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

    void Button_ACT1() // dodanie nowej drużyny
    {
        Control AddTeam = TeamAddPrefab.Instantiate<Control>();
        TeamFillupBarScript AddTeamScript = AddTeam as TeamFillupBarScript;

        if (!HasNode(urchildname)) // sprawdzamy czy node poniżej jest 
        {
            if (MenuScript.TeamCallInCount < 8)
            {
                AddChild(AddTeam);
                urchildname = AddTeam.Name;
                Button.Call("OnChangeButtonLabel", "-", 360);
                AddTeam.GlobalPosition = GlobalPosition;
                AddTeam.GlobalPosition = new Vector2(AddTeam.GlobalPosition.X, AddTeam.GlobalPosition.Y + 375);
            }
            else
            {
                DisableAddTeamButton();
                Button.Call("OnChangeButtonLabel", " ", 360);
                GD.Print("Nie można dodać kolejnej drużyny, osiągnięto maksymalną arbitralną liczbę drużyn");
            }
        }
        else
        {
            Button.Call("OnChangeButtonLabel", "+", 360);
            // na sam koniec usuwasz i nie możesz mieć po tym referencji do skryptu AddTeamScript bo zapewne wyjdzie null
            foreach (Node Child in GetChildren())
            {
                if (Child.Name == urchildname)
                {
                    Child.QueueFree();
                }

            }
            MenuScript.TeamCallInCount = TeamToMenuID;
            //GD.Print("Liczba drużyn .: " + TeamToMenuID);
        }
    }
    void Button_ACT2() // otwarcie okna dodawania jednostek
    {
        GD.Print("Reset Pawncount");
        PawnCount = 0;
        TFBFAUC.Visible = true;
        var UnitSelectPrefabs = LoadUnitPrefabs("res://Prefabs/Units/");
        UnitMenuSelectionPath = GD.Load<PackedScene>("res://Prefabs/UnitMenuSelectionBoxPrefab.tscn");
        MenuScript.ThisTeaMsunitListRoot.Visible = true;
        Dictionary<string, Control> unitBoxes = new();
        foreach (var prefab in UnitSelectPrefabs)
        {
            Control PawnSelect = UnitMenuSelectionPath.Instantiate<Control>();
            Node unitsINFO = prefab.Instantiate(); // instancja pionka na chwile
            TempPawnHolder.AddChild(unitsINFO);
            PawnBaseFuncsScript unitScript = unitsINFO as PawnBaseFuncsScript;
            PawnSelect.Call("RecivePawnInfo", unitScript.UnitType, unitScript.ProfilePick, unitScript.PV, unitScript.Descriptor,unitScript.WeaponDamage); // ze skryptu bieremy info
            GD.Print($"Objętość pionka to {unitScript.ObjętośćPionka}");
            MenuScript.UnitList.AddChild(PawnSelect); // dodajemy box

            PawnSelect.Call("WhosBitchin", this, unitScript.ObjętośćPionka, prefab.ResourcePath); // wysyłamy zarówno godność gracza jak i ścierzkę do pionka
            unitsINFO.QueueFree(); // z pamięci precz
            unitBoxes[prefab.ResourcePath] = PawnSelect;
        }
        foreach (GameMNGR_Script.UnitSelection SU in USQA)
        {
            if (unitBoxes.TryGetValue(SU.ScenePath, out var box))
            {
                box.Call("ReciveTeamCompInfo", SU.Count);
                GD.Print($"Powiązano {SU.ScenePath} z boxem, Count = {SU.Count}");
            }
            else
            {
                GD.PrintErr($"Nie znaleziono boxa dla {SU.ScenePath}");
            }
        }
        // tu musisz zadzwonić do UnitMenuSelectionBoxScript i powiedzieć hello tu jest twoja jednostka, więc chyba tworzenie tego musi zejść na tworzenie kilku instancji UnitMenuSelectionBoxScript z tego tu skryptu, inaczej tego nie widzę 
    }
    void Button_ACT3() // to je głupie, ale chuj (zamknięcie i potwierdzenie okna wybierania jednostek)
    {
        MenuScript.ThisTeaMsunitListRoot.Visible = false;
        foreach (Node ShitMenuButtons in MenuScript.UnitList.GetChildren())
        {
            UnitMenuSelectionBoxScript UMSBS = ShitMenuButtons as UnitMenuSelectionBoxScript;
            UMSBS.Call("ParseTeamCompInfo");
            PawnCount = PawnCount + UMSBS.ThisUnitsCount;
            ShitMenuButtons.QueueFree();
        }
        //GD.Print("zwalnianie tej głupiej listy");
        TFBFAUC.Visible = false;
    }
    // Widok miejsca spawnu
    void Button_ACT4()
    {
        SpawnPointPanel.Visible = true;
    }
    // ctrl+c ctrl+v... co ja bym bez ciebie zrobił ? ... bym napisał leprzy kod zapewne
    // wybór miejsca spawn'u
    void Button_ACT5()
    {
        MenuScript.TeamDeploymentZoneChangeAnnouncment(1, GetInstanceId());
        SpawnPointPanel.Visible = false;
    }
    void Button_ACT6()
    {
        MenuScript.TeamDeploymentZoneChangeAnnouncment(2, GetInstanceId());
        SpawnPointPanel.Visible = false;
    }
    void Button_ACT7()
    {
        MenuScript.TeamDeploymentZoneChangeAnnouncment(3, GetInstanceId());
        SpawnPointPanel.Visible = false;
    }
    void Button_ACT8()
    {
        MenuScript.TeamDeploymentZoneChangeAnnouncment(4, GetInstanceId());
        SpawnPointPanel.Visible = false;
    }
    void Button_ACT9()
    {
        MenuScript.TeamDeploymentZoneChangeAnnouncment(5, GetInstanceId());
        SpawnPointPanel.Visible = false;
    }
    void Button_ACT10()
    {
        MenuScript.TeamDeploymentZoneChangeAnnouncment(6, GetInstanceId());
        SpawnPointPanel.Visible = false;
    }
    void Button_ACT11()
    {
        MenuScript.TeamDeploymentZoneChangeAnnouncment(7, GetInstanceId());
        SpawnPointPanel.Visible = false;
    }
    void Button_ACT12()
    {
        MenuScript.TeamDeploymentZoneChangeAnnouncment(8, GetInstanceId());
        SpawnPointPanel.Visible = false;
    }
    // Kolor Drużyny
    void TextButton_ACT1()
    {
        ColorPanel.Visible = true;
    }
    void TextButton_ACT2()
    {
        //GD.Print("kolor czerwony");
        TeamColorCoding = new Color(Colors.Red);
        Colorpickerbutton.SelfModulate = TeamColorCoding;
        ColorPanel.Visible = false;
    }
    void TextButton_ACT3()
    {
        //GD.Print("kolor pomarańczowy");
        TeamColorCoding = new Color(Colors.OrangeRed);
        Colorpickerbutton.SelfModulate = TeamColorCoding;
        ColorPanel.Visible = false;
    }
    void TextButton_ACT4()
    {
        //GD.Print("kolor biały");
        TeamColorCoding = new Color(Colors.White);
        Colorpickerbutton.SelfModulate = TeamColorCoding;
        ColorPanel.Visible = false;
    }
    void TextButton_ACT5()
    {
        //GD.Print("kolor niebieski");
        TeamColorCoding = new Color(Colors.Blue);
        Colorpickerbutton.SelfModulate = TeamColorCoding;
        ColorPanel.Visible = false;
    }
    void TextButton_ACT6()
    {
        //GD.Print("kolor zielony");
        TeamColorCoding = new Color(Colors.Green);
        Colorpickerbutton.SelfModulate = TeamColorCoding;
        ColorPanel.Visible = false;
    }
    void TextButton_ACT7()
    {
        //GD.Print("kolor żółty");
        TeamColorCoding = new Color(Colors.Yellow);
        Colorpickerbutton.SelfModulate = TeamColorCoding;
        ColorPanel.Visible = false;
    }
    void TextButton_ACT8()
    {
        //GD.Print("kolor brązowy");
        TeamColorCoding = new Color(Colors.SaddleBrown);
        Colorpickerbutton.SelfModulate = TeamColorCoding;
        ColorPanel.Visible = false;
    }
    void TextButton_ACT9()
    {
        //GD.Print("kolor różowy");
        TeamColorCoding = new Color(255f, 0f, 255f); // no i teraz to jest róż 
        Colorpickerbutton.SelfModulate = TeamColorCoding;
        ColorPanel.Visible = false;
    }
    // zakaz dodania większej ilości drużyn
    void DisableAddTeamButton()
    {
        Button.Call("OnDisablebutton");
    }
}
