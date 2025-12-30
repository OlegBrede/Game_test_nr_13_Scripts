using Godot;
using System;

public partial class UNI_DeathPhantomScript : Node2D
{
    Node2D ColoredParts;
    AnimationPlayer AnimPlayer;
    Timer DespawnTimer;
    AudioStreamPlayer2D DeathSound;
    UNI_AudioStreamPlayer2d UASP;
    public override void _Ready()
    {
        ColoredParts = GetNode<Node2D>("ColoredParts");
        DespawnTimer = GetNode<Timer>("Timer");
        DeathSound = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        UASP = DeathSound as UNI_AudioStreamPlayer2d;
        DespawnTimer.Timeout += Despawn;
        AnimPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        AnimPlayer.Play("Death");
        GD.Print("Death Phantom spawned");
    }
    public void ReciveInitInfo(Color TeamColor,GameMNGR_Script gameMNGR_Script)
    {
        ColoredParts.Modulate = TeamColor;
        UASP.SCS = gameMNGR_Script.SCS;
        UASP.PlaySound(0,true);
    }
    void Despawn()
    {
        QueueFree();
    }
}
