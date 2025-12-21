using Godot;
using System;

public partial class GUIButtonsToPawnScript : Node2D
{
    GameMNGR_Script gameMNGR_Script;
    Node2D Panel;
    Node2D Paperdollref;
    // ####### OGÓLNE #########
    [Export] Node2D Confirmations;
    [Export] Node2D Actions;
    [Export] Node2D PaperDoll;
    // ######### Specyficzne ############
    [Export] Node2D MoveButton;
    [Export] Node2D ShootButton;
    [Export] Node2D AimShootButton;
    [Export] Node2D MeleeButton;
    [Export] Node2D MeleeWeaponBigStrikeButton;
    [Export] Node2D OverwatchButton;
    [Signal] public delegate void MoveActionEventHandler();
    [Signal] public delegate void NormalShotActionEventHandler();
    [Signal] public delegate void AimedShotActionEventHandler();
    [Signal] public delegate void OverwatchActionEventHandler();
    [Signal] public delegate void WideWallopActionEventHandler();
    [Signal] public delegate void StrongWallopActionEventHandler();
    [Signal] public delegate void PawnConfirmEventHandler(int index);
    [Signal] public delegate void PawnDeclineEventHandler(int index);
    int Parameter = 0; // parameter to magiczny numer uzasadniający co konkretnie ma zaakceptować sygnał (z racji na to że akcje parametru nie biorą to może być punkt wycieku pamięci więc muszę się upewnić)
    public override void _Ready()
    {
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");
        Actions.Visible = false;
        Confirmations.Visible = false;
    }
    public void ReciveWellBeingInfo(PawnBaseFuncsScript PawnScript)
    {
        if (PawnScript.ShootingAllowence <= 0 || PawnScript.WeaponAmmo <= 0)
        {
            GD.Print("pionek nie może strzelać");
            DisableNEnableAction(2,false);
        }
        else
        {
            DisableNEnableAction(2,true);
        }
        if (PawnScript.MeleeAllowence <= 0)
        {
            GD.Print("pionek nie może atakować wręcz");
            DisableNEnableAction(3,false);
        }
        else
        {
            DisableNEnableAction(3,true);
        }
        if (PawnScript.MovinCapability <= 0)
        {
            GD.Print("pionek nie może się ruszać");
            DisableNEnableAction(1,false);
        }
        else
        {
            DisableNEnableAction(1,true);
        }
    } 
    public void PALO(bool VisC,bool VisA) // Player Action LOader
    {
        Confirmations.Visible = VisC;
        Actions.Visible = VisA;
    }
    public void DisableNEnableAction(int Whom, bool what)
    {
        switch (Whom)
        {
            case 1: // disable move 
                if (what == false)
                {
                    MoveButton.Call("OnDisablebutton");
                }
                else
                {
                    MoveButton.Call("OnEnablebutton");
                }
                break;
            case 2: // disable shoot 
                if (what == false)
                {
                    ShootButton.Call("OnDisablebutton");
                    AimShootButton.Call("OnDisablebutton");
                    OverwatchButton.Call("OnDisablebutton");
                }
                else
                {
                    ShootButton.Call("OnEnablebutton");
                    AimShootButton.Call("OnEnablebutton");
                    OverwatchButton.Call("OnEnablebutton");
                }
                break;
            case 3:  // disable melee 
                if (what == false)
                {
                    MeleeButton.Call("OnDisablebutton");
                    MeleeWeaponBigStrikeButton.Call("OnDisablebutton");
                }
                else
                {
                    MeleeButton.Call("OnEnablebutton");
                    MeleeWeaponBigStrikeButton.Call("OnEnablebutton");
                }
                break;
            default:
                GD.Print("Wyłączenie wypada poza wyznaczenie, nic nie zostaje wyłączone");
                break;
        }
    }
    public void HideMUnhideAction(int Whom, bool what)
    {
        switch (Whom)
        {
            case 1: 
                if (what == false)
                {
                    AimShootButton.Visible = false;
                    ShootButton.Visible = false;
                }
                else
                {
                    AimShootButton.Visible = true;
                    ShootButton.Visible = true;
                }
                break;
            case 2: 
                if (what == false)
                {
                    MeleeButton.Visible = false;
                }
                else
                {
                    MeleeButton.Visible = true;
                }
                break;
            case 3:  
                if (what == false)
                {
                    MeleeWeaponBigStrikeButton.Visible = false;
                }
                else
                {
                    MeleeWeaponBigStrikeButton.Visible = true;
                }
                break;
            case 4:  
                if (what == false)
                {
                    OverwatchButton.Visible = false;
                }
                else
                {
                    OverwatchButton.Visible = true;
                }
                break;
            default:
                GD.Print("Wyłączenie wypada poza wyznaczenie, nic nie zostaje wyłączone");
                break;
        }
    }
    public void RecivePaperdoll(string PathtoPaperdoll)
    {
        if (PathtoPaperdoll == null)
        {
            GD.Print("Ten Pionek nie ma paperdoll");
            return;
        }
        PackedScene PaperDollPath = GD.Load<PackedScene>(PathtoPaperdoll);
        Paperdollref = PaperDollPath.Instantiate<Node2D>();
        PaperDoll.AddChild(Paperdollref);
    }
    public void DeletePaperDoll()
    {
        if (Paperdollref != null)
        {
            Paperdollref.QueueFree();
        }
        else
        {
            GD.Print("Nie ma paperdoll do usunięcia");
        }
    }
    public void ReciveWellBeingInfo(string Part, int HP,int MAXHP)
    {
        if (Paperdollref != null)
        {
            Paperdollref.Call("HP_InfoParser",Part,HP,MAXHP);
        }
    }
    void Button_ACT1() // szerszy cone oraz większa szansa na trafienie ale mniejszy DMG
    {
        if (gameMNGR_Script.SelectedPawn != null)
        {
            if (gameMNGR_Script.SelectedPawn.MP > 0)
            {
                EmitSignal(SignalName.WideWallopAction);
                Parameter = 3;
                PALO(false, false);
            }
        }
    }
    void Button_ACT2()
    {
        if (gameMNGR_Script.SelectedPawn != null)
        {
            if (gameMNGR_Script.SelectedPawn.MP > 1) // potrzeba jakoś poinformować gracza że nie może wykonać tego ruchu 
            {
                EmitSignal(SignalName.AimedShotAction);
                Parameter = 2;
                PALO(false, false);
            }
            else
            {
                gameMNGR_Script.PlayerPhoneCallWarning("ACTION NEEDS 2 MP");
            }
        }
    }
    void Button_ACT3()
    {
        if (gameMNGR_Script.SelectedPawn != null)
        {
            if (gameMNGR_Script.SelectedPawn.MP > 0)
            {
                EmitSignal(SignalName.NormalShotAction);
                Parameter = 2;
                PALO(false, false);
            }
        }
    }
    void Button_ACT4()
    {
        if (gameMNGR_Script.SelectedPawn != null)
        {
            if (gameMNGR_Script.SelectedPawn.MP > 0)
            {
                //GD.Print("MoveAction ordered");
                EmitSignal(SignalName.MoveAction);
                Parameter = 1;
                PALO(false, false);
            }
        }
    }
    void Button_ACT7() // Overwatch
    {
        if (gameMNGR_Script.SelectedPawn.MP > 1)
        {
            GD.Print("Kliknięto Przycisk OV");
            EmitSignal(SignalName.OverwatchAction);
            Parameter = 4;
            PALO(false, false);
        }else
        {
            gameMNGR_Script.PlayerPhoneCallWarning("ACTION NEEDS 2 MP");
        }
    }
    void Button_ACT8() // mocniejszy wallop na niższą szansę, mniejszy cone ale większy DMG
    {
        if (gameMNGR_Script.SelectedPawn != null)
        {
            if (gameMNGR_Script.SelectedPawn.MP > 0)
            {
                EmitSignal(SignalName.StrongWallopAction);
                Parameter = 3;
                PALO(false, false);
            }
        }
    }
    void Button_ACT5() // Decline
    {
        if (gameMNGR_Script.SelectedPawn != null)
        {
            if (gameMNGR_Script.SelectedPawn.MP > 0)
            {
                EmitSignal(SignalName.PawnDecline,Parameter);
            }
        }
    }
    void Button_ACT6() // Accept
    {
        if (gameMNGR_Script.SelectedPawn != null)
        {
            if (gameMNGR_Script.SelectedPawn.MP > 0)
            {
                EmitSignal(SignalName.PawnConfirm,Parameter);
            }
        }
    }
}
