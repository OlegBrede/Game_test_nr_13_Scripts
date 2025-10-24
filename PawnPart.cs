using Godot;
using System;

[GlobalClass]
public partial class PawnPart : Resource
{
    [Export] public string Name { get; set; } = "Unnamed";
    [Export] public int HP { get; set; } = 100;
    [Export] public int ChanceToHit { get; set; } = 3;
    [Export] public string ParentPart { get; set; } = null;
}
