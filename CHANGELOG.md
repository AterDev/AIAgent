# 变更日志

## [Unreleased] - 2024-02-01

### 新增 (Added)

#### 前端管理页面 - AI 能力管理

##### 1. 模型在线调试页面
- **路径**: `/model-debug/index`
- **文件**:
  - `src/ClientApp/WebApp/src/app/pages/model-debug/index/index.ts`
  - `src/ClientApp/WebApp/src/app/pages/model-debug/index/index.html`
  - `src/ClientApp/WebApp/src/app/pages/model-debug/index/index.scss`
- **功能**:
  - 模型选择和参数配置（温度、最大 token、系统提示词）
  - 实时测试模型响应
  - Token 使用统计展示
  - 测试历史记录（最近 10 次）
  - 导出测试结果为 JSON
  - 响应式设计，支持移动端

##### 2. AI Agent 配置与调试页面
- **路径**: `/agent-debug/index`
- **文件**:
  - `src/ClientApp/WebApp/src/app/pages/agent-debug/index/index.ts`
  - `src/ClientApp/WebApp/src/app/pages/agent-debug/index/index.html`
  - `src/ClientApp/WebApp/src/app/pages/agent-debug/index/index.scss`
- **功能**:
  - Agent 选择和配置
  - 系统提示词覆盖
  - 高级参数配置（温度、流式输出、工具调用日志）
  - 实时测试 Agent 执行
  - 查看完整对话历史
  - 工具调用详情展示（输入/输出）
  - 执行历史记录和重运行
  - 导出调试会话

##### 3. 工作流编排监控页面
- **路径**: `/workflow-monitor/index`
- **文件**:
  - `src/ClientApp/WebApp/src/app/pages/workflow-monitor/index/index.ts`
  - `src/ClientApp/WebApp/src/app/pages/workflow-monitor/index/index.html`
  - `src/ClientApp/WebApp/src/app/pages/workflow-monitor/index/index.scss`
- **功能**:
  - 实时统计面板（运行中、已完成、成功率、平均耗时）
  - 运行中工作流监控（进度条、自动刷新）
  - 历史执行记录展示
  - 工作流步骤时间线
  - 步骤详情查看（输入/输出/错误）
  - 工作流取消和重试
  - 导出执行详情
  - 脉冲动画标记运行中步骤

#### 路由配置
- **文件**: `src/ClientApp/WebApp/src/app/app.routes.ts`
- 添加 `/model-debug` 路由
- 添加 `/agent-debug` 路由
- 添加 `/workflow-monitor` 路由

#### 文档
- **新增文档**: `docs/实现方案-前端管理页面/2-实现总结.md`
  - 详细的功能说明
  - 技术实现细节
  - UI/UX 特性
  - 后续改进建议
  - 测试建议
- **更新文档**: `README.md`
  - 添加新功能说明链接

### 技术实现

- 使用 Angular 21+ Standalone Components
- 使用 Signals 进行响应式状态管理
- 使用 Angular Material 21 组件库
- 使用 RxJS 实现异步数据流和自动刷新
- 响应式设计，支持桌面和移动端
- 表单验证和错误处理
- 实时数据更新（interval 轮询）

### 依赖项

无新增依赖项，使用项目现有依赖：
- @angular/core: 21.1.0
- @angular/material: 21.0.1
- @angular/forms: 21.1.0
- rxjs: 7.8.2

### 注意事项

1. **模拟数据**: 当前实现使用模拟数据，需要后端实现以下 API：
   - `POST /api/debug/model-test` - 模型调试接口
   - `POST /api/debug/agent-test` - Agent 调试接口
   - `POST /api/workflow-execution/{id}/cancel` - 取消工作流
   - `POST /api/workflow-execution/{id}/retry` - 重试工作流

2. **实时通信**: 建议后续集成 SignalR 实现真正的实时推送，替代当前的轮询机制

3. **权限控制**: 页面已配置在 AuthGuard 保护下，但操作级别的权限需要后端 API 配合

### 测试状态

- [x] TypeScript 编译通过
- [ ] 单元测试（待实现）
- [ ] E2E 测试（待实现）
- [ ] 真实后端 API 集成测试（待后端实现）

### 兼容性

- Node.js: 20.20.0+
- npm: 10.8.2+
- pnpm: 10.28.2+
- Angular: 21+
- 浏览器: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+

## 相关 Issue

- 分析缺失的 AI 能力
- 添加模型在线调试功能
- 添加 AI Agent 配置/调试界面
- 添加工作流编排监控功能

## 贡献者

- AI Assistant - 功能实现、文档编写
