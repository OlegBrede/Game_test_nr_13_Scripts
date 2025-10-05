using Godot;
using System;

public partial class GameOverScreenScript : Node2D
{
    string SceneToLoad = "res://Scenes/main_menu.tscn";
    void Button_ACT1()
    {
        GetTree().ChangeSceneToFile(SceneToLoad);
    }
}
