using Godot;
using System;
using System.Runtime;

public partial class TeamFillupBarScript : Control
{
    [Export] NodeCompButtonUni1FuncScript Button;
    PackedScene TeamAddPrefab;
    [Export] public bool AI_Active = false;
    public string teamName = "Team 1";
    public int PawnCount = 0;
    public Color TeamColorCoding = new Color(1,1,1);
    string urchildname = "Team_Fillup";
    MainMenuScript MenuScript;
    [Export] Control ColorPanel;
    [Export] LineEdit TeamNameEdit;
    [Export] TextureButton Colorpickerbutton;
    private int TeamToMenuID = 0;
    private bool colorpickeractive = false;
    public override void _Ready()
    {
        ColorPanel.Visible = false;
        MenuScript = GetTree().Root.GetNode<MainMenuScript>("MainMenu");
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
        TeamNameEdit.PlaceholderText = "Team " + MenuScript.TeamCallInCount;
        //GD.Print("Liczba drużyn .: " + TeamToMenuID);
    }
    public override void _Process(double delta)
    {
        teamName = TeamNameEdit.Text;
        TeamColorCoding = Colorpickerbutton.SelfModulate;// głupie ale jak działa to chuj
    }
    void Button_ACT1()
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
    void TextButton_ACT1()
    {
        ColorPanel.Visible = true;
    }
    void TextButton_ACT2()
    {
        GD.Print("kolor czerwony");
        TeamColorCoding = new Color(Colors.Red);
        Colorpickerbutton.SelfModulate = TeamColorCoding;
        ColorPanel.Visible = false;
    }
    void TextButton_ACT3()
    {
        GD.Print("kolor pomarańczowy");
        TeamColorCoding = new Color(Colors.OrangeRed);
        Colorpickerbutton.SelfModulate = TeamColorCoding;
        ColorPanel.Visible = false;
    }
    void TextButton_ACT4()
    {
        GD.Print("kolor biały");
        TeamColorCoding = new Color(Colors.White);
        Colorpickerbutton.SelfModulate = TeamColorCoding;
        ColorPanel.Visible = false;
    }
    void TextButton_ACT5()
    {
        GD.Print("kolor niebieski");
        TeamColorCoding = new Color(Colors.Blue);
        Colorpickerbutton.SelfModulate = TeamColorCoding;
        ColorPanel.Visible = false;
    }
    void TextButton_ACT6()
    {
        GD.Print("kolor zielony");
        TeamColorCoding = new Color(Colors.Green);
        Colorpickerbutton.SelfModulate = TeamColorCoding;
        ColorPanel.Visible = false;
    }
    void TextButton_ACT7()
    {
        GD.Print("kolor żółty");
        TeamColorCoding = new Color(Colors.Yellow);
        Colorpickerbutton.SelfModulate = TeamColorCoding;
        ColorPanel.Visible = false;
    }
    void TextButton_ACT8()
    {
        GD.Print("kolor brązowy");
        TeamColorCoding = new Color(Colors.SaddleBrown);
        Colorpickerbutton.SelfModulate = TeamColorCoding;
        ColorPanel.Visible = false;
    }
    void TextButton_ACT9()
    {
        GD.Print("kolor różowy");
        TeamColorCoding = new Color(255f,0f,255f); // no i teraz to jest róż 
        Colorpickerbutton.SelfModulate = TeamColorCoding;
        ColorPanel.Visible = false;
    }
    void DisableAddTeamButton()
    {
        Button.Call("OnDisablebutton");
    }
}
