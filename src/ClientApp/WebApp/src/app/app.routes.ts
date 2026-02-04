import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { LayoutComponent } from './layout/layout';
import { Notfound } from './pages/notfound/notfound';
import { AuthGuard } from './share/auth.guard';

export const routes: Routes = [
  { path: 'login', component: Login },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [AuthGuard],
    canActivateChild: [AuthGuard],
    children: [
      // ModelMod - 应用与模型管理
      {
        path: 'application',
        children: [
          { path: '', redirectTo: '/application/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/application/index/index').then(m => m.ApplicationIndex) },
        ]
      },
      {
        path: 'application-quota',
        children: [
          { path: '', redirectTo: '/application-quota/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/application-quota/index/index').then(m => m.ApplicationQuotaIndex) },
        ]
      },
      {
        path: 'ai-model-info',
        children: [
          { path: '', redirectTo: '/ai-model-info/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/ai-model-info/index/index').then(m => m.AIModelInfoIndex) },
        ]
      },
      {
        path: 'ai-model-provider',
        children: [
          { path: '', redirectTo: '/ai-model-provider/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/ai-model-provider/index/index').then(m => m.AIModelProviderIndex) },
        ]
      },
      // KnowledgeBaseMod - 知识库管理
      {
        path: 'rag-collection',
        children: [
          { path: '', redirectTo: '/rag-collection/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/rag-collection/index/index').then(m => m.RagCollectionIndex) },
        ]
      },
      {
        path: 'rag-document',
        children: [
          { path: '', redirectTo: '/rag-document/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/rag-document/index/index').then(m => m.RagDocumentIndex) },
        ]
      },
      // McpMod - MCP工具管理
      {
        path: 'mcp-tool',
        children: [
          { path: '', redirectTo: '/mcp-tool/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/mcp-tool/index/index').then(m => m.McpToolIndex) },
        ]
      },
      // AIAgentMod - AI Agent管理
      {
        path: 'aiagent',
        children: [
          { path: '', redirectTo: '/aiagent/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/aiagent/index/index').then(m => m.AIAgentIndex) },
        ]
      },
      {
        path: 'agent-execution',
        children: [
          { path: '', redirectTo: '/agent-execution/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/agent-execution/index/index').then(m => m.AgentExecutionIndex) },
        ]
      },
      // AI 能力管理 - 新增调试页面
      {
        path: 'model-debug',
        children: [
          { path: '', redirectTo: '/model-debug/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/model-debug/index/index').then(m => m.ModelDebugIndex) },
        ]
      },
      {
        path: 'agent-debug',
        children: [
          { path: '', redirectTo: '/agent-debug/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/agent-debug/index/index').then(m => m.AgentDebugIndex) },
        ]
      },
      // WorkflowMod - 工作流管理
      {
        path: 'workflow',
        children: [
          { path: '', redirectTo: '/workflow/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/workflow/index/index').then(m => m.WorkflowIndex) },
        ]
      },
      {
        path: 'workflow-execution',
        children: [
          { path: '', redirectTo: '/workflow-execution/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/workflow-execution/index/index').then(m => m.WorkflowExecutionIndex) },
        ]
      },
      {
        path: 'workflow-monitor',
        children: [
          { path: '', redirectTo: '/workflow-monitor/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/workflow-monitor/index/index').then(m => m.WorkflowMonitorIndex) },
        ]
      },
      // SystemMod - 系统管理
      {
        path: 'storage-provider',
        children: [
          { path: '', redirectTo: '/storage-provider/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/storage-provider/index/index').then(m => m.StorageProviderIndex) },
        ]
      },
      {
        path: 'system-config',
        children: [
          { path: '', redirectTo: '/system-config/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/system-config/index/index').then(m => m.SystemConfigIndex) },
        ]
      }
    ],
  },
  
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', component: Notfound },
];
