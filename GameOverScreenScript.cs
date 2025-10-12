using Godot;
using System;

public partial class GameOverScreenScript : Node2D
{
    string SceneToLoad = "res://Scenes/main_menu.tscn";
    Label WinLabel;
    public override void _Ready()
    {
        WinLabel = GetNode<Label>("Label");
        if (GameMNGR_Script.Instance.Winner != null)
        {
            WinLabel.Text = $"GAME OVER\n{GameMNGR_Script.Instance.Winner} WON";
        }
        else
        {
            WinLabel.Text = "NO ONE WINS \n FUCK OFF";
        }
    }
    void Button_ACT1()
    {
        GetTree().ChangeSceneToFile(SceneToLoad);
    }
}
