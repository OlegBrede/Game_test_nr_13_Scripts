using Godot;
using System;
using System.ComponentModel;

public enum PawnState { Standing, Moving, Dead }

public partial class PawnBaseFuncsScript : CharacterBody2D
{
    [Export] public string UnitName = "Princess"; // TEMP
    [Export] public string UnitType = "Human"; 
    [Export] public string Descriptor = "Lorem\nIpsum\ndolor sit amet";
    [Export] public int PV = 1; // precalculated point value
    [Export] public int HP = 100; //TEMP
    [Export] public float MAD = 3750; // movement allowence distance 
    [Export] public int WeaponRange = 4000; // powinno być 11250
    [Export] public int WeaponDamage = 50; // może w wypadku broni białej może dałoby się mieć "rzut" zamiast strzału ? ale znowu , trzeba byłoby jakoś coś zrobić z tym tamtym mieczem shotgun
    [Export] public int MeleeDamage = 75; // TEMP 
    [Export] public int MP = 2; //movement points (how many times can a pawn move in one turn)
    public string TeamId = "";
    [Export] Node2D ColoredPartsNode;
    [Export] Node2D PCNP; // player controller node path
    [Export] Node2D AICNP; // AI controller node path
    [Export] AnimationPlayer UNIAnimPlayerRef;
    [Export] AnimationPlayer SpecificAnimPlayer;
    [Export] public Sprite2D ProfilePick;
    public Node2D TargetMarkerRef;
    public PawnState State { get; private set; } = PawnState.Standing;
    private bool AC = false;
    public override void _Ready()
    {
        UNIAnimPlayerRef.Play("StandStill");
        if (SpecificAnimPlayer != null)
        {
            SpecificAnimPlayer.AnimationFinished += OnAnimDone;
            SpecificAnimPlayer.Play("None");
        }
    }
    public override void _Process(double delta)
    {
        if (AC == true)
        {
            //MoveAndSlide();
        }
    }
    void Namechange(string Changetothis)
    {
        UnitName = Changetothis;
    }
    void ActivateCollision()
    {
        AC = true;
    }
    public void TakeDamage(int dmg)
    {
        HP -= dmg;
        UNIAnimPlayerRef.Play("Damage");
        GD.Print($"{UnitName} took {dmg} dmg");
        if (HP <= 0)
            Die();
    }
    public void Die()
    {
        State = PawnState.Dead;
        
        if (SpecificAnimPlayer != null)
        {
            SpecificAnimPlayer.Play("Death");
        }
        else
        {
            GD.Print($"NO_DEAD_ANIM_VER {UnitName} is dead.");
            ProcessMode = ProcessModeEnum.Disabled;
            QueueFree();
        }
        
    }
    void OnAnimDone(StringName animName)
    {
        if (animName == "Death")
        {
            GD.Print($"ANIM_VER {UnitName} is dead.");
            ProcessMode = ProcessModeEnum.Disabled;
            QueueFree();
        }
    }
    public void ShowSelection(bool ShowAnim)
    {
        if (ShowAnim == true)
        {
            UNIAnimPlayerRef.Play("SelectionFlash");
        }
        else
        {
            UNIAnimPlayerRef.Stop();
            UNIAnimPlayerRef.Play("StandStill");
        }
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
    public void PlayAttackAnim()
    {
        if (SpecificAnimPlayer != null)
        {
            SpecificAnimPlayer.Play("Attack");
        }
    }
    void DeleteUnusedControlNodes(bool TrueisAI)
    {
        if (TrueisAI == true)
        {
            PCNP.QueueFree();
            GD.Print($"kontrola Gracza Ununięta z {UnitName} od drużyny {TeamId}");
        }
        else
        {
            AICNP.QueueFree();
            GD.Print($"kontrola AI Ununięta z {UnitName} od drużyny {TeamId}");
        }
    }
    public void RSSP() // reset selected status player
    {
        PCNP.Call("ResetSelectedStatus");
    }
    public void PlayerActionPhone(string FuncName,int Parameter)
    {
        PCNP.Call(FuncName,Parameter);
    }
}
