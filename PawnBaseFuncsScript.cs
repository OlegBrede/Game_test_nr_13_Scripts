using Godot;
using System;

public enum PawnState { Standing, Moving, Dead }

public partial class PawnBaseFuncsScript : CharacterBody2D
{
    [Export] public string UnitName = "Princess"; // TEMP
    [Export] public string UnitType = "Human"; // TEMP
    [Export] public int HP = 100; //TEMP
    [Export] public int MA = 3750; // movement allowance for walk distance
    [Export] public int MP = 2; //movement points (how many times can a pawn move in one turn)
    [Export] public string Weapon = "Sword"; //TEMP
    [Export] public string TeamId = "Team1";
    public Node2D TargetMarkerRef;
    public PawnState State { get; private set; } = PawnState.Standing;

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

    public void SetTeam(string teamId)
    {
        TeamId = teamId;
    }
    void ResetMP()
    {
        MP = 2;
    }
}
