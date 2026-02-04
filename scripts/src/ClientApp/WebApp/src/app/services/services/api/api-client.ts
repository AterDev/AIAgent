import { inject, Injectable } from '@angular/core';
import { AgentsService } from './services/agents.service';
import { AIPromptService } from './services/aiprompt.service';
import { AppsService } from './services/apps.service';
import { ExternalAuthService } from './services/external-auth.service';
import { RagAgentConfigService } from './services/rag-agent-config.service';
import { RagSearchService } from './services/rag-search.service';
import { SystemConfigsService } from './services/system-configs.service';
import { ToolsService } from './services/tools.service';
import { WorkflowsService } from './services/workflows.service';
@Injectable({
  providedIn: 'root'
})
export class ApiClient {
  /** Open platform agents */
  public agents = inject(AgentsService);
  /** 提示词 */
  public aIPrompt = inject(AIPromptService);
  /** Open platform apps */
  public apps = inject(AppsService);
  /** ExternalAuth */
  public externalAuth = inject(ExternalAuthService);
  /** RAG 模型配置 */
  public ragAgentConfig = inject(RagAgentConfigService);
  /** Open platform RAG search */
  public ragSearch = inject(RagSearchService);
  /** Open platform system configs */
  public systemConfigs = inject(SystemConfigsService);
  /** Open platform tools */
  public tools = inject(ToolsService);
  /** Open platform workflows */
  public workflows = inject(WorkflowsService);
}
