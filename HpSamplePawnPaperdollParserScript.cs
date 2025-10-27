using Godot;
using System;

public partial class HpSamplePawnPaperdollParserScript : Node2D
{
    [Export] Node2D[] Parts;
    void HP_InfoParser(string partName,int PartHP,int PartMaxHP)
    {
        switch(partName){
            case "head":
                if (PartHP > 0)
                {
                    float[] PartHPButfloat = {PartHP,PartMaxHP};
                    float HP_Mod = Mathf.Clamp(PartHPButfloat[0] / PartHPButfloat[1],0,1);
                    float Intencity = Mathf.Clamp(255f * HP_Mod,0,255f);
                    Parts[0].Modulate = new Color(255f - Intencity,Intencity,0);
                }
                else
                {
                    Parts[0].Modulate = Colors.Black;
                }
                break;
                case "Tors":
                if (PartHP > 0)
                {
                    float[] PartHPButfloat = {PartHP,PartMaxHP};
                    float HP_Mod = Mathf.Clamp(PartHPButfloat[0] / PartHPButfloat[1],0,1);
                    float Intencity = Mathf.Clamp(255f * HP_Mod,0,255f);
                    Parts[1].Modulate = new Color(255f - Intencity,Intencity,0);
                }
                else
                {
                    Parts[1].Modulate = Colors.Black;
                }
                break;
                case "RArm":
                if (PartHP > 0)
                {
                    float[] PartHPButfloat = {PartHP,PartMaxHP};
                    float HP_Mod = Mathf.Clamp(PartHPButfloat[0] / PartHPButfloat[1],0,1);
                    float Intencity = Mathf.Clamp(255f * HP_Mod,0,255f);
                    Parts[2].Modulate = new Color(255f - Intencity,Intencity,0);
                }
                else
                {
                    Parts[2].Modulate = Colors.Black;
                }
                break;
                case "LArm":
                if (PartHP > 0)
                {
                    float[] PartHPButfloat = {PartHP,PartMaxHP};
                    float HP_Mod = Mathf.Clamp(PartHPButfloat[0] / PartHPButfloat[1],0,1);
                    float Intencity = Mathf.Clamp(255f * HP_Mod,0,255f);
                    Parts[3].Modulate = new Color(255f - Intencity,Intencity,0);
                }
                else
                {
                    Parts[3].Modulate = Colors.Black;
                }
                break;
                case "RLeg":
                if (PartHP > 0)
                {
                    float[] PartHPButfloat = {PartHP,PartMaxHP};
                    float HP_Mod = Mathf.Clamp(PartHPButfloat[0] / PartHPButfloat[1],0,1);
                    float Intencity = Mathf.Clamp(255f * HP_Mod,0,255f);
                    Parts[4].Modulate = new Color(255f - Intencity,Intencity,0);
                }
                else
                {
                    Parts[4].Modulate = Colors.Black;
                }
                break;
                case "LLeg":
                if (PartHP > 0)
                {
                    float[] PartHPButfloat = {PartHP,PartMaxHP};
                    float HP_Mod = Mathf.Clamp(PartHPButfloat[0] / PartHPButfloat[1],0,1);
                    float Intencity = Mathf.Clamp(255f * HP_Mod,0,255f);
                    Parts[5].Modulate = new Color(255f - Intencity,Intencity,0);
                }
                else
                {
                    Parts[5].Modulate = Colors.Black;
                }
                break;
            default:
                GD.Print("Parser Nie Poznał części");
                break;
        }
    }
}
