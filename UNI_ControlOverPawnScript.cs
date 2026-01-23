using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
public partial class UNI_ControlOverPawnScript : Node2D
{
    [Export] public PawnBaseFuncsScript PawnScript;
    [Export] CharacterBody2D Pawn_Ref_as_Char;
    [Export] NavigationAgent2D NavAgent;
    [Export] RayCast2D ShootingRay;
    [Export] UNI_LOSRayCalcScript ShootingRayScript;
    [Export] RayCast2D CheckerRayScript;
    [Export] Area2D WideMeleeAttackArea;
    [Export] Area2D StrongMeleeAttackArea;
    [Export] UNI_AudioStreamPlayer2d ASP;
    [Export] Timer OverwatchReturnFireTimer;
    GameMNGR_Script gameMNGR_Script;
    Node2D UNI_MoveMarker;
    Area2D OverlapingBodiesArea;
    RandomNumberGenerator RNGGEN = new RandomNumberGenerator();
    public List<PawnPart> EnemyPartsToHit = new List<PawnPart>();
    float EngagementDistance = 0f;
    int BurstFireCounter = 0;
    Timer BurstFireTimer;
    // ############################### OVERWATCH ###############################
    [Export] Node2D ONB; //overwatch node bucket
    private List<PawnBaseFuncsScript> OverwatchTracked = new List<PawnBaseFuncsScript>(); // lista tych co są w obszarze OV
    Area2D OverwatchArea;
    CharacterBody2D SetTargetOV;
    // ############################### OVERWATCH ###############################
    int[] BurstfireARGints = {0,0};
    float[] BurstfireARGfoats = {0,0};
    CharacterBody2D SetTarget;
    
    public override void _Ready()
    {
        if (GetTree().CurrentScene.Name != "BaseTestScene")
        {
            GD.Print($"Scena nie jest BaseTestScene, jest {GetTree().CurrentScene.Name}");
            GD.Print("UNI_ControlOverPawnScript wyłączone ... ");
            return;
        }
        else
        {
            GD.Print("Scena jest BaseTestScene");
            GD.Print("UNI_ControlOverPawnScript włączone ... ");
        }
        ONB.Visible = false;
        OverwatchArea = ONB.GetNode<Area2D>("Area2D");
        BurstFireTimer = GetNode<Timer>("BurstFireTimer");
        BurstFireTimer.Timeout += () => BurstFireTrigger(BurstfireARGints[0],BurstfireARGints[1],BurstfireARGfoats[0],BurstfireARGfoats[1],SetTarget);
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");
        ASP.SCS = gameMNGR_Script.SCS;
        UNI_MoveMarker = GetNode<Node2D>("UNI_MoveNode");
        OverlapingBodiesArea = UNI_MoveMarker.GetNode<Area2D>("Area2D");
        OverwatchReturnFireTimer.Timeout += () => ShootOV(SetTargetOV);
    }
    // DO ZROBIENIA SĄ JESZCZE .: 
    // - DODANIE HOVER INFO NA SAMPLEBUTTON 
    // - ZRÓB QUEUE NA TO NA CO MA SKUPIĆ OKO KAMERA CZYLI ZGADUJĘ CAPTURE ACTON POWINNO DOSTAĆ LISTĘ 
    
