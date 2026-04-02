import { inject, Injectable } from '@angular/core';
import { AgentDebugService } from './services/agent-debug.service';
import { AgentExecutionService } from './services/agent-execution.service';
import { AIAgentService } from './services/aiagent.service';
import { AIModelInfoService } from './services/aimodel-info.service';
import { AIModelProviderService } from './services/aimodel-provider.service';
import { ApplicationService } from './services/application.service';
import { ApplicationModelPermissionService } from './services/application-model-permission.service';
import { ApplicationQuotaService } from './services/application-quota.service';
import { ApplicationRagCollectionPermissionService } from './services/application-rag-collection-permission.service';
import { ApplicationToolPermissionService } from './services/application-tool-permission.service';
import { ChatMessageService } from './services/chat-message.service';
import { ConversationService } from './services/conversation.service';
import { FileUploadService } from './services/file-upload.service';
import { MCPServerInfoService } from './services/mcpserver-info.service';
import { McpToolService } from './services/mcp-tool.service';
import { ModelDebugService } from './services/model-debug.service';
import { ModelInvocationService } from './services/model-invocation.service';
import { RagChunkService } from './services/rag-chunk.service';
import { RagCollectionService } from './services/rag-collection.service';
import { RagDocumentService } from './services/rag-document.service';
import { StorageProviderService } from './services/storage-provider.service';
import { SystemConfigService } from './services/system-config.service';
import { SystemUserService } from './services/system-user.service';
import { ToolCallRecordService } from './services/tool-call-record.service';
import { WorkflowService } from './services/workflow.service';
import { WorkflowExecutionService } from './services/workflow-execution.service';
@Injectable({
  providedIn: 'root'
})
export class AdminClient {
  /** AgentDebug */
  public agentDebug = inject(AgentDebugService);
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
  /** 应用知识库关联管理 */
  public applicationRagCollectionPermission = inject(ApplicationRagCollectionPermissionService);
  /** 应用工具权限管理 */
  public applicationToolPermission = inject(ApplicationToolPermissionService);
  /** 聊天消息 控制器 */
  public chatMessage = inject(ChatMessageService);
  /** 对话实例 */
  public conversation = inject(ConversationService);
  /** 文件上传管理 */
  public fileUpload = inject(FileUploadService);
  /** MCP Server 管理 */
  public mCPServerInfo = inject(MCPServerInfoService);
  /** MCP 工具管理 */
  public mcpTool = inject(McpToolService);
  /** ModelDebug */
  public modelDebug = inject(ModelDebugService);
  /** 模型调用记录管理 */
  public modelInvocation = inject(ModelInvocationService);
  /** 文档分块管理 */
  public ragChunk = inject(RagChunkService);
  /** 知识库管理 */
  public ragCollection = inject(RagCollectionService);
  /** 文档管理（仅管理，不包含处理逻辑） */
  public ragDocument = inject(RagDocumentService);
  /** 存储服务商 */
  public storageProvider = inject(StorageProviderService);
  /** 系统配置 */
  public systemConfig = inject(SystemConfigService);
  /** 系统用户 */
  public systemUser = inject(SystemUserService);
  /** MCP 调用记录管理 */
  public toolCallRecord = inject(ToolCallRecordService);
  /** 工作流管理 */
  public workflow = inject(WorkflowService);
  /** 工作流执行管理 */
  public workflowExecution = inject(WorkflowExecutionService);
}
