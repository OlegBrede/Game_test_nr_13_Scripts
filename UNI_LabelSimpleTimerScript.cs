using Godot;
using System;

public partial class UNI_LabelSimpleTimerScript : Label
{
    Timer FadeoutTimer;
    public override void _Ready()
    {
        Visible = false;
        FadeoutTimer = GetNode<Timer>("FadeoutTimer");
        FadeoutTimer.Timeout += HideLabel;
    }
    void ShowFadeWarning(string TeamWithNoPawns)
    {
        Visible = true;
        Text = $"TEAM {TeamWithNoPawns} NEEDS AT LEAST ONE PAWN ";
        FadeoutTimer.Start();
    }
    void HideLabel()
    {
        Visible = false;
    }
}
