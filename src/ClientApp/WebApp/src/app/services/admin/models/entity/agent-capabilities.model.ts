/**
 * Agent 能力标志（按位组合）
 */
export enum AgentCapabilities {
  /** None */
  None = 0,
  /** Tools */
  Tools = 1,
  /** Streaming */
  Streaming = 2,
  /** StructuredOutput */
  StructuredOutput = 4,
  /** Multimodal */
  Multimodal = 8,
  /** Handoff */
  Handoff = 16,
  /** HumanInTheLoop */
  HumanInTheLoop = 32,
  /** Rag */
  Rag = 64,
  /** Mcp */
  Mcp = 128,
}
