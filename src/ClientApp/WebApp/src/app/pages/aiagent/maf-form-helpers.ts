import { AgentCapabilities } from 'src/app/services/admin/models/entity/agent-capabilities.model';
import { AgentMemoryMode } from 'src/app/services/admin/models/entity/agent-memory-mode.model';
import { I18N_KEYS } from 'src/app/share/i18n-keys';

/**
 * 能力多选选项（位标志）。
 */
export const AGENT_CAPABILITY_OPTIONS: { value: AgentCapabilities; labelKey: string }[] = [
  { value: AgentCapabilities.Tools, labelKey: I18N_KEYS.aiagent.capTools },
  { value: AgentCapabilities.Streaming, labelKey: I18N_KEYS.aiagent.capStreaming },
  { value: AgentCapabilities.StructuredOutput, labelKey: I18N_KEYS.aiagent.capStructuredOutput },
  { value: AgentCapabilities.Multimodal, labelKey: I18N_KEYS.aiagent.capMultimodal },
  { value: AgentCapabilities.Handoff, labelKey: I18N_KEYS.aiagent.capHandoff },
  { value: AgentCapabilities.HumanInTheLoop, labelKey: I18N_KEYS.aiagent.capHumanInTheLoop },
  { value: AgentCapabilities.Rag, labelKey: I18N_KEYS.aiagent.capRag },
  { value: AgentCapabilities.Mcp, labelKey: I18N_KEYS.aiagent.capMcp },
];

/** 记忆模式下拉选项。 */
export const AGENT_MEMORY_OPTIONS: { value: AgentMemoryMode; labelKey: string }[] = [
  { value: AgentMemoryMode.None, labelKey: I18N_KEYS.aiagent.memoryNone },
  { value: AgentMemoryMode.Window, labelKey: I18N_KEYS.aiagent.memoryWindow },
  { value: AgentMemoryMode.Summary, labelKey: I18N_KEYS.aiagent.memorySummary },
];

/** 把按位组合的 capabilities 展开为勾选值数组。 */
export function capabilitiesToArray(value: AgentCapabilities | number | null | undefined): AgentCapabilities[] {
  const v = value ?? 0;
  return AGENT_CAPABILITY_OPTIONS.map(o => o.value).filter(flag => (v & flag) === flag);
}

/** 把多选数组合并为按位组合值。 */
export function arrayToCapabilities(values: AgentCapabilities[] | null | undefined): AgentCapabilities {
  return (values ?? []).reduce((acc, v) => acc | v, 0 as AgentCapabilities);
}

/** 把逗号分隔字符串转换为字符串数组（自动 trim，过滤空值）。 */
export function csvToArray(input: string | null | undefined): string[] {
  if (!input) return [];
  return input.split(',').map(s => s.trim()).filter(s => s.length > 0);
}

/** 把字符串数组拼接为逗号分隔字符串，便于在单行输入框中展示和编辑。 */
export function arrayToCsv(values: string[] | null | undefined): string {
  return (values ?? []).join(', ');
}
