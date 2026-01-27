# 代码编写工作流

此工作流文件定义了一个用于代码编写和提交的标准流程，确保代码质量和一致性。

## 角色定义

- 技术负责人: 在`agents/pm.md` 中定义
- 后端开发: 在`agents/backend.md` 中定义
- 前端开发: 在`agents/frontend.md` 中定义
- 架构师: 在`agents/perigon.md` 中定义

## 工作流程

### 循环入口
工作循环由以下任一条件触发：
- 用户通过Issue/PR提交新需求
- PM完成当前批次规划后自动进入下一批
- 所有任务完成后自动停止

### 详细流程

#### Phase 1: 需求规划 (PM)
1. PM 阅读 `docs/开发文档` 中的需求文档
2. 解析功能需求，生成**工作分解表 (WBS)**，内容包含：
   - Task ID (T001/T002/...)
   - 任务名称/目标
   - 涉及模块 (后端/前端/两者)
   - 验收标准
   - 依赖关系
   - 优先级 (1~5)
3. 按依赖关系排序任务，分配给对应的Agent

#### Phase 2: 代码实现 (Backend/Frontend Agent)
1. Agent 接收任务，确认理解需求
2. 编写代码，按 `agents/backend.md` 或 `agents/frontend.md` 规范执行
3. 完成后提交给PM审查，并清单自检项：
   - [ ] 代码遵循规范 (.github/skills/)
   - [ ] 单元测试通过
   - [ ] 无编译错误
   - [ ] 接口文档已更新

#### Phase 3: 代码审查 (PM/Tech Lead)
1. PM 进行全栈审查，参考 `agents/pm.md` 标准
2. **审查通过** → 进入 Phase 4
3. **发现问题** → 生成**缺陷单文档**（见下文格式）→ 反馈给开发Agent → 回到 Phase 2

#### Phase 4: 提交与循环继续
1. PM 确认无问题，生成 commit 信息
2. 代码提交到版本控制
3. **检查后续任务**：
   - 如有未开始任务 → 分配新任务 → 回到 Phase 1
   - 如所有任务完成 → 检查是否还有待完成需求 → 完成或继续规划

### 缺陷单文档格式 (审查失败时)
```markdown
# Defect Report: [Task ID]

**问题**: [具体描述]
**严重级别**: Critical / High / Medium / Low
**发现时间**: Phase 3 Round X
**受影响范围**: [文件/模块]

## 根本原因
[Why]

## 解决方案
[What to fix]
- 详细的修改步骤
- 代码示例（如需要）

## 验证方法
[How to verify the fix]

## 关键参考
[技能文档、相似实现、Perigon指南]
```

### 循环完成条件
- ✅ `docs/开发文档` 中的所有任务状态为 **完成**
- ✅ 代码库无编译错误
- ✅ 整体架构审查通过

## 开发需求说明

- 参考 `docs/1 技术详细设计文档.md`中的技术说明。
- 参考 `docs/开发文档`中的具体需求。
- 代码审核要着重参考`skills/perigon/SKILL.md`中的Perigon项目规范。

## Agent沟通机制

不同的角色可以通过输出文档的方式进行沟通和协作，确保每个Agent都能清晰理解自己的任务和职责。

### 文档交接规范

#### PM → 开发Agent: **任务清单 (Tasklist)**
```markdown
# Weekly Task Dispatch
**Batch ID**: W01
**Dispatch Time**: 2026-01-27
**Total Tasks**: 5

## Task: [T001] 实现用户认证模块
| 字段 | 值 |
|------|-----|
| Agent | Backend Dev |
| Module | SystemMod/User |
| Entity | User, Role, Permission |
| Deadline | 2026-01-29 |
| AC1 | 用户登陆 API 返回 JWT Token |
| AC2 | 权限管理器集成 OAuth |
| Depends | 无 |
| Priority | P1 |
| Reference | docs/开发文档/09 开放平台API与权限.md |

## Task: [T002] 前端登陆页面
| 字段 | 值 |
|------|-----|
| Agent | Frontend Dev |
| Module | WebApp/Auth |
| E2E | 输入用户名密码 → 调用T001 API → 跳转到首页 |
| Depends | T001 |
| Priority | P1 |
```

#### 开发Agent → PM: **完成通知 + 自检清单**
```markdown
# Task Completion Notification
**Task ID**: T001
**Status**: Ready for Review
**Submission Time**: 2026-01-29 10:00

## Changed Files
- src/Definition/Entity/SystemMod/User.cs
- src/Services/ApiService/AuthController.cs

## Self-Check Checklist
- [x] 代码遵循 .github/skills/backend/SKILL.md
- [x] DTO 字段验证规则已定义
- [x] 异常处理完整（无有效令牌、权限不足等）
- [x] 编译无错误
- [x] 单元测试通过 (90% coverage)
- [x] API 文档已更新 (见 OpenAPI spec)

## PR Link
https://github.com/AterDev/AIAgent/pull/123
```