    public void ActionMove() // wywołanie tej akcji ma sprawić poruszenie się na pozycję PosToMoveTo
    {
        GD.Print("Action move");
        ResetOverwatch();
        if (PawnScript.MovinCapability < 1)
        {
            GD.Print("Pioek chce się poruszyć ale nie może");
            return;
        }
        var path = NavAgent.GetCurrentNavigationPath();
        if (path.Length > 0)
        {
            var targetPos = path[path.Length - 1];
            PawnScript.GlobalPosition = targetPos;
            if (PawnScript.PawnMoveStatus == PawnMoveState.Moving)
            {
                float Addtive = PawnScript.PrevDistance + PawnScript.DistanceMovedByThisPawn;
                //GD.Print(Addtive);// nie mam pojęcia czemu to nie działa , ale nie jest to na tyle inwazyjne żeby się za to raptownie brać, bo pionek zurzuwając dwa punkty ruchu na to by zresetować prędkość nic nie osiągnie 
                PawnScript.PrevDistance = Addtive;
            }
            else
            {
                PawnScript.PrevDistance = PawnScript.DistanceMovedByThisPawn;
            }
            PawnScript.PawnMoveStatus = PawnMoveState.Moving;
            PawnScript.SpecificAnimPlayer.Play("Walk");
        }
        PawnScript.DeductMP(1); // Akcja opóźniona z racji na overwatch
    }
    public void ActionMeleeAttack(bool StrongOrNot,int STLI) // wywołanie tej akcji ma zadać DMG do odpowiednich celów
    {
        ResetOverwatch();
        if (PawnScript.MeleeAllowence < 1)
        {
            GD.Print("Pioek chce walczyć wręcz ale nie może");
            return;
        }
        PawnScript.DeductMP(1);
        PawnScript.PlayAttackAnim(false);
        Godot.Collections.Array<Node2D> overlaps;
        if (StrongOrNot == true)
        {
            StrongMeleeAttackArea.ForceUpdateTransform();
            overlaps = StrongMeleeAttackArea.GetOverlappingBodies();
        }
        else
        {
            WideMeleeAttackArea.ForceUpdateTransform();
            overlaps = WideMeleeAttackArea.GetOverlappingBodies();
        }
        foreach (var body in overlaps)
        {
            if (body is CharacterBody2D)
            {
                PawnBaseFuncsScript PS = body as PawnBaseFuncsScript;
                if (PS.TeamId != PawnScript.TeamId)
                {
                    float FinalDMG;
                    if (PawnScript.MeleeWeaponAllowence > 0)
                    {
                        FinalDMG = PawnScript.MeleeWeaponDamage;
                    }
                    else
                    {
                        FinalDMG = PawnScript.MeleeDamage;
                    }
                    if (StrongOrNot == true) // strong wallop 
                    {
                        FinalDMG = FinalDMG * 1.5f;
                        PS.Call("CalculateHit", (int)FinalDMG , 5f,STLI,0, PawnScript.UnitName,50f); // tu powinna być szansa na trafienie z krwawieniem
                    }
                    else // wide wallop
                    {
                        PS.Call("CalculateHit", (int)FinalDMG, 2.5f,STLI,0, PawnScript.UnitName,50f);
                    }
                }
            }
        }
        ASP.PlaySound(2,true);
    }
    public void ActionRangeAttack(bool AimedOrnot,float SFDV, float PartProbability,int STLI,int Firemode,CharacterBody2D HittenGuy,bool OVShot) //ShootingTargetLockIndex - ustala gdzie trafi strzał (w jaki index) / OV shot to bool który sprawdza czy strzałbył wynikiem OV czy nie, bo strzał pionka podczas OV nie resetuje OV, karzdy inny jednak tak 
    {
        if (OVShot == false)
        {
            ResetOverwatch();
        }
        PawnBaseFuncsScript Enemy_PBFS = null;
        GD.Print($"Pionek {PawnScript.UnitName} teraz strzela, początkowy check LOS");
        if (HittenGuy != null)
        {
            Enemy_PBFS = HittenGuy as PawnBaseFuncsScript;
        }
        if (Enemy_PBFS != null && Enemy_PBFS.PawnMoveStatus == PawnMoveState.Dead)
        {
            GD.Print($"Pionek {Enemy_PBFS.UnitName} umiera, nie  przeszkadzać");
            return;
        }
        if (HasLineOfSight(HittenGuy) == false && OVShot == true)
        {
            GD.Print($"Pionek {PawnScript.UnitName} nie trafi bo nie ma LOS a jest włączony OV"); // nie dajemy tu return bo check sprawdza śroek nie kawędź pionka, trzeba będzie poprawić kiedyś
            return;
        }
        if (PawnScript.ShootingAllowence < 1 || PawnScript.WeaponAmmo < 1)
        {
            GD.Print($"Pionek {PawnScript.UnitName} nie ma amunicji");
            return;
        }
        int WeaponDamageModified;
        bool ShowActionWideShot; // w sęsie że kamera przechodzi z celu na cel bo ukazanie tego w jednym kadrze byłoby niemożliwe 
        if (ShootingRayScript.Raylengh > 1000)
        {
            ShowActionWideShot = true;
        }
        else
        {
            ShowActionWideShot = false;
        }
        if (AimedOrnot == false)
        {
            if (OVShot == false)
            {
                PawnScript.DeductMP(1);
            }
            WeaponDamageModified = PawnScript.WeaponDamage;
        }
        else
        {
            PawnScript.DeductMP(2);
            WeaponDamageModified = PawnScript.WeaponDamage - RNGGEN.RandiRange(0,PawnScript.WeaponDamage / 2); // to powino zadziałać 
        }
        switch (Firemode)
        {
            case 2: // burst fire, ilość wystrzelonych pocisków w skrypcie bazowym pionka
                if (HittenGuy != null)
                {
                    gameMNGR_Script.CaptureAction(PawnScript.GlobalPosition, HittenGuy.GlobalPosition,ShowActionWideShot); // ustalenie kolejności wydarzeń ujętyh przez kamerę powinna być ustalona wcześniej
                }
                BurstfireARGints[0] = WeaponDamageModified;
                BurstfireARGints[1] = STLI;
                BurstfireARGfoats[0] = SFDV;
                BurstfireARGfoats[1] = PartProbability;
                SetTarget = HittenGuy;
                ASP.PlaySound(3,true);
                BurstFireTimer.Start();
            break;
            case 3: // Shotgun, ilość wystrzelonych pocisków na raz w skrypcie bazowym pionka (ten sam co burst fire)
                ClusterFire(WeaponDamageModified,STLI,SFDV,PartProbability,ShowActionWideShot,HittenGuy);
                ASP.PlaySound(1,true);
            break;
            case 4: // Explosives
                SpawnExplosiveArea();
                ASP.PlaySound(4,true);
            break;
            case 5: // Area fire (podobnie wygląda do Overwatch, ale strzela do wszystkiego "nawet własnych jednostek" DMG dalej modyfikowany przez dystans w zasięgu rażenias)
                SpawnAreaFire();
                ASP.PlaySound(5,true);
            break;
            default: // to jest jeden czyli inaczej pojedyńczy strzał
                PawnScript.WeaponAmmo--;
                if (HittenGuy != null)
                {
                    HittenGuy.Call("CalculateHit", WeaponDamageModified, SFDV + PartProbability,STLI,1, PawnScript.UnitName, EngagementDistance);
                    GD.Print($"Kość floatDice10 musi przebić nad {SFDV} dodatkowe Part probability było {PartProbability} więc razem {SFDV + PartProbability}");
                    gameMNGR_Script.CaptureAction(PawnScript.GlobalPosition, HittenGuy.GlobalPosition,ShowActionWideShot);
                }
                else
                {
                    GD.Print("Nie znaleziono celu, strzal się nie dokonał");
                }
                ASP.PlaySound(0,true);
            break;
        }
        PawnScript.PlayAttackAnim(true);
    }
    public bool HasLineOfSight(CharacterBody2D Target)
    {
        bool LOS_Sum = false;
        if (Target == null)
        {
            GD.Print("Target był null");
            return false;
        }
        CheckerRayScript.TargetPosition = Target.GlobalPosition - GlobalPosition;
        GD.Print($"globalna pozycja strzelającego {PawnScript.GlobalPosition}, globalna pozycja celu {Target.GlobalPosition}, pozycja TargetPosition {Target.GlobalPosition}");
        //await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); // await może bć tylko async a funkcja tylko void 
        CheckerRayScript.ForceRaycastUpdate();
        if (CheckerRayScript.GetCollider() == Target)
        {
            LOS_Sum = true;   
        }
        GD.Print($"LOS check to {LOS_Sum} bo to w co trafił ray to {CheckerRayScript.GetCollider()} a celem jest {Target}");
        return LOS_Sum;
    }
    void BurstFireTrigger(int WeaponDamageModified, int STLI, float SFDV, float PartProbability,CharacterBody2D HittenGuy)
    {
        if (BurstFireCounter < PawnScript.ShotsPerMP)
        {
            BurstFireCounter++;
            if (PawnScript.WeaponAmmo > 0)
            {
                PawnScript.WeaponAmmo--;
                if (HittenGuy != null && IsInstanceValid(HittenGuy))
                {
                    HittenGuy.Call("CalculateHit", WeaponDamageModified, SFDV + PartProbability,STLI,1, PawnScript.UnitName, EngagementDistance);
                    GD.Print($"Kość floatDice10 musi przebić nad {SFDV} dodatkowe Part probability było {PartProbability} więc razem {SFDV + PartProbability}");
                }
            }
            else
            {
                GD.Print("Burst Fire przerwany z racji na brak amunicji");
                BurstFireStop();
            }
        }
        else
        {
            BurstFireStop();
        }
    }
    void BurstFireStop()
    {
        BurstFireTimer.Stop();
        SetTarget = null;
        BurstFireCounter = 0;
        BurstfireARGints[0] = 0;
        BurstfireARGints[1] = 0;
        BurstfireARGfoats[0] = 0;
        BurstfireARGfoats[1] = 0;
    }
    void ClusterFire(int WeaponDamageModified, int STLI, float SFDV, float PartProbability, bool ShowActionWdeShot,CharacterBody2D HittenGuy)
    {
        PawnScript.WeaponAmmo--;
        if (HittenGuy != null)
        {
            gameMNGR_Script.CaptureAction(PawnScript.GlobalPosition, HittenGuy.GlobalPosition,ShowActionWdeShot);
        }
        for (int i = 0; i < PawnScript.ShotsPerMP; i++)
        {
            if (HittenGuy != null)
            {
                HittenGuy.Call("CalculateHit", WeaponDamageModified, SFDV + PartProbability,STLI,1, PawnScript.UnitName, EngagementDistance);
                GD.Print($"Kość floatDice10 musi przebić nad {SFDV} dodatkowe Part probability było {PartProbability} więc razem {SFDV + PartProbability}");
            }
        }
    }
    void SpawnExplosiveArea()
    {
        
    }
    void SpawnAreaFire()
    {
        
    }
    //######################################## OVERWATCH ##########################################
    
