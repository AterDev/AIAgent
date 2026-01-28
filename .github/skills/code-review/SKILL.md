---
name: code-review
description: 全栈代码审查规范 - 质量保障、性能优化、安全检查
---

## 何时使用

- 审查功能实现逻辑
- 全栈代码质量检查
- 性能瓶颈分析
- 安全风险识别

## 核心职责

### 1. 代码正确性审查

#### 后端审查要点
- **实体设计**: 
  - 是否继承 `EntityBase`？
  - 字符串是否设置 `[MaxLength]`？
  - decimal 是否设置精度（如 `[Column(TypeName = "decimal(18,2)")]`）？
  - 枚举是否有 `[Description]` 注解？
  - 外键关系是否合理？
  
- **Manager 层**:
  - 业务逻辑是否在 Manager 中而非 Controller？
  - 是否继承 `ManagerBase<T>` 或 `ManagerBase`？
  - 是否避免了 Manager 之间的直接调用？
  - 异常处理是否抛出 `BusinessException`？
  
- **Controller 层**:
  - 是否继承 `RestControllerBase`？
  - 方法名是否遵循 RESTful 约定（`AddAsync`/`UpdateAsync`/`DeleteAsync`/`GetDetailAsync`/`FilterAsync`）？
  - 是否返回 `ActionResult<T>`？
  - 是否避免直接访问 `DbContext`？
  - 错误处理是否使用 `Problem()` / `NotFound()`？
  - 是否避免使用 `ApiResponse` 包装器？
  
- **数据访问**:
  - 查询是否使用 `Select` 投影而非 `Include` 加载整个导航属性？
  - 批量操作是否使用 `EFCore.BulkExtensions`？
  - 事务操作是否使用 `ExecuteInTransactionAsync`？
  
#### 前端审查要点
- **组件设计**:
  - 是否使用 standalone 组件（避免 `NgModule`）？
  - 是否使用 Angular Material 组件库？
  - 状态管理是否优先使用 signals？
  - 订阅是否正确清理（使用 `takeUntilDestroyed` 或 async pipe）？
  
- **服务层**:
  - API 调用是否通过服务而非直接在组件中？
  - 端点配置是否使用环境变量而非硬编码？
  
- **路由和导航**:
  - 路由是否懒加载？
  - 路由守卫是否正确配置（`auth.guard.ts`）？
  
- **国际化**:
  - 字符串是否使用 i18n 而非硬编码？
  - 键名是否与 `app/share/i18n-keys.ts` 对齐？

### 2. 性能审查

- **数据库查询**:
  - 是否存在 N+1 查询问题？
  - 是否滥用 `Include` 导致过量数据加载？
  - 分页查询是否正确实现？
  - 是否缺少必要的索引？
  
- **API 设计**:
  - 列表接口是否返回分页数据而非全量数据？
  - DTO 是否只包含必要字段（避免过度传输）？
  - 是否存在冗余的 HTTP 请求？
  
- **前端性能**:
  - 组件是否存在不必要的重绘？
  - 是否滥用 `ChangeDetectorRef.detectChanges()`？
  - 列表渲染是否使用 `trackBy`？

### 3. 安全审查

- **输入验证**:
  - API 边界是否进行输入验证？
  - 是否使用 `[Required]`, `[MaxLength]`, `[Range]` 等注解？
  - 客户端验证是否与服务端一致？
  
- **权限控制**:
  - Controller 是否有适当的授权检查（`[Authorize]`）？
  - 是否存在越权访问风险？
  
- **敏感信息**:
  - 是否泄露敏感数据（如密码、token）？
  - 错误信息是否包含内部实现细节？
  
### 4. 架构和设计审查

- **模块职责**:
  - Entity/DTO/Manager/Controller 的职责是否清晰？
  - 是否存在跨层调用（如 Controller 直接访问 DbContext）？
  
- **依赖关系**:
  - 模块之间的依赖是否合理？
  - 是否存在循环依赖？
  - Manager 之间是否存在不必要的直接调用？
  
- **代码复用**:
  - 是否有重复代码应提取为共享方法？
  - 是否滥用继承导致紧耦合？

### 5. 代码风格和可维护性

- **命名规范**:
  - 变量、方法、类名是否语义清晰？
  - 是否遵循 C# / TypeScript 命名约定？
  
- **注释和文档**:
  - 复杂逻辑是否有必要的注释？
  - 公共 API 是否有 XML 文档注释？
  
- **代码结构**:
  - 是否使用 file-scoped namespace？
  - 是否使用 primary constructors？
  - 是否恰当使用 collection expressions？
  
- **错误处理**:
  - 异常是否正确捕获和处理？
  - 日志记录是否充分？

## 审查输出格式

### 问题报告模板

```markdown
# 代码审查报告

**审查范围**: [文件路径或模块名称]  
**审查日期**: [日期]  
**审查结果**: ✅ 通过 / ⚠️ 需改进 / ❌ 阻断

---

## 🔴 阻断性问题（必须修复）

### [问题标题]
- **文件**: `path/to/file.cs#L10-L15`
- **问题描述**: 明确说明问题
- **风险等级**: Critical / High
- **建议修复**:
  ```csharp
  // 修复后的代码示例
  ```
- **参考**: [相关规范文档链接]

---

## ⚠️ 改进建议

### [建议标题]
- **文件**: `path/to/file.ts#L42`
- **当前问题**: 描述不够理想的实现
- **改进方案**: 提供优化建议
- **收益**: 说明改进后的好处（性能/可维护性等）

---

## ✅ 良好实践

- [值得肯定的代码实现]
- [遵循规范的示例]

---

## 📚 参考资料

- [Backend Skill](.github/skills/backend/SKILL.md)
- [Angular Skill](.github/skills/angular/SKILL.md)
- [Perigon Best Practices](https://dusi.dev/docs/Perigon/en-US/10.0/Best-Practices/Overview.html)

## 审查原则

1. **准确性优先**: 基于项目实际规范而非个人偏好
2. **问题分级**: 区分阻断性问题、改进建议和良好实践
3. **提供方案**: 不仅指出问题，还要给出具体修复建议
4. **引用规范**: 每个问题都应引用相关 Skill 文档或官方文档
5. **保持客观**: 避免主观臆断，基于事实和规范进行评审
6. **建设性反馈**: 肯定好的实践，鼓励持续改进

## 禁止事项

- ❌ 不要重写整个文件，仅指出需要修改的部分
- ❌ 不要引入新的模式，遵循现有 Perigon 约定
- ❌ 不要降低审查标准，正确性第一
- ❌ 不要在未理解业务意图的情况下批准代码