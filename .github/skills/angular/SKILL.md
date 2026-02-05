---
name: angular
description: Angular 21+ 前端开发规范（standalone/Material/signals）。用于 Angular 组件/页面/路由/表单、Material UI、signals 状态、i18n、前端样式与交互相关任务。
---

## 何时使用

本技能适用于使用 Angular 框架进行前端开发的项目。

---

## 项目结构

### 目录布局

```
src/ClientApp/WebApp/
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

**核心原则**：

- **100% Standalone 组件**：不使用 NgModule
- **Angular Material**：统一的 UI 组件库
- **Signals 优先**：使用新的响应式 API
- **严格的类型安全**：TypeScript 严格模式
---

## 开发流程

0. 先调用MCP 工具，从Api文档生成客户端请求服务，`outputPath`参数为前端的`src\ClientApp\WebApp\src\app`目录的绝对路径,不要再添加子路径了。clientType为:NgHttp。这一步需要将`AppHost`项目启动起来之后，才能正常调用工具生成请求服务。
1. 创建独立组件：目录及文件结构
2. 配置路由和菜单
3. 实现ts逻辑和HTML模板
4. **执行构建验证**（必须步骤）：验证编译无错误
5. 检查导入和依赖

优先通过使用 MCP 工具生成组件，Perigon提供`通过razor 模板生成代码`的能力，以获取可参考的代码结构和示例。

**特别注意**：生成的请求服务代码不要添加或修改，它是与接口保持一致的，包括类型定义，切勿重复定义类型。

---

## 构建验证（每次修改后必须执行）

### 验证前端构建
```pwsh
cd src/ClientApp/WebApp
npm run build
```

### 常见构建错误及解决
1. **TypeScript 类型错误**：检查接口定义和类型注解
2. **模块未找到**：检查 import 路径和 tsconfig.json
3. **Angular 编译错误**：检查组件装饰器和模板语法
4. **依赖缺失**：执行 `pnpm install`

### 实时开发验证（可选）
```pwsh
npm run start  # 启动开发服务器，实时查看编译错误
```

### 构建-修复循环
修改代码 → 构建 → 发现错误 → 修复 → 重新构建，直到无错误

## 组件开发

- `enumText`管道：用于将枚举值转换为对应的文本显示。
- `toKeyValue`管道：用于将枚举类型转换为键值对数组，以便在选项列表中使用。

### Standalone 组件
- ✓ **必须**：所有组件使用 standalone 模式
- ✗ **禁止**：创建或使用 NgModule

### Angular Material

**常用组件**:
| 组件类型 | 导入 | 用途 |
|----------|------|------|
| 表格 | `MatTableModule` | 数据表格展示 |
| 表单 | `MatFormFieldModule`, `MatInputModule` | 表单输入 |
| 按钮 | `MatButtonModule` | 操作按钮 |
| 对话框 | `MatDialogModule` | 弹窗交互 |
| 分页器 | `MatPaginatorModule` | 分页控制 |

### 样式管理

**样式层级**：
- **全局样式**：`styles.scss` - 基础样式和重置
- **主题样式**：`theme.scss` - Material 主题定制
- **CSS 变量**：`vars.scss` - 颜色、间距等变量
- **组件样式**：每个组件的 `.scss` 文件 - 局部样式

**样式规范**：
- ✓ 组件样式保持局部作用域
- ✗ 避免全局样式覆盖（除非主题需要）
- 使用 SCSS 变量和混入
- ✗ 不要在组件中使用内联样式，而是在scss中定义。

### 多语言

- 禁止使用硬编码字符串，而是定义和使用i18nKeys
- 使用 `translate` 管道进行文本翻译
---

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

<!-- ❌ 避免：formControlName 使用字符串硬编码 -->
<form [formGroup]="form">
  <input matInput formControlName="name" />
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

---

## 服务和 API

### 服务位置
- **路径**：`app/services/`
- **HTTP 客户端**：`admin-client.ts` / 自定义客户端
- **基础服务**：`base.service.ts`
- **模型定义**：`models/` 和 `services/`

### HTTP 拦截器

**拦截器配置**：
- **customer-http.interceptor.ts**：保持激活
- 自动处理认证 Token
- 统一错误处理

### API 代理

**开发环境代理**：
- **配置文件**：`proxy.conf.json`
- 开发服务器自动转发 API 请求
- 避免 CORS 问题

**示例配置**：
```json
{
  "/api": {
    "target": "http://localhost:5000",
    "secure": false,
    "changeOrigin": true
  }
}
```

---

## 路由

### 路由配置
- **主路由**：`app/app.routes.ts`
- **页面组件**：`app/pages/*`
- **布局外壳**：`app/layout/*`

### 路由守卫

**认证守卫**：
- **位置**：`app/share/auth.guard.ts`
- **服务**：配合 `auth.service.ts` 使用

### 懒加载

**路由配置**：
```typescript
export const routes: Routes = [
  {
    path: 'users',
    loadComponent: () => 
      import('./pages/user/user-list.component')
        .then(m => m.UserListComponent),
    canActivate: [authGuard]
  }
];
```

**规范**：
- ✓ 适当使用路由懒加载
- ✓ 路由级提供者就近配置

---

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
<button>{{ 'common.save' | translate }}</button>
```

---

### 分页器

**自定义分页器**：
- **位置**：`share/custom-paginator-intl.ts`
- 本地化分页标签

---

## 模板语法

### 控制流语法（Angular 17+）

**必须使用新的控制流语法**，不再使用旧的结构指令：

| 旧语法 (❌) | 新语法 (✓) |
|------------|-----------|
| `*ngIf` | `@if` / `@else` |
| `*ngFor` | `@for` |
| `*ngSwitch` | `@switch` / `@case` |

**示例：**

```html
<!-- ❌ 旧语法 - 禁止使用 -->
<div *ngIf="isLoading">Loading...</div>
<div *ngFor="let item of items">{{ item }}</div>

<!-- ✓ 新语法 - 必须使用 -->
@if (isLoading) {
  <div>Loading</div>
} @else {
  <div>Content</div>
}

@for (item of items; track item.id) {
  <div>{{ item.name }}</div>
}

@switch (status) {
  @case ('active') { <span>Active</span> }
  @case ('pending') { <span>Pending</span> }
  @default { <span>Unknown</span> }
}
```

**注意事项**：
- ✓ `@for` 循环必须包含 `track` 表达式
- ✓ Material Table 的 `*matHeaderRowDef` / `*matRowDef` 保留不变（这些是 Material 特有指令）
- ✗ 不要在新项目中使用 `*ngIf` / `*ngFor`

### 数据绑定和管道

**属性绑定和插值**：
```html
<!-- 属性绑定 -->
<button [disabled]="isLoading">Submit</button>
<img [src]="imageUrl" [alt]="description" />

<!-- 插值表达式 -->
<h1>{{ title }}</h1>

<!-- 事件绑定 -->
<button (click)="handleClick()">Click</button>
```

**管道使用**：
```html
<!-- 日期和翻译管道 -->
<span>{{ createdTime | date: 'short' }}</span>
<h2>{{ i18nKeys.common.title | translate }}</h2>

<!-- title 属性中使用管道 -->
<button [title]="i18nKeys.common.view | translate">
  <mat-icon>visibility</mat-icon>
</button>
```

---

## Angular 约定

**规范**：
- ✓ 优先使用 async pipe 或 signals
- ✗ 避免手动订阅（除非必要）
- ✓ 必须手动订阅时使用 `takeUntilDestroyed`

**ARIA 属性**：
- 使用适当的 `aria-*` 属性
- 遵循 Material 的无障碍模式
- 确保键盘导航可用

**最佳实践**：
- 避免在模板中使用函数调用
- 合理使用 `trackBy` 函数

**避免操作**：
- ✗ 未经要求不执行 build
- ✗ 未经要求不执行 test
- ✓ 修改后检查编辑器诊断
---