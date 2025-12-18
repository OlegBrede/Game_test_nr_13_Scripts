using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public enum PawnMoveState { Standing, Moving, Fainted, Dead}
public enum PawnStatusEffect // TEMP
{
    None = 0,
    inPain = 1 << 0,
    Imobalized = 1 << 1,
    Bleeding = 1 << 2,
}
public enum ResponseAnimLexicon // zapewnie będzie więej
{
    damage,
    die
}
public partial class PawnBaseFuncsScript : CharacterBody2D
{
    [Export] public string UnitName = "Princess"; // TEMP
    [Export] public string UnitType = "Human";
    [Export] public string Descriptor = "Lorem\nIpsum\ndolor sit amet";
    [Export] public string PathToPaperDoll;
    [Export] public int PV = 1; // precalculated point value
    public int Integrity = 0; //TEMP
    public int BaseIntegrity = 0;
    [Export] public float MAD = 3750; // movement allowence distance
    public float DistanceMovedByThisPawn = 0;
    public int MeleeAllowence = 0;
    public int MeleeWeaponAllowence = 0;
    public int ShootingAllowence = 0;
    public int MovinCapability = 0;
    [Export] bool CanOverwatch = true;
    [Export] bool ZweiHanderShootah = false;
    public bool OverwatchStatus = false;
    [Export]public float Penalty_range = 1.15f;
    [Export]public float Penalty_shooter = 0.64f;
    [Export]public float Penalty_target = 0.42f;
    [Export] public float WeaponRange = 1750; // jest to mierzone w innyhc jednostkach od tych systemowych, może kiedyś zrobię parser
    [Export] public float DistanceZero = 0; // dystans który jest objętością pionka w lokalnych jednostkach raycast'u by wszystko grało spójnie
    [Export] public int WeaponDamage = 85; // może w wypadku broni białej może dałoby się mieć "rzut" zamiast strzału ? ale znowu , trzeba byłoby jakoś coś zrobić z tym tamtym mieczem shotgun
    [Export] public int WeaponAmmo = 7;
    [Export] public int WeaponMaxAmmo = 7;
    [Export] public int Firemode = 1; // informacje o trybach strzału są opisane w UNI_ControlOverPawnScript
    [Export] public int ShotsPerMP = 3; // ile strzałów na jedną akcję (np. przy burst fire lub przy shotgun)
    [Export] public int MeleeDamage = 38;
    [Export] public int MeleeWeaponDamage = 120;
    [Export] public int MP = 2; //movement points (how many times can a pawn move in one turn)
    public string TeamId = "";
    [Export] Node2D ColoredPartsNode;
    [Export] Node2D PCNP; // player controller node path
    [Export] Node2D AICNP; // AI controller node path
    [Export] AnimationPlayer UNIAnimPlayerRef;
    [Export] AnimationPlayer SpecificAnimPlayer;
    [Export] public Sprite2D ProfilePick;
    [Export] string[] CriticalParts; // części ciała pionka bez których nie może on funkcjonować 
    [Export] public PawnPart[] PawnParts { get; set; } // części ciała pionka
    [Export] UNI_AudioStreamPlayer2d ASP;
    public bool Gameplayprimed = false;
    public float PrevDistance = 0;
    public float ObjętośćPionka = 0;
    public int kills = 0; // TEMP
    float FightDistance = 0; // dystans w jakim zadziała się akcja, usprawiedliwiający pokazanie walki w wolnym tępie, jest terz obsługiwana w UNI_ControlOverPawnScript, to dlaego że dystans dalej wpywa na aktywację timera
    public Node2D TargetMarkerRef;
    public PawnMoveState PawnMoveStatus { get; set; } = PawnMoveState.Standing;
    public PawnStatusEffect PawnsActiveStates = PawnStatusEffect.None;
    GameMNGR_Script gameMNGR_Script;
    Timer ResponseAnimTimer;
    Timer DMGLabelVisTimer;
    ResponseAnimLexicon responseAnimLexicon;
    private bool AC = false; // (ACTIVATE COLLISION) TEMP 
    RandomNumberGenerator RNGGEN = new RandomNumberGenerator();
    Node2D DMG_Label_bucket;
    PackedScene ThisLabelScene;
    // 0 = selection sounds
    // 1 = hurt sounds
    int[,] VoicelineRange = {{0,0},{0,0}}; 
    public override void _Ready()
    {
        //GD.Print("PawnReadyTriggered");
        // radius check in 
        CollisionShape2D KształtPionka = GetNode<CollisionShape2D>("CollisionShape2D");
        var circle = (CircleShape2D)KształtPionka.Shape;
        float radius = circle.Radius;
        if (radius != ObjętośćPionka)
        {
            ObjętośćPionka = radius;
        }
        // ustalenie gamemenagera
        
        if (GetTree().CurrentScene.Name != "BaseTestScene") // hack ale powinien naprawić błąd wyskakujący przy wczytywaniu
        {
            GD.Print($"Scena nie jest BaseTestScene, jest {GetTree().CurrentScene.Name}");
            GD.Print("PawnBaseFuncsScript wyłączone ... ");
            return;
        }
        else
        {
            GD.Print("Scena jest BaseTestScene");
            GD.Print("PawnBaseFuncsScript włączone ... ");
            Gameplayprimed = true;
        }
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene"); // to dalej daje error, pomyślę nad rozwiązaniem 
        //GD.Print("skrypt przechodzi dalej ... ");
        RNGGEN.Randomize();
        UNIAnimPlayerRef.Play("StandStill");
        DMGLabelVisTimer = GetNode<Timer>("DMGLabelVisTimer");
        DMGLabelVisTimer.Timeout += DeleteDamageNumbers;
        ResponseAnimTimer = GetNode<Timer>("ResponseAnimTimer");
        DMG_Label_bucket = GetNode<Node2D>("DMG_Label_bucket");
        ResponseAnimTimer.Timeout += AnimAwaitResponse;
        if (SpecificAnimPlayer != null)
        {
            SpecificAnimPlayer.AnimationFinished += OnAnimDone;
            SpecificAnimPlayer.Play("None");
        }
        
        foreach (var Bodypart in PawnParts)
        {
            Bodypart.HP = Bodypart.MAXHP; // ustawienie hp każdej częśći na jej max
            Integrity += Bodypart.HP; // kalkulacja wyświetlanego hp 
            if (Bodypart.MeleeCapability == true)
            {
                MeleeAllowence++;
            }
            if (Bodypart.ShootingCapability == true)
            {
                ShootingAllowence++;
            }
            if (Bodypart.EsentialForMovement == true)
            {
                MovinCapability++;
            }
            if (Bodypart.MeleeWeaponCapability == true)
            {
                MeleeWeaponAllowence++;
            }
        }
        //GD.Print($"ShootingAllowence is {ShootingAllowence}, MeleeAllowence is {MeleeAllowence} MeleeWeaponAllowence is {MeleeWeaponAllowence}");
        BaseIntegrity = Integrity;
    }
    void Namechange(string Changetothis)
    {
        UnitName = Changetothis;
    }
    void SetGendah(int GenderNumber)
    {
        if (GenderNumber == 1)//male
        {
            VoicelineRange[0,0] = 0;
            VoicelineRange[0,1] = 1;
            VoicelineRange[1,0] = 2;
            VoicelineRange[1,1] = 3;
        }else{// female
            VoicelineRange[0,0] = 4;
            VoicelineRange[0,1] = 5;
            VoicelineRange[1,0] = 6;
            VoicelineRange[1,1] = 7;
        }
    }
    void ActivateCollision()
    {
        AC = true;
    }
    public void CalculateHit(int dmg, float probability,int Where, string Bname,float DamageDistance)
    {
        FightDistance = DamageDistance;
        GD.Print($"Engagement Distance is {FightDistance}");
        float FloatDice = RNGGEN.RandfRange(0, 10);
        if (FloatDice >= probability) // rzucarz na liczbę powyżej kostki
        {
            //GD.Print($"hit on {FloatDice}");
            gameMNGR_Script.GenerateActionLog($"{Bname} Hit {UnitName}");
            TakeDamage(dmg,Where);
            ShowHitInfoLabel(dmg);
        }
        else
        {
            //GD.Print($"miss on {FloatDice}");
            gameMNGR_Script.GenerateActionLog($"{Bname} Missed {UnitName}");
            ShowHitInfoLabel(0);
        }
    }
    int LocationRollCalc()
    {
        List<string> LocationHitProbabilitytable = new List<string>();
        foreach (var Part in PawnParts)
        {
            for (int i = 0; i < Part.ChanceToHit; i++) // im więcej razy dany element pojawi się na liście tym łatwiej go wylosować 
            {
                LocationHitProbabilitytable.Add(Part.Name);
                //GD.Print($"dodano do listy {Part.Name}");
            }
        }
        int HitChanceRoll = RNGGEN.RandiRange(0, LocationHitProbabilitytable.Count() - 1);
        var WantedPart = System.Array.Find(PawnParts, p => p.Name == LocationHitProbabilitytable[HitChanceRoll]);
        int indeks = System.Array.IndexOf(PawnParts, WantedPart);
        LocationHitProbabilitytable.Clear(); // na wszelki wypadek
        return indeks;
    }
    void TakeDamage(int dmg,int Where)
    {
        responseAnimLexicon = ResponseAnimLexicon.damage;
        string W_co; // nazwa części ciała która dostaje 
        int PlacementRoll_INDEX; // index części ciała która dostaje 
        if (Where <= PawnParts.Count()) // to działą na zasadzie takiej że lokacja obrażeń danego miejsca jest predefiniowana do czasu jak nie wyjdzie poza zakres, a zakres zewnętrznie ustalony jest na 999 (chyba nie będzie nigdy pionka z 1000 części ciała)
        {
            PlacementRoll_INDEX = Where; //index na wskazaną część ciała
            W_co = PawnParts[Where].Name; // nazwa części ciała
            GD.Print($"Nielosowy traf w {W_co}");
        }
        else
        {
            PlacementRoll_INDEX = LocationRollCalc(); // losowy index
            W_co = PawnParts[PlacementRoll_INDEX].Name; // nazwa części ciała
            GD.Print($"Losowy traf w {W_co}");
        }
        int LeftoverDMG = 0;
        if (PawnParts[PlacementRoll_INDEX].HP > dmg)// jeśli DMG jest mniejszy od HP
        {
            PawnParts[PlacementRoll_INDEX].HP -= dmg; //DMG dociera do części ciała
            gameMNGR_Script.GenerateActionLog($"{UnitName} took {dmg} damage to the {W_co}");
        }
        else // równy DMG lub większy od HP to utrata części i/lub przeniesienie DMG dalej
        {
            PawnParts[PlacementRoll_INDEX].HP -= dmg; //odejmujemy DMG
            LeftoverDMG = PawnParts[PlacementRoll_INDEX].HP * -1; //odwracając ujemny DMG dostajemy ile DMG będzie transferowana w górę
            GD.Print($"Dana część nie wytrzyma DMG, więc reszts obrażeń to ({LeftoverDMG})");
            gameMNGR_Script.GenerateActionLog($"{UnitName} lost {W_co} due to damage");
            foreach (string CriticalPart in CriticalParts)// szukamy czy dana część ciała była krytyczna do funkcjonowania jednostki
            {
                if (CriticalPart == PawnParts[PlacementRoll_INDEX].Name && PawnParts[PlacementRoll_INDEX].HP <= 0)// jak była, i nie ma HP
                {
                    GD.Print($"dany pionek umiera od dostania w {PawnParts[PlacementRoll_INDEX].Name}");
                    responseAnimLexicon = ResponseAnimLexicon.die;
                    if (FightDistance > 1500f)
                    {
                        GD.Print("Anim Resp Timer start trigger...");
                        ResponseAnimTimer.Start();
                    }
                    else
                    {
                        ResponseAnimTimer.Stop(); // nie diała, mimo tego i tak strzelanie powoduje że kamera leci do celu 
                        GD.Print("Dystans zbyt krótki by angarzować spowolnienie kamery");
                        ASP.PlaySound(VoicelineRange[1,RNGGEN.RandiRange(0,1)]);
                        UNIAnimPlayerRef.Play("Damage");
                        Die();
                    }
                    return; // tak na wszelki wypadek by ten kod nie szedł dalej po tym jak już zadecyduje umrzeć 
                }
            }
            if (PawnParts[PlacementRoll_INDEX].ParentPart != null)
            {
                GD.Print("DMG idzie w górę hierarchii");
                TakeDamage(LeftoverDMG,FindPartToDamage(PlacementRoll_INDEX)); // okej, teraz rekurencja nie powinna crash-ować gry
            }
        }
        Integrity = 0; // czyszczenie numery który wyświetla integralność pionka (te tamte procenty)
        foreach (var Bodypart in PawnParts)
        {
            Integrity += Bodypart.HP;
        }
        if (FightDistance > 1500f)
        {
            GD.Print("Anim Resp Timer start trigger...");
            ResponseAnimTimer.Start();
        }
        else
        {
            ResponseAnimTimer.Stop(); // nie diała, mimo tego i tak strzelanie powoduje że kamera leci do celu 
            GD.Print("Dystans zbyt krótki by angarzować spowolnienie kamery");
            ASP.PlaySound(VoicelineRange[1,RNGGEN.RandiRange(0,1)]);
            UNIAnimPlayerRef.Play("Damage");
        }
    }
    int FindPartToDamage(int partIndex)
    {
        GD.Print("Szukanie części ...");
        PawnPart part = PawnParts[partIndex];
        // Znajdź rodzica po nazwie
        var parentPart = System.Array.Find(PawnParts, p => p.Name == PawnParts[partIndex].ParentPart);

        // Jeśli nie ma rodzica — obrażenia zostają tutaj
        if (parentPart == null)
        {
            GD.Print($"{part.Name} nie ma rodzica, dmg zostaje tutaj.");
            return partIndex;
        }

        // Jeśli ma rodzica, znajdź jego indeks
        int parentIndex = System.Array.IndexOf(PawnParts, parentPart);

        // Jeśli rodzic ma HP > 0, to tam idą obrażenia
        if (parentPart.HP > 0)
        {
            GD.Print($"Obrażenia przechodzą z {part.Name} do {parentPart.Name}");
            return parentIndex;
        }
        else
        {
            // Rodzic martwy, idź wyżej w hierarchii
            GD.Print($"{parentPart.Name} ma 0 HP, szukam wyżej...");
            return FindPartToDamage(parentIndex);
        }
    }
    void DecreseFightCapability(bool Melee, bool Shootah, bool Legs)
    {
        GD.Print("Częśćciała zniszczona, efektywność walki zmniejszona");
        if (Melee == true)
        {
            MeleeAllowence--;
        }
        if (Shootah == true)
        {
            if (ZweiHanderShootah == true)
            {
                ShootingAllowence -= 2; //albo lewa albo prawa ręka musi zostać usunięta by broń została dezaktywowana
            }
            else
            {
                ShootingAllowence--;
            }
        }
        if (Legs == true)
        {
            MAD = MAD / MovinCapability; // tu odejmowana jest możliwość szybszego ruchu gdy postać ma mniej nóg
            MovinCapability--;
        }
        CheckFightingCapability();
    }
    public void CheckFightingCapability()
    {
        if (ShootingAllowence <= 0 || WeaponAmmo <= 0)
        {
            GD.Print("pionek nie może strzelać");
            gameMNGR_Script.PlayerPhoneCallbackIntBool("DisableNEnableAction", 2,false); // zapewne będzie trzeba to zastąpić sygnałem jak uprzednio ale .... może jak starczy czasu ?
        }
        else
        {
            gameMNGR_Script.PlayerPhoneCallbackIntBool("DisableNEnableAction", 2,true);
        }
        if (MeleeAllowence <= 0)
        {
            GD.Print("pionek nie może atakować wręcz");
            gameMNGR_Script.PlayerPhoneCallbackIntBool("DisableNEnableAction", 3, false);
        }
        else
        {
            gameMNGR_Script.PlayerPhoneCallbackIntBool("DisableNEnableAction", 3,true);
        }
        if (MovinCapability <= 0)
        {
            GD.Print("pionek nie może się ruszać");
            gameMNGR_Script.PlayerPhoneCallbackIntBool("DisableNEnableAction", 1, false);
        }
        else
        {
            gameMNGR_Script.PlayerPhoneCallbackIntBool("DisableNEnableAction", 1, true);
        }
    }
    void AnimAwaitResponse()
    {
        GD.Print("AnimAwaitResponse triggered ...");
        if (FightDistance > 1500f)
        {
            switch (responseAnimLexicon)
            {
                case ResponseAnimLexicon.damage:
                    if (UNIAnimPlayerRef.IsPlaying())
                    {
                        UNIAnimPlayerRef.Stop();
                    }
                    ASP.PlaySound(VoicelineRange[1,RNGGEN.RandiRange(0,1)]);
                    UNIAnimPlayerRef.Play("Damage");
                break;
                case ResponseAnimLexicon.die:
                    ASP.PlaySound(VoicelineRange[1,RNGGEN.RandiRange(0,1)]);
                    UNIAnimPlayerRef.Play("Damage");
                    Die();
                break;
            }
        }
        FightDistance = 0;
    }
    void ShowHitInfoLabel(int dmg)
    {
        ThisLabelScene = GD.Load<PackedScene>("res://Prefabs/dmg_num_popup.tscn");
        Label dmgLabel = ThisLabelScene.Instantiate<Label>();
        Control dmgLabelPosControl = new Control();
        DMG_Label_bucket.AddChild(dmgLabelPosControl);
        dmgLabelPosControl.AddChild(dmgLabel);
        // --- KOLOR + OUTLINE ---
        if (dmg == 0)
        {
            dmgLabel.Call("ShowFadeWarning","Miss");
            dmgLabel.AddThemeColorOverride("font_color", Colors.White);
            dmgLabel.AddThemeColorOverride("font_outline_color", Colors.Gray);
        }
        else
        {
            dmgLabel.Call("ShowFadeWarning",$"-{dmg}");
            dmgLabel.AddThemeColorOverride("font_color", Colors.Red);
            dmgLabel.AddThemeColorOverride("font_outline_color",new Color(0.4f, 0f, 0f));// bordowy
        }
        dmgLabel.AddThemeConstantOverride("outline_size", 22);
        // --- LOSOWA POZYCJA NAD PIONKIEM ---
        float RandomPosNum = 300f;
        float randomX = (float)GD.RandRange(-RandomPosNum - 100, RandomPosNum); // ta randomizacja nie raandoizuje, proszę poprawić statycznym stołem ala doom jak nie pyknie 
        float randomY = (float)GD.RandRange(-RandomPosNum - 100, RandomPosNum);
        GD.Print($"random X i Y to {randomX},{randomY}");
        //randomY = Mathf.Max(randomY, 380); // limit Y
        dmgLabelPosControl.Position = new Vector2(randomX, randomY);
        DMGLabelVisTimer.Start();
    }
    void DeleteDamageNumbers()
    {
        foreach (Control Numbers in DMG_Label_bucket.GetChildren())
        {
            Numbers.QueueFree();
            GD.Print("Damage number deleted");
        }
    }
    public void Die()
    {
        PawnMoveStatus = PawnMoveState.Dead;
        PawnsActiveStates = PawnStatusEffect.None;
        if (gameMNGR_Script.Turn == TeamId)
        {
            gameMNGR_Script.TeamsCollectiveMP -= MP; // odjęte punkty MP od ogólnej puli
        }
        gameMNGR_Script.GenerateActionLog($"{UnitName} is dead.");
        if (SpecificAnimPlayer != null)
        {
            SpecificAnimPlayer.Play("Death");
        }
        else
        {
            //GD.Print($"NO_DEAD_ANIM_VER {UnitName} is dead.");
            ProcessMode = ProcessModeEnum.Disabled;
            QueueFree();
        }
    }
    void OnAnimDone(StringName animName)
    {
        if (animName == "Death")
        {
            //GD.Print($"ANIM_VER {UnitName} is dead.");
            ProcessMode = ProcessModeEnum.Disabled;
            QueueFree();
        }
    }
    public void ShowSelection(bool ShowAnim)
    {
        if (ShowAnim == true)
        {
            UNIAnimPlayerRef.Play("SelectionFlash");
        }
        else
        {
            UNIAnimPlayerRef.Stop();
            UNIAnimPlayerRef.Play("StandStill");
        }
    }
    public void SetTeam(string teamId, Color TeamColors)
    {
        TeamId = teamId;
        ColoredPartsNode.Modulate = TeamColors;
    }
    void ResetMP()
    {
        MP = 2;
    }
    void ResetMoveStatus()
    {
        if (PawnMoveStatus == PawnMoveState.Moving && PawnMoveStatus != PawnMoveState.Fainted)
        {
            PawnMoveStatus = PawnMoveState.Standing;
            DistanceMovedByThisPawn = 0;
            PrevDistance = 0;
        }
    }
    void ReplenishAmmo(int ByHowMuch)
    {
        GD.Print("Amunicja uzupełniona");
        WeaponAmmo += ByHowMuch;
        if (WeaponAmmo > WeaponMaxAmmo)
        {
            WeaponAmmo = WeaponMaxAmmo;
        }
    }
    public void PlayAttackAnim(bool trueisRange)
    {
        if (SpecificAnimPlayer != null)
        {
            if (trueisRange == true)
            {
                SpecificAnimPlayer.Play("Attack"); // trza zmienić nazwę na "Range attack" i "melee Attack"
            }
            else
            {
                SpecificAnimPlayer.Play("Attack");
            }
        }
    }
    void DeleteUnusedControlNodes(bool TrueisAI)
    {
        if (TrueisAI == true)
        {
            PCNP.QueueFree();
            //GD.Print($"kontrola Gracza Usunięta z {UnitName} od drużyny {TeamId}");
        }
        else
        {
            AICNP.QueueFree();
            //GD.Print($"kontrola AI Usunięta z {UnitName} od drużyny {TeamId}");
        }
    }
    public void RSSP() // reset selected status player
    {
        if(PCNP == null)
        {
            GD.Print($"pionek nie ma kontrolera gracza ");
            return;
        }  
        PCNP.Call("ResetSelectedStatus");
        PawnPlayerController PPC = PCNP as PawnPlayerController;
        PPC.UnsubscribeFromUIControlls();
    }
    public void SetUISubscription()
    {
        PawnPlayerController PPC = PCNP as PawnPlayerController;
        PPC.SubscribeToUIControlls();
    }
    public void PlayerActionPhone(string FuncName,int Parameter) // TO DO .: - to jest "chyba" do wywalenia z racji na zastąpienie tego sygnałami
    {
        PCNP.Call(FuncName,Parameter);
    }
}
