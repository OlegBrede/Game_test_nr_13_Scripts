using Godot;
using System;

public partial class UnitMenuSelectionBoxScript : Label
{
    public int ThisUnitsCount = 0;
    public string NameOnLabel = "";
    public string PathToThisPawn = "";
    public int HPOnLabel = 0;
    public int DMGOnLabel = 0;
    public int InternalTeamDesignation;
    Label UnitInfoLabel;
    Label UnitCountLabel;
    [Export]Node2D ViewerAttachmentRootNode;
    TeamFillupBarScript Bitch; // chodzi o gracza który wybiera pionki
    public override void _Ready()
    {
        UnitInfoLabel = GetNode<Label>("UnitInfoLabelNode");
        UnitCountLabel = GetNode<Label>("UnitCountLabelNode");
        UnitInfoLabel.Text = $"Name .: {NameOnLabel}\nHP .: {HPOnLabel}\nDMG .: {DMGOnLabel}\nPoints Value.: ";
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
        Bitch.Call("ReciveUnits", ThisUnitsCount, PathToThisPawn);
        GD.Print($"Drużyna dostała {ThisUnitsCount} pionków typu {PathToThisPawn}");
    }
    void RecivePawnInfo(string Name,Node2D Picture, int HP, int DMG)// do otrzymywania informacji o dokonanych wyborach 
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
        HPOnLabel = HP;
        DMGOnLabel = DMG;
    }
    void ReciveTeamCompInfo(int ILE)
    {
        ThisUnitsCount = ILE;
    }
    public void WhosBitchin(TeamFillupBarScript TheBitch, string PawnPath)
    {
        GD.Print($"{TheBitch} this Bitch callin");
        Bitch = TheBitch;
        PathToThisPawn = PawnPath;
    }
}
