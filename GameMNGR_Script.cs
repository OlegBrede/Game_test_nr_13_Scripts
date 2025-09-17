using Godot;
using System;

public partial class GameMNGR_Script : Node2D
{
    public static GameMNGR_Script Instance { get; private set; }

    public PawnBaseFuncsScript SelectedPawn { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public void SelectPawn(PawnBaseFuncsScript pawn)
    {
        SelectedPawn = pawn; // możesz też emitować sygnał tutaj jeśli kto inny chce reagować
        GD.Print($"Selected pawn is {SelectedPawn}");
    }

    public void DeselectPawn() => SelectedPawn = null;
}
