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
        path: 'model-profile',
        children: [
          { path: '', redirectTo: '/model-profile/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/model-profile/index/index').then(m => m.ModelProfileIndex) },
        ]
      },
      {
        path: 'model-provider',
        children: [
          { path: '', redirectTo: '/model-provider/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/model-provider/index/index').then(m => m.ModelProviderIndex) },
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
      // SystemMod - 系统管理
      {
        path: 'system-config',
        children: [
          { path: '', redirectTo: '/system-config/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/system-config/index/index').then(m => m.SystemConfigIndex) },
        ]
      },
      {
        path: 'system-role',
        children: [
          { path: '', redirectTo: '/system-role/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/system-role/index/index').then(m => m.Index) },
        ]
      },
      {
        path: 'system-user',
        children: [
          { path: '', redirectTo: '/system-user/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/system-user/index/index').then(m => m.Index) },
        ]
      },
      {
        path: 'system-logs',
        children: [
          { path: '', redirectTo: '/system-logs/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/system-logs/index/index').then(m => m.Index) },
        ]
      },
    ],
  },
  
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', component: Notfound },
];