#### PM → 开发Agent: **缺陷单 (如审查失败)**
```markdown
# Defect Report: T001 - Round 2

**问题**: AuthController 直接访问 DbContext，违反 Perigon 规范
**严重级别**: High
**发现时间**: Code Review Phase, 2026-01-29 11:00

## 根本原因
AuthController 中 Login() 方法没有通过 AuthManager 来访问数据，直接在 DbContext 中查询用户。

## 解决方案
按照 Perigon 规范，所有业务逻辑应在 Manager 中：
1. 创建 AuthManager extends ManagerBase<User>
2. 在 AuthManager 中实现 Login(username, password) 方法
3. AuthController.Login() → authManager.Login(...) + token生成

## 代码示例
- 参考: src/Services/ApiService/UserController.cs (L45) - 正确的 Manager 调用模式
- Manager 模式: .github/skills/backend/SKILL.md § Manager 规范

## 验证方法
- [ ] AuthManager 实现完成
- [ ] 所有DB查询都通过 Manager 进行
- [ ] 单元测试覆盖 AuthManager.Login()
- [ ] AuthController 代码行数 < 50 行

**Resubmit After**: 修复完成后重新提交审查
```

## 状态跟踪与进度管理

### 工作周期状态表 (由PM维护)
```markdown
# Development Progress - W01

| Task ID | 功能 | Agent | 状态 | Round | 开始时间 | 预计完成 | 阻塞项 |
|---------|------|-------|------|-------|---------|---------|--------|
| T001 | 用户认证 | Backend | 审查中 | 2 | 01-27 | 01-29 | - |
| T002 | 登陆页面 | Frontend | 等待中 | 1 | 待开始 | 01-30 | T001 |
| T003 | 权限管理 | Backend | 规划中 | - | 待开始 | 02-01 | T001 |
| T004 | 数据库迁移 | Backend | 完成 | 1 | 01-26 | 01-27 | - |... | | | | | | |

**Summary**: 4/7 在进行中，1个阻塞（T002等待T001），整体进度 40%
```

### 循环决策规则

| 情况 | PM 决策 | 下一步 |
|------|--------|--------|
| 所有任务完成 & 构建通过 | ✅ 完成 | 检查是否有新需求，无则结束 |
| 任务完成但有新需求 | 📋 规划新批次 | 回到 Phase 1 |
| 审查需修改 (High/Medium) | 📝 发缺陷单 | 开发Agent 修复 → 重新审查 |
| 审查需修改 (Low) | 📝 发缺陷单 | 下一个周期修复 |
| 发现架构问题 | ⚠️ 升级为设计决策 | PM 与 Architect Agent 讨论 |
| 依赖任务未完成 | ⏸️ 暂停 | 等待依赖完成 |

## 自动工作循环启动 & 监控

### 启动条件
工作循环自动进入下一个周期当满足以下条件：
1. **初始触发**: 用户通过 Issue 创建需求 或手动调用启动命令
2. **自动继续**: 当前批次所有任务进入"完成"或"已提交"状态
3. **自动停止**: 所有 `docs/开发文档` 中的需求都已实现 & 无待处理缺陷单

### 关键检查点 (Gate)

#### Gate 1: 需求合理性检查 (PM 规划前)
- [ ] 需求文档中的功能是否清晰可理解？
- [ ] 是否有技术风险或依赖障碍？
- [ ] 是否可以分解为 3~5 天内能完成的任务？

#### Gate 2: 代码审查通过 (Phase 3)
- [ ] 编译零错误
- [ ] 无规范违反 (skills 检查)
- [ ] 无数据访问层泄漏
- [ ] 无 N+1 查询或性能风险
- [ ] 错误处理完整

#### Gate 3: 集成验证 (提交前)
- [ ] 如涉及 API 更改，前端 Service 是否已适配？
- [ ] 如涉及数据库迁移，是否已更新 Entity & DbContext？
- [ ] 新增/修改的 DTO 是否通过 OpenAPI 文档导出？

### 监控与报告

#### 每日进度报告 (由PM生成)
```markdown
# Daily Progress Report - 2026-01-27

**工作周期**: W01
**当前活跃任务**: 3
**已完成**:1 (T004)
**审查中**: 1 (T001 Round 2)
**开发中**: 1 (T002 等依赖)

**风险**:
- 🔴 T001 已进入第2轮审查，延期风险增高

**预期**: T001 今日修复 & 通过
```

#### 循环完成报告 (Phase 4 最后一步)
```markdown
# Cycle Completion Summary - W01

**周期**: W01
**总任务数**: 7
**完成数**: 7 ✅
**平均轮次**: 1.2 (表示有些任务需修改)

**代码提交**:
- Commits: 7
- Lines Added: 1,250
- Files Changed: 18

**质量指标**:
- 编译成功率: 100%
- 第一次审查通过率: 71% (5/7)
- 平均审查时间: 2h

**Next Batch**: 准备启动 W02 吗？[Y/N]
```

## 启动工作循环的方式

### 方式 1: 手动启动（用户调用）
```
@perigon-tech-lead-agent 启动编码循环 W01
要求: 
- 读取 docs/开发文档/ 中所有需求
- 生成任务分解表
- 分配首批任务给开发Agent
```

### 方式 2: 自动继续（前序任务完成）
```
[系统检测] T001 & T002 均已提交 & 通过审查
→ 自动触发 PM 规划下一批
→ 分配 T003, T004, ... 给开发Agent
```

### 方式 3: 轮询模式（低频监控）
```
每 4 小时检查一次：
- 是否有完成的任务需要触发审查？
- 是否有缺陷单需要开发Agent处理？
- 是否有阻塞项可以解除？
```

## 成功指标

一个完整工作循环成功的标志：
- ✅ 所有规划任务完成 & 代码通过审查
- ✅ 无未解决的缺陷单
- ✅ 构建通过 (零编译错误)
- ✅ 代码提交历史清晰 (良好的 commit message)
- ✅ 所有 Agent 都理解下一步任务



