using Godot;
using System;
using System.Collections.Generic;
public partial class UNI_ControlOverPawnScript : Node2D
{
    [Export] public PawnBaseFuncsScript PawnScript;
    [Export] NavigationAgent2D NavAgent;
    [Export] UNI_LOSRayCalcScript ShootingRayScript;
    [Export] Area2D WideMeleeAttackArea;
    [Export] Area2D StrongMeleeAttackArea;
    GameMNGR_Script gameMNGR_Script;
    Node2D UNI_MoveMarker;
    Area2D OverlapingBodiesArea;
    RandomNumberGenerator RNGGEN = new RandomNumberGenerator();
    public List<PawnPart> EnemyPartsToHit = new List<PawnPart>();
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
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");
        UNI_MoveMarker = GetNode<Node2D>("UNI_MoveNode");
        OverlapingBodiesArea = UNI_MoveMarker.GetNode<Area2D>("Area2D");
    }
    // DO ZROBIENIA SĄ JESZCZE .: 
    // - DODANIE HOVER INFO NA SAMPLEBUTTON 
    // - PRZY OKAZJI NAPRAW TEN BUG PRZY POBIERANIU OBJĘTOŚCI
    // - PRZYWRÓĆ FUNKCJONALNOŚĆ KODU ODPOWIEDZIALNEGO ZA GENEROWANBIE PROPORCJONALNEJ BAŃKI NA PODSTAWIE RADIUS COLLISIONSHAPE 2D SPHEARE ?
    // - NAPRAW I PRZYWRÓĆ OVERWATCH
    // - BRONIE MAJĄ MIEĆ TYPY STRZAŁÓW
    // - 
    public void ActionMove() // wywołanie tej akcji ma sprawić poruszenie się na pozycję PosToMoveTo
    {
        PawnScript.MP--;
        gameMNGR_Script.TeamsCollectiveMP--;
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
        }
    }
    public void ActionMeleeAttack(bool StrongOrNot,int STLI) // wywołanie tej akcji ma zadać DMG do odpowiednich celów
    {
        PawnScript.MP--;
        gameMNGR_Script.TeamsCollectiveMP--;
        PawnScript.PlayAttackAnim();
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
                    if (StrongOrNot == true) // strong wallop 
                    {
                        PS.Call("CalculateHit", PawnScript.MeleeDamage * 1.5f , 5f,STLI, PawnScript.UnitName);
                    }
                    else // wide wallop
                    {
                        PS.Call("CalculateHit", PawnScript.MeleeDamage, 2.5f,STLI, PawnScript.UnitName);
                    }
                }
            }
        }
    }
    public void ActionRangeAttack(bool AimedOrnot,float SFDV, float PartProbability,int STLI) //ShootingTargetLockIndex
    {
        int WeaponDamageModified;
        if (AimedOrnot == false)
        {
            PawnScript.MP--;
            gameMNGR_Script.TeamsCollectiveMP--;
            WeaponDamageModified = PawnScript.WeaponDamage;
        }
        else
        {
            PawnScript.MP -= 2;
            gameMNGR_Script.TeamsCollectiveMP -= 2;
            if (PawnScript.MP < 0)
            {
                GD.PrintErr("Błąd kalkulacji MP przy strzale wycelowanym");
                gameMNGR_Script.TeamsCollectiveMP++;
            }
            WeaponDamageModified = PawnScript.WeaponDamage - RNGGEN.RandiRange(0,PawnScript.WeaponDamage / 2); // to powino zadziałać 
        }
        PawnScript.WeaponAmmo--;
        PawnScript.PlayAttackAnim();
        if (ShootingRayScript.RayHittenTarget != null)
        {
            ShootingRayScript.RayHittenTarget.Call("CalculateHit", WeaponDamageModified, SFDV + PartProbability,STLI, PawnScript.UnitName);
            GD.Print($"Kość floatDice10 musi przebić nad {SFDV} dodatkowe Part probability było {PartProbability} więc razem {SFDV + PartProbability}");
            gameMNGR_Script.Call("CaptureAction", PawnScript.GlobalPosition, ShootingRayScript.RayHittenTarget.GlobalPosition);
        }
    }
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
