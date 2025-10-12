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
    void ShowFadeWarning(string messig)
    {
        Visible = true;
        Text = messig;
        FadeoutTimer.Start();
    }
    void HideLabel()
    {
        Visible = false;
    }
}
