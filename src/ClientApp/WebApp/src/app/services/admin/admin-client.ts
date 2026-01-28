import { inject, Injectable } from '@angular/core';
import { AgentExecutionService } from './services/agent-execution.service';
import { AIAgentService } from './services/aiagent.service';
import { AIModelInfoService } from './services/aimodel-info.service';
import { AIModelProviderService } from './services/aimodel-provider.service';
import { ApplicationService } from './services/application.service';
import { ApplicationModelPermissionService } from './services/application-model-permission.service';
import { ApplicationQuotaService } from './services/application-quota.service';
import { ApplicationToolPermissionService } from './services/application-tool-permission.service';
import { ChatMessageService } from './services/chat-message.service';
import { ConversationService } from './services/conversation.service';
import { MCPServerInfoService } from './services/mcpserver-info.service';
import { McpToolService } from './services/mcp-tool.service';
import { ModelInvocationService } from './services/model-invocation.service';
import { ModelProfileService } from './services/model-profile.service';
import { ModelProviderService } from './services/model-provider.service';
import { RagChunkService } from './services/rag-chunk.service';
import { RagCollectionService } from './services/rag-collection.service';
import { RagDocumentService } from './services/rag-document.service';
import { SystemConfigService } from './services/system-config.service';
import { ToolCallRecordService } from './services/tool-call-record.service';
import { WorkflowService } from './services/workflow.service';
import { WorkflowExecutionService } from './services/workflow-execution.service';
@Injectable({
  providedIn: 'root'
})
export class AdminClient {
  /** Agent 执行管理 */
  public agentExecution = inject(AgentExecutionService);
  /** agent */
  public aIAgent = inject(AIAgentService);
  /** 模型信息 */
  public aIModelInfo = inject(AIModelInfoService);
  /** AI模型提供商 */
  public aIModelProvider = inject(AIModelProviderService);
  /** 应用定义 */
  public application = inject(ApplicationService);
  /** 应用模型权限管理 */
  public applicationModelPermission = inject(ApplicationModelPermissionService);
  /** 应用配额管理 */
  public applicationQuota = inject(ApplicationQuotaService);
  /** 应用工具权限管理 */
  public applicationToolPermission = inject(ApplicationToolPermissionService);
  /** 聊天消息 控制器 */
  public chatMessage = inject(ChatMessageService);
  /** 对话实例 */
  public conversation = inject(ConversationService);
  /** MCP Server 管理 */
  public mCPServerInfo = inject(MCPServerInfoService);
  /** MCP 工具管理 */
  public mcpTool = inject(McpToolService);
  /** 模型调用记录管理 */
  public modelInvocation = inject(ModelInvocationService);
  /** 模型配置管理 */
  public modelProfile = inject(ModelProfileService);
  /** 模型提供商管理 */
  public modelProvider = inject(ModelProviderService);
  /** 文档分块管理 */
  public ragChunk = inject(RagChunkService);
  /** 知识库管理 */
  public ragCollection = inject(RagCollectionService);
  /** 文档管理 */
  public ragDocument = inject(RagDocumentService);
  /** 系统配置 */
  public systemConfig = inject(SystemConfigService);
  /** MCP 调用记录管理 */
  public toolCallRecord = inject(ToolCallRecordService);
  /** 工作流管理 */
  public workflow = inject(WorkflowService);
  /** 工作流执行管理 */
  public workflowExecution = inject(WorkflowExecutionService);
}
