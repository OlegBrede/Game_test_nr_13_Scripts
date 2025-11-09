using Godot;
using System;

public partial class UnitMenuSelectionBoxScript : Label
{
    public int ThisUnitsCount = 0;
    public string NameOnLabel = "";
    public string PathToThisPawn = "";
    public int DMGOnLabel = 0;
    public int InternalTeamDesignation;
    public int PVOnLabel = 1;
    public string DescriptorOnLabel;
    float UnitRadius = 0;
    Label UnitInfoLabel;
    Label UnitCountLabel;
    [Export]Node2D ViewerAttachmentRootNode;
    TeamFillupBarScript Bitch; // chodzi o gracza który wybiera pionki
    public override void _Ready()
    {
        UnitInfoLabel = GetNode<Label>("UnitInfoLabelNode");
        UnitCountLabel = GetNode<Label>("UnitCountLabelNode");
        UnitInfoLabel.Text = $"Name .: {NameOnLabel}\nDMG .: {DMGOnLabel}\nPoints Value.: {PVOnLabel}\nDescription.: {DescriptorOnLabel}";
    }
    public override void _Process(double delta)
    {
        UnitCountLabel.Text = $"x{ThisUnitsCount}";
    }
    void Button_ACT0()// dodaj pionek
    {
        if (ThisUnitsCount >= 0)
        {
            ThisUnitsCount++;
        }
    }
    void Button_ACT1()// usuń pionek 
    {
        if (ThisUnitsCount > 0)
        {
            ThisUnitsCount--;
        }
    }
    public void ParseTeamCompInfo()// do przesyłania z powrotem info o dokonanych wyborach
    {
        Bitch.Call("ReciveUnits", ThisUnitsCount,UnitRadius, PathToThisPawn);
        GD.Print($"Drużyna dostała {ThisUnitsCount} pionków typu {PathToThisPawn}");
    }
    void RecivePawnInfo(string Name,Sprite2D Picture,int PV, string Description, int DMG)// do otrzymywania informacji o dokonanych wyborach 
    {
        NameOnLabel = Name;
        if (Picture != null)
        {
            ViewerAttachmentRootNode.AddChild(Picture);
        }
        else
        {
            Sprite2D DefPicture = new Sprite2D();
            DefPicture.Texture = (Texture2D)GD.Load("res://Sprites/Base Ludzik.png");
            ViewerAttachmentRootNode.AddChild(DefPicture);
        }
        DMGOnLabel = DMG;
        PVOnLabel = PV;
        DescriptorOnLabel = Description;
    }
    void ReciveTeamCompInfo(int ILE)
    {
        ThisUnitsCount = ILE;
    }
    public void WhosBitchin(TeamFillupBarScript TheBitch,float UR, string PawnPath)
    {
        GD.Print($"{TheBitch} this Bitch callin");
        Bitch = TheBitch;
        UnitRadius = UR;
        PathToThisPawn = PawnPath;
    }
}
