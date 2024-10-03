using System.ComponentModel;
using RulesEngine.Models;
using Engine = RulesEngine.RulesEngine;
using static Newtonsoft.Json.JsonConvert;

namespace SKHelpers.Plugins.RulesEngine;

public class RulesEnginePlugin(Engine engine)
{
    [KernelFunction]
    [Description("Evaluates all rules against provided inputs.")]
    [return: Description("The results of the rule evaluations.")]
    public async Task<IEnumerable<string>> EvaluateAllRulesAsync(
        [Description("The name of the workflow.")]
        string workflowName,
        [Description("The inputs for the rule evaluation.")]
        IDictionary<string, object> inputs)
    {
        var ruleParams = inputs.Select(input => new RuleParameter(input.Key, input.Value));
        var results = await engine.ExecuteAllRulesAsync(workflowName, ruleParams);
        return results.Select(SerializeObject);
    }


    [KernelFunction]
    [Description("Evaluates a rule against provided inputs.")]
    [return: Description("The result of the rule evaluation.")]
    public async Task<string> EvaluateRuleAsync(
        [Description("The name of the workflow.")]
        string workflowName,
        [Description("The rule name to evaluate.")]
        string ruleName,
        [Description("The inputs for the rule evaluation.")]
        IDictionary<string, object> inputs)
    {
        var ruleParams = inputs.Select(input => new RuleParameter(input.Key, input.Value));
        var result = await engine.ExecuteActionWorkflowAsync(workflowName, ruleName, [.. ruleParams]);
        return SerializeObject(result);
    }

    [KernelFunction]
    [Description("Adds or updates a workflow to the rules engine.")]
    public void AddOrUpdateWorkflow(
        [Description("The workflow definition in JSON format.")]
        string workflowJson)
    {
        var workflows = DeserializeObject<Workflow[]>(workflowJson);
        engine.AddOrUpdateWorkflow(workflows);
    }

    [KernelFunction]
    [Description("Removes a workflow from the rules engine.")]
    public void RemoveWorkflow(
        [Description("The name of the workflow to remove.")]
        string workflowName)
        => engine.RemoveWorkflow(workflowName);
}
