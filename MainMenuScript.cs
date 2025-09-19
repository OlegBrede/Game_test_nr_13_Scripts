using Godot;
using System;

public partial class MainMenuScript : Node2D
{
    [Export] public string SceneToLoad = "res://Scenes/base_test_scene.tscn";
    void Button_ACT1()
    {
        if (SceneToLoad != null)
        {
            GetTree().ChangeSceneToFile(SceneToLoad);
		}
        else
        {
            GD.Print("Nie można załadować sceny");
        }
    }
    void Button_ACT2()
    {
        GD.Print("Przycisk wciśnięty");
    }
    void Button_ACT3()
    {
        GD.Print("Przycisk wciśnięty");
    }
    void Button_ACT4()
    {
        GD.Print("Exiting");
		GetTree().Quit();
    }
}
