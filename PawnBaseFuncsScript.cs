using Godot;
using System;

public enum PawnState { Standing, Moving, Dead }

public partial class PawnBaseFuncsScript : CharacterBody2D
{
    [Export] public string UnitName = "Princess"; // TEMP
    [Export] public string UnitType = "Human"; // TEMP
    [Export] public int HP = 100; //TEMP
    [Export] public int MA = 3750; // movement allowance for walk distance
    [Export] public int WeaponRange = 4000; // powinno być 11250
    [Export] public int WeaponDamage = 50;
    [Export] public int MP = 2; //movement points (how many times can a pawn move in one turn)
    [Export] public string Weapon = ""; //TEMP
    public string TeamId = "Team1";
    [Export] Node2D ColoredPartsNode;
    public Node2D TargetMarkerRef;
    public PawnState State { get; private set; } = PawnState.Standing;
    private bool AC = false;
    public override void _Process(double delta)
    {
        if (AC == true) {
            //MoveAndSlide();
        }
    }
    void ActivateCollision()
    {
        AC = true;
    }
    public void TakeDamage(int dmg)
    {
        HP -= dmg;
        if (HP <= 0)
            Die();
    }

    public void Die()
    {
        State = PawnState.Dead;
        ProcessMode = ProcessModeEnum.Disabled;
        GD.Print($"{UnitName} is dead.");
        QueueFree();
    }

    public void SetTeam(string teamId, Color TeamColors)
    {
        TeamId = teamId;
        ColoredPartsNode.Modulate = TeamColors;
    }
    void ResetMP()
    {
        MP = 2;
    }
}
