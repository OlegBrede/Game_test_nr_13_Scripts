using Godot;
using System;
using System.Linq;

public partial class HpSamplePawnPaperdollParserScript : Node2D
{
    [Export] Node2D[] Parts;
    [Export] PaperDollPartRes[] paperdollParts;
    void HP_InfoParser(string partName, int partHP, int partMaxHP)
    {
        if(paperdollParts.Count() <= 0)
        {
            GD.Print("Zapomniano dla tego pionka dodać paperdoll");
            return;
        }
        foreach (PaperDollPartRes part in paperdollParts)
        {
            if (partName == part.PartName)
            {
                if (partHP > 0)
                {
                    float[] PartHPButfloat = { partHP, partMaxHP };
                    Parts[part.PartIndex].Modulate = ColorChanger(PartHPButfloat[0],PartHPButfloat[1]);
                }
                else
                {
                    Parts[part.PartIndex].Modulate = Colors.Black;
                }
            }
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

