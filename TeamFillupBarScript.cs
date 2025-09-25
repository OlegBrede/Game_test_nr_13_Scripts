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
    string urchildname = "Team_Fillup";
    [Export] LineEdit TeamNameEdit;
    public override void _Ready()
    {
        TeamAddPrefab = GD.Load<PackedScene>("res://Prefabs/team_fillup_Bar_Prefab.tscn");

        if (GetChildCount() == 0)
        {
            Button.Call("OnChangeButtonLabel", "-", 360);
        }
        else
        {
            Button.Call("OnChangeButtonLabel", "+", 360);
        }
    }
    public override void _Process(double delta)
    {
        teamName = TeamNameEdit.Text;
    }
    void Button_ACT1()
    {
        Control AddTeam = TeamAddPrefab.Instantiate<Control>();
        TeamFillupBarScript AddTeamScript = AddTeam as TeamFillupBarScript;
        
        if (!HasNode(urchildname)) // sprawdzamy czy node poniżej jest 
        {
            AddChild(AddTeam);
            urchildname = AddTeam.Name;
            Button.Call("OnChangeButtonLabel", "-", 360);
            AddTeam.GlobalPosition = GlobalPosition; 
            AddTeam.GlobalPosition = new Vector2(AddTeam.GlobalPosition.X, AddTeam.GlobalPosition.Y + 375);
        }
        else
        {
            Button.Call("OnChangeButtonLabel", "+", 360);
            // na sam koniec usuwasz i nie możesz mieć po tym referencji do skryptu AddTeamScript bo zapewne wyjdzie null
            foreach (Node Child in GetChildren()) {
                if (Child.Name == urchildname) {
                    Child.QueueFree();
                }
                  
            }
        }
    }
}
