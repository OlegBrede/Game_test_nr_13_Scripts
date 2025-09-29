using Godot;
using System;

public partial class SamplePopUpScript : Node2D
{
    [Export] Label DescriptionLabel;
    GameMNGR_Script GameMenagerScript;
    bool InternalTrueIsRound = false;
    public override void _Ready()
    {
        GameMenagerScript = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");
        Visible = false;
    }
    void PopUpContentsFunc(string PopUpMessageGet, bool TrueIsRound) // prawda to zakończenie rundy (w sensie całej gry), fałsz to zakończenie tury (twojej czy mojej itp)
    {
        Visible = true;
        DescriptionLabel.Text = PopUpMessageGet;
        InternalTrueIsRound = TrueIsRound;
    }
    void Button_ACT1() // cancel
    {
        Visible = false;
    }
    void Button_ACT2() // confirm
    {
        Visible = false;
        if (InternalTrueIsRound == true)
        {
            GameMenagerScript.Call("NextRoundFunc");
        }
        else
        {
            GameMenagerScript.Call("NextTurnFunc");
        }
        
    }
}
