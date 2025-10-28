using Godot;
using System;

public partial class HpSamplePawnPaperdollParserScript : Node2D
{
    [Export] Node2D[] Parts;
    void HP_InfoParser(string partName, int PartHP, int PartMaxHP)
    {
        switch (partName)
        {
            case "head":
                if (PartHP > 0)
                {
                    float[] PartHPButfloat = { PartHP, PartMaxHP };
                    Parts[0].Modulate = ColorChanger(PartHPButfloat[0],PartHPButfloat[1]);
                }
                else
                {
                    Parts[0].Modulate = Colors.Black;
                }
                break;
            case "Tors":
                if (PartHP > 0)
                {
                    float[] PartHPButfloat = { PartHP, PartMaxHP };
                    Parts[1].Modulate = ColorChanger(PartHPButfloat[0],PartHPButfloat[1]);
                }
                else
                {
                    Parts[1].Modulate = Colors.Black;
                }
                break;
            case "RArm":
                if (PartHP > 0)
                {
                    float[] PartHPButfloat = { PartHP, PartMaxHP };
                    Parts[2].Modulate = ColorChanger(PartHPButfloat[0],PartHPButfloat[1]);
                }
                else
                {
                    Parts[2].Modulate = Colors.Black;
                }
                break;
            case "LArm":
                if (PartHP > 0)
                {
                    float[] PartHPButfloat = { PartHP, PartMaxHP };
                    Parts[3].Modulate = ColorChanger(PartHPButfloat[0],PartHPButfloat[1]);
                }
                else
                {
                    Parts[3].Modulate = Colors.Black;
                }
                break;
            case "RLeg":
                if (PartHP > 0)
                {
                    float[] PartHPButfloat = { PartHP, PartMaxHP };
                    Parts[4].Modulate = ColorChanger(PartHPButfloat[0],PartHPButfloat[1]);
                }
                else
                {
                    Parts[4].Modulate = Colors.Black;
                }
                break;
            case "LLeg":
                if (PartHP > 0)
                {
                    float[] PartHPButfloat = { PartHP, PartMaxHP };
                    Parts[5].Modulate = ColorChanger(PartHPButfloat[0],PartHPButfloat[1]);
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
    Color ColorChanger(float fHPPart,float fHPMAXPart)
    {
        float HP_Mod = Mathf.Clamp(fHPPart / fHPMAXPart, 0f, 1f);
        Color finalColor;

        if (HP_Mod > 0.5f)
        {
            // Od zielonego (0,255,0) do żółtego (255,255,0)
            float t = (HP_Mod - 0.5f) * 2f; // 0 → 1
            float r = 255f * (1f - t); // zaczyna od 0 przy 1.0 HP, idzie do 255 przy 0.5 HP
            float g = 255f;
            finalColor = new Color(r / 255f, g / 255f, 0f);
        }
        else
        {
            // Od żółtego (255,255,0) do czerwonego (255,0,0)
            float t = HP_Mod * 2f; // 0 → 1
            float r = 255f;
            float g = 255f * t; // maleje z 255 przy 0.5 HP do 0 przy 0 HP
            finalColor = new Color(r / 255f, g / 255f, 0f);
        }
        return finalColor;
    }
}

