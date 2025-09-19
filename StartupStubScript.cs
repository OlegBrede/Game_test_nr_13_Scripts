using Godot;
using System;

public partial class StartupStubScript : Node
{
    GameMNGR_Script gameMNGR_Script;
    public override void _Ready()
    {
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");
        gameMNGR_Script.SetupGameScene();
    }
}
