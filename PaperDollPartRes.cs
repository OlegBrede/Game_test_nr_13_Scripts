using Godot;
using System;

[GlobalClass]
public partial class PaperDollPartRes : Resource
{
    [Export] public string PartName { get; set; } = "";
    [Export] public int PartIndex { get; set; } = 0; // index w sęsie że index dla zbiou Node2D[] Parts; by było wiadomo do kórego należy co
}
