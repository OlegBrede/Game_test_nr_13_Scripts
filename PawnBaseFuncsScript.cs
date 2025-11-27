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
    [Export] public float DistanceZero = 10000; // dystans który jest objętością pionka w lokalnych jednostkach raycast'u by wszystko grało spójnie
    [Export] public int WeaponDamage = 85; // może w wypadku broni białej może dałoby się mieć "rzut" zamiast strzału ? ale znowu , trzeba byłoby jakoś coś zrobić z tym tamtym mieczem shotgun
    [Export] public int WeaponAmmo = 7;
    [Export] public int WeaponMaxAmmo = 7;
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
    public float PrevDistance = 0;
    public float ObjętośćPionka = 0;
    public int kills = 0; // TEMP
    public Node2D TargetMarkerRef;
    public PawnMoveState PawnMoveStatus { get; set; } = PawnMoveState.Standing;
    public PawnStatusEffect PawnsActiveStates = PawnStatusEffect.None;
    GameMNGR_Script gameMNGR_Script;
    private bool AC = false;
    RandomNumberGenerator RNGGEN = new RandomNumberGenerator();
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
            return;
        }
        else
        {
            GD.Print("Scena jest BaseTestScene");
        }
        gameMNGR_Script = GetTree().Root.GetNodeOrNull<GameMNGR_Script>("BaseTestScene"); // to dalej daje error, pomyślę nad rozwiązaniem 
        //GD.Print("skrypt przechodzi dalej ... ");
        RNGGEN.Randomize();
        UNIAnimPlayerRef.Play("StandStill");
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
    void ActivateCollision()
    {
        AC = true;
    }
    public void CalculateHit(int dmg, float probability,int Where, string Bname)
    {
        float FloatDice = RNGGEN.RandfRange(0, 10);
        if (FloatDice >= probability) // rzucarz na liczbę powyżej kostki
        {
            //GD.Print($"hit on {FloatDice}");
            gameMNGR_Script.GenerateActionLog($"{Bname} Hit {UnitName}");
            TakeDamage(dmg,Where);
        }
        else
        {
            //GD.Print($"miss on {FloatDice}");
            gameMNGR_Script.GenerateActionLog($"{Bname} Missed {UnitName}");
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
        int finalBodyPart; // końcowa część ciała do kalkulacji lokacji trafienia (bo losowy strzał może trafić w cześć ciała która nie ma już HP)
        int PlacementRoll_INDEX;
        string W_co;
        if (Where <= PawnParts.Count())
        {
            PlacementRoll_INDEX = Where;
            W_co = PawnParts[Where].Name;
            GD.Print($"Nielosowy strzał w {W_co}");
        }
        else
        {
            PlacementRoll_INDEX = LocationRollCalc();
            W_co = PawnParts[PlacementRoll_INDEX].Name;
            GD.Print($"Losowy strzał w {W_co}");
        }
        int LeftoverDMG = 0;
        if (PawnParts[PlacementRoll_INDEX].HP > 0 && PawnParts[PlacementRoll_INDEX].HP >= dmg)// jeśli dana część ciała ma jeszcze HP i DMG jest mniejszy lub równy HP
        {
            PawnParts[PlacementRoll_INDEX].HP -= dmg; //DMG dociera do części ciała
            finalBodyPart = PlacementRoll_INDEX; //index
        }
        else if(PawnParts[PlacementRoll_INDEX].HP < dmg) // jeśli DMG jest większy od HP
        {
            PawnParts[PlacementRoll_INDEX].HP -= dmg; //odejmujemy DMG
            LeftoverDMG = PawnParts[PlacementRoll_INDEX].HP * -1; //odwracając ujemny DMG dostajemy ile DMG będzie transferowana w górę
            GD.Print($"Dana część nie wytrzyma DMG, więc resztę obrażeń ({LeftoverDMG}) dostanie rodzic ...");
            PawnParts[PlacementRoll_INDEX].HP = 0; //ustawiamy HP na 0 by nie było tu ujemnej wartości
            DecreseFightCapability(PawnParts[PlacementRoll_INDEX].MeleeCapability, PawnParts[PlacementRoll_INDEX].ShootingCapability,PawnParts[PlacementRoll_INDEX].EsentialForMovement);
            int FoundDamageRecypiant = FindPartToDamage(PlacementRoll_INDEX); //szukamy rodzica który ma dostać DMG
            PawnParts[FoundDamageRecypiant].HP -= LeftoverDMG;// od zdrowia rodzica zostaje odjęty DMG
            W_co = PawnParts[FoundDamageRecypiant].Name; // ustawienie nazwy części ciała
            finalBodyPart = FoundDamageRecypiant; // index ostatniej części ciała która dostanie DMG
        }
        else // jeśli Trafiony obszar nie ma już HP i dostał DMG 
        {
            GD.Print("Dana część jest już zepsuta idziemy dalej z DMG ...");
            int FoundDamageRecypiant = FindPartToDamage(PlacementRoll_INDEX); // Znajdujemy rodzica do transferu DMG
            PawnParts[FoundDamageRecypiant].HP -= dmg; //Aplikacja DMG
            W_co = PawnParts[FoundDamageRecypiant].Name; // ustawienie nazwy części ciała
            finalBodyPart = FoundDamageRecypiant; // index ostatniej części ciała która dostanie DMG
        }
        UNIAnimPlayerRef.Play("Damage");
        gameMNGR_Script.GenerateActionLog($"{UnitName} took {dmg} damage to the {W_co}");
        Integrity = 0; // czyszczenie numery który wyświetla integralność pionka (te tamte procenty)
        foreach (var Bodypart in PawnParts)
        {
            Integrity += Bodypart.HP;
        }
        //GD.Print($"{UnitName} took {dmg} damage");
        //GD.Print($"dany pionek ma {CriticalParts.Count()} krytyczne części");
        //if (PawnParts[finalBodyPart].HP <= 0)
        //{
            //GD.Print($"efektywność {UnitName} spada...");
            // uprzednio tu był DecreseFightCapability ale został on przeniesiony w górę bo tu nie działał
            // jeśli to natomiast dalej nie będzie działąło to postaraj się przepisać to tu rekurencyjnie, na wypadek gdyby DMG przeszedł hen dalej 
        //}
        foreach (string CriticalPart in CriticalParts)
        {
            if (CriticalPart == PawnParts[finalBodyPart].Name && PawnParts[finalBodyPart].HP <= 0)
            {
                GD.Print($"dany pionek dostał w {PawnParts[finalBodyPart].Name}");
                Die();
            }
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
    public void PlayAttackAnim()
    {
        if (SpecificAnimPlayer != null)
        {
            SpecificAnimPlayer.Play("Attack");
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
