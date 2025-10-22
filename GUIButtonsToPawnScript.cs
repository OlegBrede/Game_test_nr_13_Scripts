using Godot;
using System;

public partial class GUIButtonsToPawnScript : Node2D
{
    GameMNGR_Script gameMNGR_Script;
    Node2D Panel;
    [Export] Node2D Confirmations;
    [Export] Node2D Actions;
    int Parameter = 0;
    public override void _Ready()
    {
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");
        Actions.Visible = false;
        Confirmations.Visible = false;
    }
    public void ShowActions()
    {
        Actions.Visible = true;
        Confirmations.Visible = false;
    }
    public void PALO()
    {
        Confirmations.Visible = true;
    }
    void Button_ACT1()
    {
        if (gameMNGR_Script.SelectedPawn != null)
        {
            if (gameMNGR_Script.SelectedPawn.MP > 0)
            {
                gameMNGR_Script.SelectedPawn.Call("PlayerActionPhone", "Player_ACT_Punch", 0);
                Parameter = 3;
                Actions.Visible = false;
            }
        }
    }
    void Button_ACT2()
    {
        if (gameMNGR_Script.SelectedPawn != null)
        {
            if (gameMNGR_Script.SelectedPawn.MP > 0)
            {
                gameMNGR_Script.SelectedPawn.Call("PlayerActionPhone", "Player_ACT_Use", 0);
                Parameter = 4;
                Actions.Visible = false;
            }
        }
    }
    void Button_ACT3()
    {
        if (gameMNGR_Script.SelectedPawn != null)
        {
            if (gameMNGR_Script.SelectedPawn.MP > 0)
            {
                gameMNGR_Script.SelectedPawn.Call("PlayerActionPhone", "Player_ACT_Shoot", 0);
                Parameter = 2;
                Actions.Visible = false;
            }
        }
    }
    void Button_ACT4()
    {
        if (gameMNGR_Script.SelectedPawn != null)
        {
            if (gameMNGR_Script.SelectedPawn.MP > 0)
            {
                gameMNGR_Script.SelectedPawn.Call("PlayerActionPhone", "Player_ACT_Move", 0);
                Parameter = 1;
                Actions.Visible = false;
            }
        }
    }
    void Button_ACT5() // Decline
    {
        if (gameMNGR_Script.SelectedPawn != null)
        {
            if (gameMNGR_Script.SelectedPawn.MP > 0)
            {
                gameMNGR_Script.SelectedPawn.Call("PlayerActionPhone", "Player_ACT_Decline", Parameter);
                Confirmations.Visible = false;
            }
        }
    }
    void Button_ACT6() // Accept
    {
        if (gameMNGR_Script.SelectedPawn != null)
        {
            if (gameMNGR_Script.SelectedPawn.MP > 0)
            {
                gameMNGR_Script.SelectedPawn.Call("PlayerActionPhone", "Player_ACT_Confirm", Parameter);
                Confirmations.Visible = false;
            }
        }
    }
}
