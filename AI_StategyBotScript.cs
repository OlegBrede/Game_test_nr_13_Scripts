using Godot;
using System;
using System.Collections.Generic;

public partial class AI_StategyBotScript : Node2D
{
    public string MyteamID = "";
    public GameMNGR_Script gameMNGR_Script;
    bool Activated = false; // środek prewencyjny przed inicjalizowaniem AI dwa razy 
    class CommanderCommand
    {
        public PawnBaseFuncsScript Com_Pawn { get; set; } // commanded pawn
        public PawnAiControlerNodeScript Com_PawnController { get; set; }
        public enum FirstActionsForThisPawn
        {
            Move,Melee_Attack,Strong_Melee_Attack,Range_Attack,Aim_Shot,Overwatch
        }
        public enum SecondActionsForThisPawn
        {
            Move,Melee_Attack,Strong_Melee_Attack,Range_Attack
        }
    }
    // #################### PAMIĘĆ BOTA #######################
    Dictionary<PawnBaseFuncsScript,PawnAiControlerNodeScript> MyPawnDictionay = new Dictionary<PawnBaseFuncsScript, PawnAiControlerNodeScript>(); // bo do obsługi pionka, jest potrzebny pionek i jego kontroller odpowiedniego typu względem wykonawcy 
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
                MyPawnDictionay.Add(MyPawns,MyPawns.AICNP as PawnAiControlerNodeScript);
                GD.Print($"[AI team {MyteamID}] MÓJ PIONEK TO {MyPawns.UnitName}");
            }
            else
            {
                EnemyPawnsList.Add(MyPawns);
                GD.Print($"[AI team {MyteamID}] PIONEK DRUŻYNY {MyPawns.TeamId} WYKRYTY");
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
