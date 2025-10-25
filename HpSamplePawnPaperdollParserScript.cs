using Godot;
using System;

public partial class HpSamplePawnPaperdollParserScript : Node2D
{
    [Export] Node2D[] Parts;
    void HP_InfoParser(string partName,int PartHP)
    {
        switch(partName){
            case "head":
                if (PartHP > 0)
                {
                    Parts[0].Modulate = Colors.Green;
                }
                else
                {
                    Parts[0].Modulate = Colors.Red;
                }
                break;
                case "Tors":
                if (PartHP > 0)
                {
                    Parts[1].Modulate = Colors.Green;
                }
                else
                {
                    Parts[1].Modulate = Colors.Red;
                }
                break;
                case "RArm":
                if (PartHP > 0)
                {
                    Parts[2].Modulate = Colors.Green;
                }
                else
                {
                    Parts[2].Modulate = Colors.Red;
                }
                break;
                case "LArm":
                if (PartHP > 0)
                {
                    Parts[3].Modulate = Colors.Green;
                }
                else
                {
                    Parts[3].Modulate = Colors.Red;
                }
                break;
                case "RLeg":
                if (PartHP > 0)
                {
                    Parts[4].Modulate = Colors.Green;
                }
                else
                {
                    Parts[4].Modulate = Colors.Red;
                }
                break;
                case "LLeg":
                if (PartHP > 0)
                {
                    Parts[5].Modulate = Colors.Green;
                }
                else
                {
                    Parts[5].Modulate = Colors.Red;
                }
                break;
            default:
                GD.Print("Parser Nie Poznał części");
                break;
        }
    }
}
