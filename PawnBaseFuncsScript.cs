using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public enum PawnMoveState { Stationary, Moving, Fainted, Dead}
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
    [Export] public float Shock10Val = 8; // Szok jakiego może doznać jednostka, wyższe wartości dają większą odporność na szok 
    public bool FaintRecoveryBool = false;
    [Export] public float MAD = 3750; // movement allowence distance
    public float DistanceMovedByThisPawn = 0;
    public int MeleeAllowence = 0;
    public int MeleeWeaponAllowence = 0;
    public int ShootingAllowence = 0;
    public int MovinCapability = 0;
    public bool OVStatus = false;
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
    [Export] Node2D SpriteBucket;
    [Export] public Node2D ColoredPartsNode;
    [Export] Node2D PCNP; // player controller node path
    [Export] public Node2D AICNP; // AI controller node path
    [Export] AnimationPlayer UNIAnimPlayerRef;
    [Export] public AnimationPlayer SpecificAnimPlayer;
    [Export] public Sprite2D ProfilePick;
    [Export] public PawnPart[] PawnParts { get; set; } // części ciała pionka
    [Export] UNI_AudioStreamPlayer2d ASP;
    // ##################### BLEEDING #####################
    public int EntryWounds = 0;
    List<PawnPart> YourPartsToBleed = new List<PawnPart>();
    // ##################### BLEEDING #####################
    public bool Gameplayprimed = false;
    public float PrevDistance = 0;
    public float ObjętośćPionka = 0;
    public int kills = 0; // TEMP
    float FightDistance = 0; // dystans w jakim zadziała się akcja, usprawiedliwiający pokazanie walki w wolnym tępie, jest terz obsługiwana w UNI_ControlOverPawnScript, to dlaego że dystans dalej wpywa na aktywację timera
    public Node2D TargetMarkerRef;
    public PawnMoveState PawnMoveStatus { get; set; } = PawnMoveState.Stationary;
    GameMNGR_Script gameMNGR_Script;
    Timer ResponseAnimTimer;
    Timer DMGLabelVisTimer;
    ResponseAnimLexicon responseAnimLexicon;
    RandomNumberGenerator RNGGEN = new RandomNumberGenerator();
    Node2D DMG_Label_bucket;
    PackedScene ThisLabelScene;
    PackedScene ThisDeathPhantomScene;
    [Export] public UNI_ControlOverPawnScript Ref_UNI_ControlOverPawnScript;
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
        ASP.SCS = gameMNGR_Script.SCS;
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
        GD.Print($"ShootingAllowence is {ShootingAllowence}, MeleeAllowence is {MeleeAllowence} MeleeWeaponAllowence is {MeleeWeaponAllowence}");
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
        }else{                 // female
            VoicelineRange[0,0] = 4;
            VoicelineRange[0,1] = 5;
            VoicelineRange[1,0] = 6;
            VoicelineRange[1,1] = 7;
        }
    }
    public void OnSellectSay()
    {
        ASP.PlaySound(VoicelineRange[0,RNGGEN.RandiRange(0,1)],true);
    }
    public void CalculateHit(int dmg, float probability,int Where,int Wounds, PawnBaseFuncsScript TheCuntThatShootingYou,float DamageDistance)
    {
        if (PawnMoveStatus == PawnMoveState.Dead)
        {
            GD.Print("próba kalkulowania uderzenia gdy pionek jest w trakcie umierania");
            return;
        }
        FightDistance = DamageDistance;
        //GD.Print($"Engagement Distance is {FightDistance}"); // urzywane do kalkulacj kamery trackującej dostanie dmg 
        float FloatDice = RNGGEN.RandfRange(0, 10);
        if (FloatDice >= probability) // rzucarz na liczbę powyżej kostki
        {
            GD.Print($"[{UnitName}] was hit on {FloatDice}");
            gameMNGR_Script.GenerateActionLog($"[color={TheCuntThatShootingYou.ColoredPartsNode.Modulate.ToHtml()}]{TheCuntThatShootingYou.UnitName}[/color] Hit [color={ColoredPartsNode.Modulate.ToHtml()}]{UnitName}[/color]");
            TakeDamage(dmg,Where,false,Wounds);
            ShowHitInfoLabel(dmg);
        }
        else
        {
            GD.Print($"[{UnitName}] was missed on {FloatDice}");
            gameMNGR_Script.GenerateActionLog($"[color={TheCuntThatShootingYou.ColoredPartsNode.Modulate.ToHtml()}]{TheCuntThatShootingYou.UnitName}[/color] Missed [color={ColoredPartsNode.Modulate.ToHtml()}]{UnitName}");
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
        var WantedPart = System.Array.Find(PawnParts, p => p.Name == LocationHitProbabilitytable[HitChanceRoll]); // nie jest wyklucczone że w tym tu krypcie są dwie różne metody na znalezienie danej części w pawn parts, brawo ty, jebany debil
        int indeks = System.Array.IndexOf(PawnParts, WantedPart);
        LocationHitProbabilitytable.Clear(); // na wszelki wypadek
        return indeks;
    }
    void TakeDamage(int dmg,int Where,bool Bleeding,int Wounds)
    {
        GD.Print($"Take damage aktywowany dla {UnitName} z parametrami dmg {dmg}, where {Where}, bleedin {Bleeding}, wounds {Wounds}");
        if (PawnMoveStatus == PawnMoveState.Dead)
        {
            GD.Print("próba TakeDMG gdy pionek jest w trakcie umierania");
            return;
        }
        if (OVStatus == true)// reset overwatch due to damage 
        {
            Ref_UNI_ControlOverPawnScript.ResetOverwatch();
        }
        responseAnimLexicon = ResponseAnimLexicon.damage;
        bool bleedin = Bleeding;
        string W_co; // nazwa części ciała która dostaje 
        int PlacementRoll_INDEX; // index części ciała która dostaje 
        if (Where <= PawnParts.Count()) // to działą na zasadzie takiej że lokacja obrażeń danego miejsca jest predefiniowana do czasu jak nie wyjdzie poza zakres, a zakres zewnętrznie ustalony jest na 999 (chyba nie będzie nigdy pionka z 1000 części ciała)
        {
            PlacementRoll_INDEX = Where; //index na wskazaną część ciała
            W_co = PawnParts[Where].Name; // nazwa części ciała
            GD.Print($"Nielosowy traf w {W_co} za {dmg} dmg");
        }
        else
        {
            PlacementRoll_INDEX = LocationRollCalc(); // losowy index
            W_co = PawnParts[PlacementRoll_INDEX].Name; // nazwa części ciała
            GD.Print($"Losowy traf w {W_co} za {dmg} dmg");
        }
        if (PawnParts[PlacementRoll_INDEX].CausesBleedin == true) // sprawdzane czy od dostania w tą część ciała pionek będzie krwawił
        {
            EntryWounds += Wounds;
            if (Bleeding == false)
            {
                RollForFaint(Shock10Val); // orginalnie miało to działać tak że shock to miała być wartość danej cześci ciała ale jednym słowem, jebać, nie chce mi sie 
                Shock10Val -= (float)EntryWounds/7 * Mathf.Sqrt((float)EntryWounds); // to daje nam płynne przeniesienie mocy uderzeia, im bardziej pionek dostaje tym słabszy jest, patrz (x * pierwiastek z x) na desmos
                GD.Print($"Shock10Val dla faint to teraz {Shock10Val} EntryWounds = {EntryWounds}");
            }
        }
        GD.Print($" traf w {W_co} sprawi krwawiene jest {PawnParts[PlacementRoll_INDEX].CausesBleedin} więc wounds jest {EntryWounds}");

        int LeftoverDMG = 0;
        if (PawnParts[PlacementRoll_INDEX].HP > dmg)// jeśli DMG jest mniejszy od HP
        {
            PawnParts[PlacementRoll_INDEX].HP -= dmg; //DMG dociera do części ciała
            if (PawnParts[PlacementRoll_INDEX].Vitality > 0 && Bleeding == false)
            {
                RNGGEN.Randomize();
                float Vitalitycheck = RNGGEN.RandfRange(0,10);
                GD.Print($"rzut na save {Vitalitycheck} musi przebić vitality równe {PawnParts[PlacementRoll_INDEX].Vitality}");
                if (PawnParts[PlacementRoll_INDEX].Vitality > Vitalitycheck)
                {
                    gameMNGR_Script.GenerateActionLog($"[color={ColoredPartsNode.Modulate.ToHtml()}]{UnitName}[/color] CRITICAL HIT !");
                    ASP.PlaySound(VoicelineRange[1,RNGGEN.RandiRange(0,1)],true);
                    UNIAnimPlayerRef.Play("Damage");
                    Die();
                }
            }
            if (bleedin == false)
            {
                gameMNGR_Script.GenerateActionLog($"[color={ColoredPartsNode.Modulate.ToHtml()}]{UnitName}[/color] took {dmg} damage to the {W_co}");
            }
        }
        else // równy DMG lub większy od HP to utrata części i/lub przeniesienie DMG dalej
        {
            PawnParts[PlacementRoll_INDEX].HP -= dmg; //odejmujemy DMG
            LeftoverDMG = PawnParts[PlacementRoll_INDEX].HP * -1; //odwracając ujemny DMG dostajemy ile DMG będzie transferowana w górę
            PawnParts[PlacementRoll_INDEX].HP = 0;
            GD.Print($"Dana część nie wytrzyma DMG, więc reszts obrażeń to ({LeftoverDMG})");
            if (bleedin == false)
            {
                gameMNGR_Script.GenerateActionLog($"[color={ColoredPartsNode.Modulate.ToHtml()}]{UnitName}[/color] lost {W_co} due to damage");
            }
            else
            {
                gameMNGR_Script.GenerateActionLog($"[color={ColoredPartsNode.Modulate.ToHtml()}]{UnitName}[/color] lost {W_co} due to bleeding");
            }
            DecreseFightCapability(PawnParts[PlacementRoll_INDEX].MeleeCapability,
            PawnParts[PlacementRoll_INDEX].MeleeWeaponCapability,
            PawnParts[PlacementRoll_INDEX].ShootingCapability,
            PawnParts[PlacementRoll_INDEX].EsentialForMovement);
            if (PawnParts[PlacementRoll_INDEX].Vitality > 0f && PawnParts[PlacementRoll_INDEX].HP <= 0)// jak była, i nie ma HP
            {
                GD.Print($"dany pionek umiera od dostania w {PawnParts[PlacementRoll_INDEX].Name}");
                responseAnimLexicon = ResponseAnimLexicon.die;
                PawnMoveStatus = PawnMoveState.Dead;
                if (OVStatus == true)
                {
                    FightDistance = 50; // to powinno naprawić bug gdzie pionek nie umiera podczas dostania OV na ryj
                }
                if (FightDistance > 1500f)
                {
                    GD.Print("Śmierć będzie z animowaną kamerą");
                    ResponseAnimTimer.Start();
                }
                else
                {
                    ResponseAnimTimer.Stop(); // nie diała, mimo tego i tak strzelanie powoduje że kamera leci do celu 
                    GD.Print("Dystans umierania zbyt krótki by angarzować spowolnienie kamery");
                    ASP.PlaySound(VoicelineRange[1,RNGGEN.RandiRange(0,1)],true);
                    UNIAnimPlayerRef.Play("Damage");
                    Die();
                }
                return; // tak na wszelki wypadek by ten kod nie szedł dalej po tym jak już zadecyduje umrzeć 
            }
            
            // Trzeba jeszcze spwadzić czy jakieś cześci bły przymocowane do tej co teraz się zniszczyła, więc jeśli ręka miała dłoń to zniszczenie ręki powoduje odpadnięcie dłoni 
            int index = -1; // ustawiamy index na -1 by wyznaczyć nieznalezioną część 
            for (int i = 0; i < PawnParts.Count(); i++) // iteracja po pawnparts znowu
            {
                if (PawnParts[i].ParentPart == PawnParts[PlacementRoll_INDEX].Name)// jeśli dana część ciała ma tego rodzica co teraz został zniszczony
                {
                    index = i; // ustawiamy indeks tej części ciała na taki by go TakeDamage() mogło znaeźć 
                    GD.Print($"{PawnParts[i].Name} miał rodzica {PawnParts[i].ParentPart} więc pionek też to traci");
                    TakeDamage(PawnParts[i].HP,index,bleedin,Wounds);
                    break;
                }
            }
            if (index == -1)
            {
                GD.Print("Nie było części do zabrania wraz z tym urazem");
            }
            if (PawnParts[PlacementRoll_INDEX].ParentPart != null)
            {
                GD.Print("DMG idzie w górę hierarchii");
                TakeDamage(LeftoverDMG,FindPartToDamage(PlacementRoll_INDEX),bleedin,Wounds); // okej, teraz rekurencja nie powinna crash-ować gry
            }
        }
        Integrity = 0; // czyszczenie numery który wyświetla integralność pionka (te tamte procenty)
        GD.Print("======================== HP =======================");
        foreach (var Bodypart in PawnParts)
        {
            Integrity += Bodypart.HP;
            GD.Print($"{Bodypart.Name} ({Bodypart.HP}/{Bodypart.MAXHP})");
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
            ASP.PlaySound(VoicelineRange[1,RNGGEN.RandiRange(0,1)],true);
            UNIAnimPlayerRef.Play("Damage");
        }
        gameMNGR_Script.PlayerGUIRef.SelectionUpdater();
    }
    public void FuncBleed()
    {
        YourPartsToBleed.Clear();
        foreach (PawnPart Part in PawnParts)
        {
            if (Part.HP > 0)
            {
                YourPartsToBleed.Add(Part);
            }
        }
        // tu kod dodający części ciała do listy 
        int BleedDMG = 0;
        foreach (PawnPart Part in YourPartsToBleed)
        {
            if (Part.CausesBleedin == true)
            {
                TakeDamage(EntryWounds,YourPartsToBleed.IndexOf(Part),true,0);
                BleedDMG += EntryWounds;
            }
        }
        gameMNGR_Script.GenerateActionLog($"[color={ColoredPartsNode.Modulate.ToHtml()}]{UnitName}[/color] bleeds {BleedDMG} damage"); // to powinno być później bo teraz pokazuje zły nr damage 
    }
    public void BandageWounds()
    {
        EntryWounds = 0;
        gameMNGR_Script.GenerateActionLog($"[color={ColoredPartsNode.Modulate.ToHtml()}]{UnitName}[/color] stops the bleeding with a bandage");
    }
    int FindPartToDamage(int partIndex) // funkcja do znajdywania rodzica danej części ciała
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
    void DecreseFightCapability(bool Melee, bool MeleeWeapon, bool Shootah, bool Legs)// nie ma tu melee weapon
    {
        GD.Print("[DecreseFightCapability] Częśćciała zniszczona, efektywność walki zmniejszona");
        if (Melee == true)
        {
            MeleeAllowence--;
        }
        if (MeleeWeapon == true)
        {
            MeleeWeaponAllowence--;
        }
        if (Shootah == true)
        {
            ShootingAllowence--;
        }
        if (Legs == true)
        {
            MAD = MAD / MovinCapability; // tu odejmowana jest możliwość szybszego ruchu gdy postać ma mniej nóg
            MovinCapability--;
        }
        GD.Print($"[DecreseFightCapability] MeleeAllowence {MeleeAllowence}, MeleeWeaponAllowence {MeleeWeaponAllowence}, ShootingAllowence {ShootingAllowence}, MovinCapability {MovinCapability} ");
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
                    ASP.PlaySound(VoicelineRange[1,RNGGEN.RandiRange(0,1)],true);
                    UNIAnimPlayerRef.Play("Damage");
                break;
                case ResponseAnimLexicon.die:
                    ASP.PlaySound(VoicelineRange[1,RNGGEN.RandiRange(0,1)],true);
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
        //GD.Print($"random X i Y to {randomX},{randomY}");
        //randomY = Mathf.Max(randomY, 380); // limit Y
        dmgLabelPosControl.Position = new Vector2(randomX, randomY);
        DMGLabelVisTimer.Start();
    }
    void DeleteDamageNumbers()
    {
        foreach (Control Numbers in DMG_Label_bucket.GetChildren())
        {
            Numbers.QueueFree();
            //GD.Print("Damage number deleted");
        }
    }
    public void DeductMP(int ByHowMutch)
    {
        if (ByHowMutch <= 0)
        {
            return;
        }
        MP -= ByHowMutch;
        gameMNGR_Script.TeamsCollectiveMP -= ByHowMutch;
        GD.Print($"Doszło do dedukcji MP, teraz jest {MP}");
        GD.Print("Sygnał emitowany");
        gameMNGR_Script.RefreshTableSituationStatus(this); // to wysyła do gamemngr info o tym że dany pionek zakończył ruch i trzeba np sprawdzić czy jest on w zasięgu OV lub czy nie stoi na apteczce
        gameMNGR_Script.PlayerGUIRef.SelectionUpdater();
    } 
    public void CheckOV_LOS(CharacterBody2D Enimy)
    {
        Ref_UNI_ControlOverPawnScript.OverwatchOnEnemyMPChanged(Enimy);
    }
    public void Die()
    {
        GD.Print($"{UnitName} teraz wykonuje funkcję die");
        if (PCNP != null)
        {
            PawnPlayerController PCNPS = PCNP as PawnPlayerController;
            PCNPS.UnsubscribeFromUIControlls();
        }
        if (SpecificAnimPlayer.IsPlaying())
        {
            SpecificAnimPlayer.Stop();
        }
        if (gameMNGR_Script.Turn == TeamId)
        {
            gameMNGR_Script.TeamsCollectiveMP -= MP; // odjęte punkty MP od ogólnej puli oraz deselekcjonuje pionka jak jest jego tura przy jego śmierci
            gameMNGR_Script.DeselectPawn();
        }
        gameMNGR_Script.GenerateActionLog($"[color={ColoredPartsNode.Modulate.ToHtml()}]{UnitName}[/color] is dead.");
        ThisDeathPhantomScene = GD.Load<PackedScene>("res://Prefabs/DeathPhantomFolder/death_phantom_1.tscn"); // TO DO, WYMAGANE USTAWIENIE ZEWNĘTRZNE Z RACJI NA RÓŻNE TYPY ANIMACJI ŚMIERCI
        Node2D DeathPhantom = ThisDeathPhantomScene.Instantiate<Node2D>();
        gameMNGR_Script.bucketForTheDead.AddChild(DeathPhantom);
        DeathPhantom.GlobalPosition = GlobalPosition;
        DeathPhantom.Call("ReciveInitInfo",ColoredPartsNode.Modulate,gameMNGR_Script);
        ProcessMode = ProcessModeEnum.Disabled;
        QueueFree();
    }
    void OnAnimDone(StringName animName)
    {
        if ((animName == "Range_Attack" || animName == "Melee_Attack") && PawnMoveStatus == PawnMoveState.Moving)
        {
            SpecificAnimPlayer.Play("Walk");
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
            UNIAnimPlayerRef.Play("StandStill");
        }
    }
    public void SetTeam(string teamId, Color TeamColors)
    {
        TeamId = teamId;
        ColoredPartsNode.Modulate = TeamColors;
    }
    public void ResetMP()
    {
        if (FaintRecoveryBool == true)
        {
            GD.Print("Faint recovery succesfull");
            PawnMoveStatus = PawnMoveState.Stationary;
            gameMNGR_Script.GenerateActionLog($"[color={ColoredPartsNode.Modulate.ToHtml()}]{UnitName}[/color] recovered.");
            SpriteBucket.Rotation = 0f;
            FaintRecoveryBool = false;
        }
        if (PawnMoveStatus != PawnMoveState.Fainted)
        {
            MP = 2;
        }
        else
        {
            MP = 0;
            GD.Print($"No MP reset Due to Faint MP at {MP}");
        }
    }
    public void ApllyStatusEffects()
    {
        
    }
    void RollForFaint(float ShockVal)
    {
        float FaintRange = RNGGEN.RandfRange(0,10);
        GD.Print($"ROLL NA FAINT {FaintRange} musi przebić {ShockVal} co dało wynik {ShockVal < FaintRange}");
        if (ShockVal < FaintRange)
        {
            ResetMoveStatus();
            SpriteBucket.Rotation = Mathf.DegToRad(90f);
            MP = 0;
            if (PawnMoveStatus != PawnMoveState.Fainted)
            {
                gameMNGR_Script.GenerateActionLog($"[color={ColoredPartsNode.Modulate.ToHtml()}]{UnitName}[/color] fainted.");
                PawnMoveStatus = PawnMoveState.Fainted;
            }
            else
            {
                gameMNGR_Script.GenerateActionLog($"[color={ColoredPartsNode.Modulate.ToHtml()}]{UnitName}[/color] fainted again.");
                GD.Print("Dystans umierania zbyt krótki by angarzować spowolnienie kamery");
                ASP.PlaySound(VoicelineRange[1,RNGGEN.RandiRange(0,1)],true);
                UNIAnimPlayerRef.Play("Damage");
                Die();
            }
        }
    }
    void ResetMoveStatus()
    {
        if (PawnMoveStatus == PawnMoveState.Moving && PawnMoveStatus != PawnMoveState.Fainted)
        {
            PawnMoveStatus = PawnMoveState.Stationary;
            SpecificAnimPlayer.Play("None");
            DistanceMovedByThisPawn = 0;
            PrevDistance = 0;
        }
    }
    void ReplenishAmmo()
    {
        int ByHowMuch = 999;
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
                SpecificAnimPlayer.Play("Range_Attack"); 
            }
            else
            {
                SpecificAnimPlayer.Play("Melee_Attack");
            }
        }
    }
    public void RSSP() // reset selected status player
    {
        if(PCNP == null)
        {
            GD.Print("pionek nie ma kontrolera gracza ");
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
    public void ActivateCollision()
    {
        // dopóki nie znajdę gdzie jest to wywoływane to niech to tu będzie 
    }
}