    public void ActionOverwatch()
    {
        GD.Print("Kod PRzeszel przez ActionOverwatch");
        PawnScript.OVStatus = true;
        OverwatchArea.Monitoring = true;
        InitOverwatch();
        PawnScript.DeductMP(2);
        //################################ TO CO BYO W FUNKCJOWNOWANIU
        // proponuję zrobić to tak jak w quar-ach area fire, to wtedy nie będzie trzeba się certolić z kierunkiem wzroku
    }
    private void InitOverwatch()
    {
        OverwatchTracked.Clear();// czyści listę
        foreach (Node2D body in OverwatchArea.GetOverlappingBodies()) // dodawanie pionków do listy 
        {
            if (body is not PawnBaseFuncsScript pawn)// jeśli nie jest to pionek pomiń 
            {
                GD.Print("To co trafiło do list nie jest PawnBaseFuncsScript");
                continue;
            }
            if (pawn.TeamId == PawnScript.TeamId)// jeśli jest z naszych pomiń 
            {
                GD.Print("Pionek należy do tej samej drużyny co Pionek z aktywnym OV");
                continue;
            }
            OverwatchTracked.Add(body as PawnBaseFuncsScript);
            GD.Print("Dodano pionek do listy OV");
        }
    }
    public void OverwatchOnEnemyMPChanged(CharacterBody2D enemy)
    {
        if (enemy == null)
        {
            GD.Print("Zapomniano podesłać celu do OV script");
            return; 
        }
        bool TrueIsMyOV = false;
        foreach (Node2D EnemyinOV in OverwatchArea.GetOverlappingBodies())
        {
            if (EnemyinOV == enemy)
            {
                TrueIsMyOV = true;
                GD.Print("Tak, ten pionek jest w moim Overwatch polu");
            }
        }
        if (TrueIsMyOV == false)
        {
            GD.Print("Pionek który się teraz ruszył nie był w moim polu OV");
            return; 
        }
        if (PawnScript.OVStatus == false)// Jeśli pionek nie jest w OV status
        {
            GD.Print("Pionek nie jest w OV status");
            return; 
        }
        if (PawnScript.WeaponAmmo <= 0)
        {
            GD.Print("Pionek nie ma już amunicji na overwatch ");
            ResetOverwatch();
            return;
        }
        SetTargetOV = enemy;
        OverwatchArea.ForceUpdateTransform();
        OverwatchReturnFireTimer.Start();
    }
    public void ShootOV(CharacterBody2D enemy)
    {
        if (enemy == null)
        {
            GD.Print("Zapomniano podesłać celu do OV shoot ? co nie powinno było się wydarzyć ?");
            return; 
        }
        bool TrueIsinArea = false;
        GD.Print($"szukamy {enemy.Name} w area");
        foreach(var PawnS in OverwatchArea.GetOverlappingBodies())
        {
            GD.Print($"[AREA] {PawnS.Name}");
            if (PawnS is CharacterBody2D && enemy == PawnS)
            {
                GD.Print("JEBANIEC JEST W AREA");
                TrueIsinArea = true;
            }
        }
        if (TrueIsinArea == false)
        {
            GD.Print("nie było tego kogo chcemy w area");
            return;
        }
        GD.Print("Strzał OV");
        ActionRangeAttack(
        false,
        RangeAttackEffectivenessCalculation(ShootingRayScript.Raylengh,enemy,false),
            0,
            999,
            PawnScript.Firemode
            ,enemy
            ,true
        );
    }
    public void ResetOverwatch()
    {
        GD.Print($"Doszło do resetu Overwatch dla pionka {PawnScript.UnitName}");
        OverwatchArea.Monitoring = false;
        PawnScript.OVStatus = false;
        ONB.Visible = false;
        SetTargetOV = null;
    }
    //######################################## OVERWATCH ##########################################
    // ################################# RUSZANIE SIĘ ################################# 
    public bool MovementAllowenceCalculationResult(Vector2 PosToCheck)
    {
        UNI_MoveMarker.GlobalPosition = PosToCheck;
        if (NavAgent.GetPathLength() <= PawnScript.MAD && IsTargetPositionFreeAsync()) // pokazujemy graczu że może tam stanąć 
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void DistanceMovedUpdate()
    {
        PawnScript.DistanceMovedByThisPawn = NavAgent.GetPathLength();
    }
    private bool IsTargetPositionFreeAsync() // nie wiem czemu to rozdzieliłem ale trudno 
    {
        var overlaps = OverlapingBodiesArea.GetOverlappingBodies();
        foreach (var body in overlaps)
        {
            if (body is StaticBody2D)
                return false;
            if (body is CharacterBody2D && body.GetInstanceId != PawnScript.GetInstanceId)
                return false;
        }
        return true;
    }
    // ################################# STRZELANIE ################################# 
    public float RangeAttackEffectivenessCalculation(float Raylengh,CharacterBody2D RayHittenTarget,bool TrueIsAimedShotActive) // wymagana weryfikacja efektywności kodu w praktyce 
    {
        // ################# KALKULACJA NA PODSTAWIE ZASIĘGU ######################
        float ShootingFinalDiceVal;
        bool ShotPosibility;
        float TargetRangeModifier;
        float TargetOwnMoveModifier;
        float TargetEnemyMoveModifier;
        EngagementDistance = Raylengh;
        float ModiRayLenghCorrector = Raylengh - PawnScript.DistanceZero;
        if (ModiRayLenghCorrector < PawnScript.WeaponRange)
        {
            ShotPosibility = true;
            //TargetRangeModifier = Mathf.Clamp(ShootingRayScript.Raylengh / PawnScript.WeaponRange, 0, 1f);
            TargetRangeModifier = Mathf.Clamp(ModiRayLenghCorrector / PawnScript.WeaponRange, 0, 1f) * PawnScript.Penalty_range;
            //GD.Print($"TargetRangeModifier {TargetRangeModifier}");
        }
        else
        {
            ShotPosibility = false;
            TargetRangeModifier = 11;
            //GD.Print("Pionek nie trafi");
        }
        // ################# KALKULACJA NA PODSTAWIE WŁASNEGO RUCHU ######################
        if (PawnScript.PawnMoveStatus == PawnMoveState.Moving)
        {
            TargetOwnMoveModifier = Mathf.Clamp(PawnScript.DistanceMovedByThisPawn / PawnScript.MAD, 0, 1f) * PawnScript.Penalty_shooter;
            //GD.Print($"Na cel wpływa modyfikator bo strzelec się rusza {TargetOwnMoveModifier}");
        }
        else
        {
            TargetOwnMoveModifier = 0;
        }
        // ################# KALKULACJA NA PODSTAWIE RUCHU PRZECIWNIKA ######################
        if (RayHittenTarget != null)
        {
            PawnBaseFuncsScript PBFS = RayHittenTarget as PawnBaseFuncsScript; //TO DO .: ten skrypt zakłada że każdy characterbody ma ten skrypt, sprawdź najpierw czy ma ten skrypt  
            if (PBFS.PawnMoveStatus == PawnMoveState.Moving)
            {
                TargetEnemyMoveModifier = Mathf.Clamp(PBFS.DistanceMovedByThisPawn / PBFS.MAD, 0, 1f) * PawnScript.Penalty_target;
                //GD.Print($"Na cel wpływa modyfikator bo cel się rusza {TargetEnemyMoveModifier}");
            }
            else
            {
                TargetEnemyMoveModifier = 0;
            }
        }
        else
        {
            TargetEnemyMoveModifier = 0;
        }
        // ############################# KULMINACJA WARTOŚCI KOŃCOWEJ #######################
        if (TrueIsAimedShotActive == true) // bo teraz szansa trafienia w konkretną cześć ciała może być znacznie trudniejsza niżeli taki se strzał losowy
        {
            EnemyPartsToHit.Clear();
            if (RayHittenTarget != null)
            {
                PawnBaseFuncsScript PBFS = RayHittenTarget as PawnBaseFuncsScript;
                foreach (PawnPart Part in PBFS.PawnParts)
                {
                    EnemyPartsToHit.Add(Part);
                }
                // tu kod dodający części ciała do listy 
            }
        }
        // ############### PODLICZENIE DEBUFFÓW ##################
        float penaltyTotal;
        if (ShotPosibility == true)
        {
            penaltyTotal = Mathf.Clamp(TargetRangeModifier + TargetOwnMoveModifier + TargetEnemyMoveModifier, 0f, 1f);
            ShootingFinalDiceVal = penaltyTotal * 10;
            //GD.Print($"{PrecentCalculationFunction(ShootingFinalDiceVal)}% to hit Modifiers are .: Range ({TargetRangeModifier}) PawnMovement ({TargetOwnMoveModifier}) TargetMovement ({TargetEnemyMoveModifier})");
        }
        else
        {
            penaltyTotal = 0;
            ShootingFinalDiceVal = 11;
        }
        return ShootingFinalDiceVal; // TEMP 
    }
    public int PrecentCalculationFunction(float SFDV) // shooting final dice value. daje to tu dlatego bo możliwe będzie że dla czytelności będzie łatwiej obliczać szansę trafienia w logice AI na procentach zamiast na dice value 
    {
        int Precent;
        if (SFDV < 10)
        {
            Precent = Mathf.RoundToInt(100f - (SFDV * 10f));
        }
        else
        {
            Precent = 0;
        }
        return Precent;
    }
}
