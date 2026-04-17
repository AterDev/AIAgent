using System.Diagnostics;

namespace AIAgentMod.Services.Maf;

/// <summary>
/// 全局 Agent/Workflow/Tool 遥测 ActivitySource。
/// 通过环境变量 OTEL_INSTRUMENTATION_GENAI_CAPTURE_MESSAGE_CONTENT=true 可开启消息内容抓取。
/// </summary>
public static class AgentTelemetry
{
    public const string SourceName = "AIAgent";

    public static readonly ActivitySource Source = new(SourceName, "1.0.0");

    public static Activity? StartAgentRun(string agentName, string modelId)
    {
        var activity = Source.StartActivity("Agent.Run", ActivityKind.Client);
        activity?.SetTag("agent.name", agentName);
        activity?.SetTag("agent.model", modelId);
        activity?.SetTag("gen_ai.system", "openai");
        return activity;
    }

    public static Activity? StartToolInvoke(string toolName)
    {
        var activity = Source.StartActivity("Tool.Invoke", ActivityKind.Internal);
        activity?.SetTag("tool.name", toolName);
        return activity;
    }

    public static Activity? StartWorkflowStep(string stepName, string stepType)
    {
        var activity = Source.StartActivity("Workflow.Step", ActivityKind.Internal);
        activity?.SetTag("workflow.step_name", stepName);
        activity?.SetTag("workflow.step_type", stepType);
        return activity;
    }
}
