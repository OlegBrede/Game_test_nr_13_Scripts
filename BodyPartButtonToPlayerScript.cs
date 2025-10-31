using Godot;
using System;

public partial class BodyPartButtonToPlayerScript : Button
{
    PawnPlayerController pawnPlayerController;
    GameMNGR_Script gameMNGR_Script;
    int ChoosenIndex = 0;
    public override void _Ready()
    {
        Connect("pressed", new Callable(this, nameof(OnButtonPressed)));
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");
    }
    void PrimeButton(int SetIndex,string SetName,PawnPlayerController PPC)
    {
        ChoosenIndex = SetIndex;
        Text = SetName;
        pawnPlayerController = PPC;
        GD.Print($"Dodano część {Text} o indeksie {ChoosenIndex}");
    }
    void OnButtonPressed()
    {
        pawnPlayerController.Call("AimedShotChosenTargetListTrigger", ChoosenIndex);
        gameMNGR_Script.HideListPupUp();
        pawnPlayerController = null;
    }
}
