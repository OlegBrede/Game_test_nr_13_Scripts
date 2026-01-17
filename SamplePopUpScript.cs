using Godot;
using System;

public partial class SamplePopUpScript : Node2D
{
    [Export] Label DescriptionLabel;
    GameMNGR_Script gameMNGR_Script;
    public override void _Ready()
    {
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");
        Visible = false;
    }
    void PopUpContentsFunc(string PopUpMessageGet) // prawda to zakończenie rundy (w sensie całej gry), fałsz to zakończenie tury (twojej czy mojej itp)
    {
        Visible = true;
        DescriptionLabel.Text = PopUpMessageGet;
    }
    void Button_ACT1() // cancel
    {
        Visible = false;
    }
    void Button_ACT2() // confirm
    {
        Visible = false;
        if (gameMNGR_Script.TeamTurnTable.Count <= 1)
        {
            gameMNGR_Script.Call("NextRoundFunc");
        }
        else
        {
            gameMNGR_Script.Call("NextTurnFunc");
        }
        
    }
}
