using System.ComponentModel;
using Stateless;
using Stateless.Graph;

namespace SKHelpers.Plugins.StateMachine;

public class StateMachinePlugin<TState, TTrigger>(
    StateMachine<TState, TTrigger> stateMachine,
    Dictionary<string, TState> stateMap,
    Dictionary<string, TTrigger> triggerMap)
{
    [KernelFunction]
    [Description("Configures a state transition.")]
    public StateMachine<TState, TTrigger>.StateConfiguration ConfigureTransition(
        [Description("The source state.")]
        string sourceState,
        [Description("The destination state.")]
        string destinationState,
        [Description("The trigger.")]
        string trigger)
        => stateMachine.Configure(stateMap[sourceState])
            .Permit(triggerMap[trigger], stateMap[destinationState]);

    [KernelFunction]
    [Description("Fires a trigger to transition states.")]
    public void FireTrigger(
        [Description("The trigger to fire.")]
        string trigger) => stateMachine.Fire(triggerMap[trigger]);

    [KernelFunction]
    [Description("Activates the state machine.")]
    public void Activate() => stateMachine.Activate();

    [KernelFunction]
    [Description("Deactivates the state machine.")]
    public void Deactivate() => stateMachine.Deactivate();

    [KernelFunction]
    [Description("Gets the current state of the state machine.")]
    [return: Description("The current state.")]
    public string? GetCurrentStateAsync() => stateMachine.State?.ToString();

    [KernelFunction]
    [Description("Generates a DOT graph representation of the state machine.")]
    [return: Description("The DOT graph representation.")]
    public string GenerateDotGraph()
        => UmlDotGraph.Format(stateMachine.GetInfo());
}
