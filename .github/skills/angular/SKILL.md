---
name: angular
description: Angular 21+ 前端开发规范（standalone/Material/signals）。用于 Angular 组件/页面/路由/表单、Material UI、signals 状态、i18n、前端样式与交互相关任务。
---

## 何时使用

本技能适用于使用 Angular 框架进行前端开发的任务。

## 项目结构

### 目录布局

```
src/
  ├── main.ts                    # 应用入口
  ├── app/
  │   ├── app.config.ts          # 应用配置
  │   ├── app.routes.ts          # 路由配置
  │   ├── layout/                # 布局容器
  │   ├── pages/                 # 页面组件
  │   ├── share/
  │   │   ├── components/        # 共享组件
  │   │   ├── pipe/              # 管道
  │   │   ├── auth.guard.ts      # 路由守卫
  │   │   ├── custom-paginator-intl.ts
  │   │   └── i18n-keys.ts       # i18n 键定义
  │   └── services/              # 服务和 API 客户端
  ├── assets/
  │   └── i18n/*.json            # 多语言文件
  ├── environments/              # 环境配置
  ├── styles/
  │   ├── styles.scss            # 全局样式
  │   ├── theme.scss             # Material 主题
  │   └── vars.scss              # CSS 变量
  └── proxy.conf.json            # 开发代理配置
```

<rules>

- **Standalone 组件**：不使用 NgModule
- **Angular Material**：统一的 UI 组件库
- **Signals 优先**：使用新的响应式 API
- **严格的类型安全**：TypeScript 严格模式
- ✓ 优先使用 async pipe 或 signals
- 避免在模板中使用函数调用
- 合理使用 `trackBy` 函数
</rules>

<workflow>

0. 生成前端请求服务及类型：通过perigon 提供的命令行工具`generate request`或`perigon mcp`工具，根据后端swagger文件生成前端代码，
`outputPath`参数为前端项目根目录下的`\src\app`目录的绝对路径(生成错了要删除)。
1. 创建独立组件：目录及文件结构
2. 配置路由和菜单
3. 实现ts逻辑和HTML模板
4. **执行构建验证**（必须步骤）：验证编译无错误
5. 检查导入和依赖

优先通过使用 MCP 工具生成组件，Perigon提供`通过razor 模板生成代码`的能力，以获取可参考的代码结构和示例。

**特别注意**：生成的请求服务代码不要添加或修改，它是与接口保持一致的，包括类型定义，切勿重复定义类型。

</workflow>

## 构建验证（每次修改后必须执行）

### 验证前端构建
```pwsh
npm run build
```

### 实时开发验证（可选）
```pwsh
npm run start  # 启动开发服务器，实时查看编译错误
```

### 构建-修复循环
修改代码 → 构建 → 发现错误 → 修复 → 重新构建，直到无错误

## 组件开发

- `enumText`管道：用于将枚举值转换为对应的文本显示。
- `toKeyValue`管道：用于将枚举类型转换为键值对数组，以便在选项列表中使用。

### UI/UX设计

具备良好的交互常识和布局和审美。要考虑排版、边距，文字大小，颜色搭配，组件间距等，优先参考已定义好的`theme.scss`中的样式变量和组件样式，保持整体风格一致。

**样式层级**：
- **全局样式**：`styles.scss` - 基础样式和重置
- **主题样式**：`theme.scss` - Material 主题定制
- **CSS 变量**：`vars.scss` - 颜色、间距等变量
- **组件样式**：每个组件的 `.scss` 文件 - 局部样式

**样式规范(重要)**：
- 先理解`theme.scss`中定义的 Material 主题和组件样式变量，优先使用这些样式。
- 使用Angular Material提供的组件和样式类，而不是自己定义样式类和样式。
- 关注行内元素垂直居中对齐
- 整体页面不要出现水平滚动条(内部表格除外)，要注意组件的宽度和外层容器的宽度关系
- ✗ 不要在组件中使用内联样式，而是在scss中定义。

### 多语言

- 禁止使用硬编码字符串，而是定义和使用i18nKeys
- 使用 `translate` 管道进行文本翻译

### 表单管理

**Reactive Forms（推荐）**：
```typescript
import { FormControl, FormGroup } from '@angular/forms';

userForm = new FormGroup({
  name: new FormControl('', [Validators.required]),
  email: new FormControl('', [Validators.required, Validators.email])
});

// 类型安全
get nameControl() {
  return this.userForm.get('name') as FormControl;
}
```

**表单规范**：
- ✓ 使用类型化表单（Typed Forms）
- ✓ 提供清晰的验证消息
- ✓ 表单逻辑保留在组件中
- ✓ **在 FormGroup 内优先使用 `[formControl]`，不要用 `formControlName`**
- ✓ **在 TypeScript 中定义 getter 访问控件，避免字符串硬编码**
- ✓ 通过 getter 复用控件，保持一致性和类型安全

**模板示例**：
```html
<!-- ✅ 推荐：使用 [formControl] + getter，避免字符串硬编码 -->
<form [formGroup]="form">
  <mat-form-field>
    <mat-label>Name</mat-label>
    <input matInput [formControl]="name" />
    @if (name.errors) {
      <mat-error>{{ getValidatorMessage(name) }}</mat-error>
    }
  </mat-form-field>
</form>

```

**组件示例（通过 getter 访问）**：
```typescript
export class UserForm {
  form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]]
  });

  // ✅ 通过 getter 访问控件，避免字符串，便于复用和重构
  get name() { return this.form.get('name') as FormControl; }
  get email() { return this.form.get('email') as FormControl; }

  getValidatorMessage(control: AbstractControl | null): string {
    if (!control || !control.errors) { return ''; }
    const errors = control.errors;
    const key = Object.keys(errors)[0];
    const params = errors[key];
    return this.translate.instant(`validation.${key.toLowerCase()}`, params);
  }
}
```

## 服务和 API

### 服务位置
- **路径**：`app/services/`
- **HTTP 客户端**：`admin-client.ts` / 自定义客户端
- **基础服务**：`base.service.ts`
- **模型定义**：`models/` 和 `services/`

## 路由

### 路由配置
- **主路由**：`app/app.routes.ts`
- **页面组件**：`app/pages/*`
- **布局外壳**：`app/layout/*`

### 路由守卫

**认证守卫**：
- **位置**：`app/share/auth.guard.ts`
- **服务**：配合 `auth.service.ts` 使用

## 国际化（i18n）

### 文件结构

**翻译文件**：
- **位置**：`assets/i18n/*.json`
- **键定义**：`app/share/i18n-keys.ts`
- **键生成脚本**：`scripts/i18n-keys.js`

**JSON 文件**：
```json
{
  "common": {
    "save": "保存",
    "cancel": "取消",
    "delete": "删除"
  },
  "user": {
    "list": "用户列表",
    "detail": "用户详情"
  }
}
```

**使用方式**：
```html
<button>{{ i18nKeys.common.save | translate }}</button>
```

## 模板语法

**注意事项**：
- ✓ `@for` 循环必须包含 `track` 表达式
- ✓ Material Table 的 `*matHeaderRowDef` / `*matRowDef` 保留不变（这些是 Material 特有指令）
- ✗ 不要在项目中使用 `*ngIf` / `*ngFor`，应该使用 `@if` / `@for` 语法
