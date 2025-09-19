using Godot;
using System;

public partial class GameMNGR_Script : Node2D
{
    public int Round = 0;
    public int Turn = 1;
    [Export] NodePath PopUpPath;
    public static GameMNGR_Script Instance { get; private set; }
    public PawnBaseFuncsScript SelectedPawn { get; private set; }
    Node2D PopUpRef;
    SamplePopUpScript PopUpRefScript;
    public bool SceneSetup = false;
    Label GameInfoLabelRef;
    CanvasLayer CamUICanvasRef;
    public bool SetupDone = false;
    public void SetupGameScene()
    {
        Round = 1;
        Instance = this;
        SceneSetup = true;
        Vector2I windowSize = DisplayServer.WindowGetSize();
        CamUICanvasRef = GetTree().Root.GetNode<CanvasLayer>("BaseTestScene/Camera2D/CanvasLayer");
        CamUICanvasRef.Offset = new Vector2(windowSize.X / 2, windowSize.Y / 2);
        PopUpRef = GetTree().Root.GetNode<Node2D>("BaseTestScene/Camera2D/CanvasLayer/SamplePopUp");
        GameInfoLabelRef = GetTree().Root.GetNode<Label>("BaseTestScene/Camera2D/CanvasLayer/GameInfoLabel");
        PopUpRefScript = PopUpRef as SamplePopUpScript;
        SetupDone = true;
    }
    public void SelectPawn(PawnBaseFuncsScript pawn)
    {
        SelectedPawn = pawn; // możesz też emitować sygnał tutaj jeśli kto inny chce reagować
        //GD.Print($"Selected pawn is {SelectedPawn}");
    }
    public void DeselectPawn() => SelectedPawn = null;
    public override void _Process(double delta)
    {
        if (SetupDone) {
           GameInfoLabelRef.Text = $" Round {Round} | Turn {Turn}"; // zamienić na odwołanie się do tablicy statycznej z nazwą drużyny

            if (Input.IsActionJustPressed("MYSPACE") && PopUpRefScript != null)
            {
                PopUpRefScript.Call("PopUpContentsFunc", "Do you want to end your turn ?");
            } 
        }
    }
    void NextRoundFunc()
    {
        var UnitsBucket = GetNode<Node>("UnitsBucket");

        foreach (Node child in UnitsBucket.GetChildren())
        {
            if (child is PawnBaseFuncsScript pawn)
            {
                pawn.Call("ResetMP"); // przykładowa funkcja w PawnBaseFuncsScript
            }
        }
        Round++;
    }
}
