using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class AI_StategyBotScript : Node2D
{
    public string MyteamID = "";
    public GameMNGR_Script gameMNGR_Script;
    bool Activated = false; // środek prewencyjny przed inicjalizowaniem AI dwa razy 
    enum Tactic
    {
        attack,retreat,defend,justDoWhatever
    }
    Tactic ChosenTactic = Tactic.justDoWhatever;
    class CommanderCommand
    {
        public PawnBaseFuncsScript Com_Pawn { get; set; } // commanded pawn
        public PawnAiControlerNodeScript Com_PawnController { get; set; } // commanded pawn controller
        public enum FirstActionsForThisPawn
        {
            Move,Melee_Attack,Strong_Melee_Attack,Range_Attack,Aim_Shot,Overwatch
        }
        public enum SecondActionsForThisPawn
        {
            Move,Melee_Attack,Strong_Melee_Attack,Range_Attack
        }
    }
    class PawnAndItsController
    {
        public PawnBaseFuncsScript PBFS { get; set; }
        public PawnAiControlerNodeScript PAICS { get; set; }
    }
    // #################### PAMIĘĆ BOTA #######################
    List<PawnAndItsController> MyPawnDictionay = new List<PawnAndItsController>(); // bo do obsługi pionka, jest potrzebny pionek i jego kontroller odpowiedniego typu względem wykonawcy 
    List<PawnBaseFuncsScript> EnemyPawnsList = new List<PawnBaseFuncsScript>();
    Stack<CommanderCommand>plannedActionsqueue = new Stack<CommanderCommand>();
    // #################### PAMIĘĆ BOTA #######################
    [Export] Timer DecisionTimer;
    public override void _Ready()
    {
        DecisionTimer.Timeout += DoActionFunc;
    }
    public void Activate()
    {
        if (Activated == true)
        {
            GD.Print($"[AI team {MyteamID}] nie ma po co włączać ai gdyż ai jest już włączone");
            return;
        }
        Activated = true;
        GD.Print($"[AI] SZTUCZNY STRATEG AKTYWOWANY DLA DRUŻYNY {MyteamID}");
        ReadTheMapSituationFunction();

        DecisionTimer.Start();
    }
    void ReadTheMapSituationFunction()
    {
        foreach (PawnBaseFuncsScript MyPawns in gameMNGR_Script.PawnBucketRef.GetChildren())
        {
            if (MyPawns.TeamId == MyteamID)
            {
                MyPawnDictionay.Add(new PawnAndItsController {PBFS = MyPawns,PAICS = MyPawns.AICNP as PawnAiControlerNodeScript});
                GD.Print($"[AI team {MyteamID}] MÓJ PIONEK TO {MyPawns.UnitName}");
            }
            else
            {
                EnemyPawnsList.Add(MyPawns);
                GD.Print($"[AI team {MyteamID}] PIONEK DRUŻYNY {MyPawns.TeamId} WYKRYTY");
            }
        }

    }
    void ChoseActioneer()
    {
        foreach (PawnAndItsController MyChosenPawn in MyPawnDictionay)
        {
            if (MyChosenPawn.PAICS.SeeFucker().Item1 == true)// jak widzi strzela
            {
                MyChosenPawn.PAICS.Shoot(MyChosenPawn.PAICS.SeeFucker().Item2);
            }
            else // jak nie widzi to podchodzi by strzelić
            {
                MyChosenPawn.PAICS.Move();// jak nie może atakować to się rusza
            }
        }
    }
    void AddActionToQueue()
    {
        plannedActionsqueue.Push(new CommanderCommand
        {
            // tu będzie zaplanowanie zadania dla tego pionka, powinno to może być w ReadTheMapSituationFunction ale nie jestem pewien
        });
    }
    void DoActionFunc() // tu będzie wykonywanie zadań 
    {
        if (plannedActionsqueue.Count == 0)
        {
            Deactivate();
        }
        else
        {
            // tutaj będzie lista która wyszukuje o kij chodzi z daną akcją tóra ma zostać wykonana i jak ją zrobić 
        }
    }
    void Deactivate()
    {
        DecisionTimer.Stop();
        Activated = false;
        // czyszczenie pamięci
        MyPawnDictionay.Clear();
        EnemyPawnsList.Clear();
        plannedActionsqueue.Clear();// tak tylko profilaktycznie
        // przejdź dalej z turami 
        if (gameMNGR_Script.TeamTurnTable.Count <= 1)
        {
            gameMNGR_Script.Call("NextRoundFunc");
        }
        else
        {
            gameMNGR_Script.Call("NextTurnFunc");
        }
    }
}
