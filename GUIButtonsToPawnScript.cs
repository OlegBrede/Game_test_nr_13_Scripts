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
    int Parameter = 0;
    public override void _Ready()
    {
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");
        Actions.Visible = false;
        Confirmations.Visible = false;
    }
    public void ShowActions()
    {
        Actions.Visible = true;
        Confirmations.Visible = false;
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
                }
                else
                {
                    ShootButton.Call("OnEnablebutton");
                    AimShootButton.Call("OnEnablebutton");
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
                case 4:  // disable overwatch
                if (what == false)
                {
                    OverwatchButton.Call("OnDisablebutton");
                }
                else
                {
                    OverwatchButton.Call("OnEnablebutton");
                }
                break;
                case 5:  // disable overwatch
                if (what == false)
                {
                    AimShootButton.Call("OnDisablebutton");
                }
                else
                {
                    AimShootButton.Call("OnEnablebutton");
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
    void Button_ACT1()
    {
        if (gameMNGR_Script.SelectedPawn != null)
        {
            if (gameMNGR_Script.SelectedPawn.MP > 0)
            {
                gameMNGR_Script.SelectedPawn.Call("PlayerActionPhone", "Player_ACT_Punch", 0);
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
                gameMNGR_Script.SelectedPawn.Call("PlayerActionPhone", "Player_ACT_AimedShot", 0);
                Parameter = 4;
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
                gameMNGR_Script.SelectedPawn.Call("PlayerActionPhone", "Player_ACT_Shoot", 0);
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
                gameMNGR_Script.SelectedPawn.Call("PlayerActionPhone", "Player_ACT_Move", 0);
                Parameter = 1;
                PALO(false, false);
            }
        }
    }
    void Button_ACT7() // Overwatch
    {
        if (gameMNGR_Script.SelectedPawn.MP > 0)
        {
            gameMNGR_Script.SelectedPawn.Call("PlayerActionPhone", "Player_ACT_Move", 0);
            Parameter = 1;
            PALO(false, false);
        }
    }
    void Button_ACT8() //charge
    {
        if (gameMNGR_Script.SelectedPawn != null) // potrzeba jakoś poinformować gracza że nie może wykonać tego ruchu 
        {
            if (gameMNGR_Script.SelectedPawn.MP > 1)
            {
                gameMNGR_Script.SelectedPawn.Call("PlayerActionPhone", "Player_ACT_Move", 0);
                Parameter = 1;
                PALO(false, false);
            }
            else
            {
                gameMNGR_Script.PlayerPhoneCallWarning("ACTION NEEDS 2 MP");
            }
        }
    }
    void Button_ACT5() // Decline
    {
        if (gameMNGR_Script.SelectedPawn != null)
        {
            if (gameMNGR_Script.SelectedPawn.MP > 0)
            {
                gameMNGR_Script.SelectedPawn.Call("PlayerActionPhone", "Player_ACT_Decline", Parameter);
            }
        }
    }
    void Button_ACT6() // Accept
    {
        if (gameMNGR_Script.SelectedPawn != null)
        {
            if (gameMNGR_Script.SelectedPawn.MP > 0)
            {
                gameMNGR_Script.SelectedPawn.Call("PlayerActionPhone", "Player_ACT_Confirm", Parameter); // by tu modyfikować ilość punktów ruchu co do akcji jest głupie, bo nie uwzględnia to potwierdzeń z fizycznego markera, nie rób tego więcej 
            }
        }
    }
}
