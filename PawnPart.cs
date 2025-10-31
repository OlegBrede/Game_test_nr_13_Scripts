using Godot;
using System;

[GlobalClass]
public partial class PawnPart : Resource
{
    [Export] public string Name { get; set; } = "Unnamed";
    [Export] public int MAXHP { get; set; } = 100; // maksymalna ilość hp dla tej części ciała
    [Export] public int HP { get; set; } = 100; // integralność danej części ciała
    [Export] public int ChanceToHit { get; set; } = 3; //ile razy zostaje zapisana część na liście do losowania trafienia, większa liczba to większe prawdopodobieństwo
    [Export] public string ParentPart { get; set; } = null; //jeśli trafienie jest w tę część ciała, ale ma ona 0 hp, to wtedy dmg przejdzie na tę część
    [Export] public bool MeleeCapability { get; set; } = false; // to ustala czy pionek dalej może walczyć wręcz
    [Export] public bool MeleeWeaponCapability { get; set; } = false; // to ustala czy pionek może dalej walczyć swoją bronią białą
    [Export] public bool ShootingCapability { get; set; } = false; // to ustala czy pionek dalej może strzelać
    [Export] public bool EsentialForMovement { get; set; } = false; //Dezygnacja dla czegokolwiek co pcha Ludzika do przodu, bez nóg niedzie nie zajdziesz 
}
