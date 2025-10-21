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
    [Export] public bool TrueisMelee = false; // typ broni
    bool isWeaponDropped = false; // czy broń została upuszczona
    [Export] public int WeaponRange = 4000; // powinno być 11250
    [Export] public int WeaponDamage = 50; // może w wypadku broni białej może dałoby się mieć "rzut" zamiast strzału ? ale znowu , trzeba byłoby jakoś coś zrobić z tym tamtym mieczem shotgun
    [Export] public int WeaponAmmo = 7;
    [Export] public int MeleeDamage = 75; // TEMP 
    [Export] public int MP = 2; //movement points (how many times can a pawn move in one turn)
    public string TeamId = "";
    [Export] Node2D ColoredPartsNode;
    [Export] Node2D PCNP; // player controller node path
    [Export] Node2D AICNP; // AI controller node path
    [Export] AnimationPlayer UNIAnimPlayerRef;
    [Export] AnimationPlayer SpecificAnimPlayer;
    [Export] public Sprite2D ProfilePick;
    public float PrekalkulowanaObjętośćPionka = 0;
    public int kills; // TEMP
    public Node2D TargetMarkerRef;
    public PawnState State { get; private set; } = PawnState.Standing;
    private bool AC = false;
    RandomNumberGenerator RNGGEN = new RandomNumberGenerator();
    public override void _Ready()
    {
        RNGGEN.Randomize();
        UNIAnimPlayerRef.Play("StandStill");
        if (SpecificAnimPlayer != null)
        {
            SpecificAnimPlayer.AnimationFinished += OnAnimDone;
            SpecificAnimPlayer.Play("None");
        }
        CollisionShape2D KształtPionka = GetNode<CollisionShape2D>("CollisionShape2D");
        var circle = (CircleShape2D)KształtPionka.Shape;
        float radius = circle.Radius;
        if (radius != PrekalkulowanaObjętośćPionka)
        {
            PrekalkulowanaObjętośćPionka = radius;
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
    public void CalculateHit(int dmg,float probability)
    {
        float FloatDice = RNGGEN.RandfRange(0, 10);
        if (FloatDice >= probability) // rzucarz na liczbę powyżej kostki
        {
            GD.Print($"hit on {FloatDice}");
            TakeDamage(dmg);
        }
        else
        {
            GD.Print($"miss on {FloatDice}");
        }
    }
    void TakeDamage(int dmg)
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
            GD.Print($"kontrola Gracza Usunięta z {UnitName} od drużyny {TeamId}");
        }
        else
        {
            AICNP.QueueFree();
            GD.Print($"kontrola AI Usunięta z {UnitName} od drużyny {TeamId}");
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
