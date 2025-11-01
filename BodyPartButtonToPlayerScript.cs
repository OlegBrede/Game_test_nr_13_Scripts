using Godot;
using System;
public partial class BodyPartButtonToPlayerScript : Button
{
    PawnPlayerController pawnPlayerController;
    GameMNGR_Script gameMNGR_Script;
    int ChoosenIndex = 0;
    float ShownProbability = 0;
    public override void _Ready()
    {
        Connect("pressed", new Callable(this, nameof(OnButtonPressed)));
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");
    }
    void PrimeButton(int SetIndex, string SetName,float probability, PawnPlayerController PPC)
    {
        ChoosenIndex = SetIndex;
        ShownProbability = probability;
        float ShownPrecent = ShownProbability * 100;
        Text = $"{SetName}  {Mathf.RoundToInt(ShownPrecent)}%";
        //int Precent = Mathf.RoundToInt(100f - (ShootingFinalDiceVal * 10f));
        pawnPlayerController = PPC;
        //GD.Print($"Dodano część {Text} o indeksie {ChoosenIndex}");
    }
    void OnButtonPressed()
    {
        pawnPlayerController.Call("AimedShotChosenTargetListTrigger", ChoosenIndex,ShownProbability);
        gameMNGR_Script.HideListPupUp();
        pawnPlayerController = null;
    }
}
