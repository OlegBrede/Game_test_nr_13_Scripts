using Godot;
using System;

public partial class SamplePopUpScript : Node2D
{
    [Export] Label DescriptionLabel;
    GameMNGR_Script GameMenagerScript;
    public override void _Ready()
    {
        GameMenagerScript = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");
        Visible = false;
    }
    void PopUpContentsFunc(string PopUpMessageGet)
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
        GameMenagerScript.Call("NextRoundFunc");
    }
}
