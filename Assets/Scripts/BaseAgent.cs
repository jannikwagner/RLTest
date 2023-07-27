using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

public abstract class BaseAgent : Agent
{
    public IEvaluationManager evaluationManager;
    [HideInInspector]
    public bool useCBF = true;
    private int actionCount;
    private int maxActions = 5000;
    private int actionsPerDecision = 5;
    private Condition postCondition;
    private IEnumerable<Condition> accs;
    private IEnumerable<Condition> higherPostConditions;
    private IEnumerable<CBFApplicator> cbfApplicators;
    protected CBFDiscreteInvalidActionMasker masker = new CBFDiscreteInvalidActionMasker();

    public int ActionCount { get => actionCount; }
    public int MaxActions { get => maxActions; }
    // public int StepsPerDecision { get => stepsPerDecision; set => stepsPerDecision = value; }
    public Condition PostCondition { get => postCondition; set => postCondition = value; }
    public IEnumerable<Condition> ACCs { get => accs; set => accs = value; }
    public abstract int NumActions { get; }
    public IEnumerable<CBFApplicator> CBFApplicators { get => cbfApplicators; set => cbfApplicators = value; }
    public int ActionsPerDecision { get => actionsPerDecision; set => actionsPerDecision = value; }
    public IEnumerable<Condition> HigherPostConditions { get => higherPostConditions; set => higherPostConditions = value; }

    public virtual void ResetEnvLocal() { }
    public virtual void ResetEnvGlobal() { }
    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        actionCount = 0;
        Debug.Log(this + ": OnEpisodeBegin");
        evaluationManager.AddEvent(new ActionStartEvent { localStep = actionCount });
    }

    public void Act()
    {
        if (actionCount % actionsPerDecision == 0)
        {
            RequestDecision();
        }
        else
        {
            RequestAction();
        }
    }

    public bool EpisodeShouldEnd()
    {
        return actionCount == maxActions;
    }

    public bool CheckPostCondition()
    {
        bool postconditionReached = PostCondition != null && PostCondition.Func();
        if (postconditionReached)
        {
            AddReward(1f);
            Debug.Log(this + ": PostCondition " + PostCondition.Name + " met");
            evaluationManager.AddEvent(new PostConditionReachedEvent { postCondition = PostCondition.Name, localStep = actionCount });
        }
        return postconditionReached;
    }
    public bool CheckACCs()
    {
        bool punished = false;
        if (ACCs != null)
        {
            foreach (var acc in ACCs)
            {
                if (!acc.Func())
                {
                    OnACCViolation();
                    if (!punished)
                    {
                        AddReward(-1f);
                        evaluationManager.AddEvent(new ACCViolatedEvent { acc = acc.Name, localStep = actionCount });
                    }
                    punished = true;
                    Debug.Log(this + ": ACC " + acc.Name + " violated");
                }
            }
        }
        return punished;
    }
    public bool CheckHigherPostConditions()
    {
        bool reached = false;
        if (HigherPostConditions != null)
        {
            foreach (var hpc in HigherPostConditions)
            {
                if (hpc.Func())
                {
                    OnACCViolation();
                    if (!reached)
                    {
                        // AddReward(1f);  // probably should not give reward
                        evaluationManager.AddEvent(new HigherPostConditionReachedEvent { postCondition = hpc.Name, localStep = actionCount });
                    }
                    reached = true;
                    Debug.Log(this + ": HPC " + hpc.Name + " reached");
                }
            }
        }
        return reached;
    }

    protected abstract void OnACCViolation();

    public override void OnActionReceived(ActionBuffers actions)
    {
        actionCount++;
        base.OnActionReceived(actions);
        AddReward(-1f / maxActions);
        bool done = CheckPostCondition();
        if (!done)
        {
            done = CheckHigherPostConditions();
        }
        if (!done)
        {
            done = CheckACCs();
        }
        if (!done && EpisodeShouldEnd())
        {
            AddReward(-1f);
            Debug.Log(this + "EpisodeShouldEnd, negative reward");
            evaluationManager.AddEvent(new LocalResetEvent { localStep = actionCount });
        }
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (!useCBF || cbfApplicators == null)
        {
            return;
        }
        if (masker == null)
        {
            masker = new CBFDiscreteInvalidActionMasker();
        }
        masker.WriteDiscreteActionMask(actionMask, cbfApplicators, NumActions);
    }
}
