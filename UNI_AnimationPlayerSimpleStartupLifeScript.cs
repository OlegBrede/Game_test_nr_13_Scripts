using Godot;
using System;

public partial class UNI_AnimationPlayerSimpleStartupLifeScript : AnimationPlayer
{
    [Export] string animname;
    public override void _Ready()
    {
        Play(animname);
    }
}
