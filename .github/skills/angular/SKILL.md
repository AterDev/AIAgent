---
name: angular
description: Angular 21+ standalone/Material/signal 前端开发约定
---

## 何时使用

在 src/ClientApp/WebApp 下的前端开发工作
- 组件（Components）开发
- 路由（Routes）配置
- 服务（Services）和 API 调用
- 样式和主题定制
- 国际化（i18n）

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

---

## 开发流程

1. 创建独立组件：目录及文件结构
2. 配置路由和菜单
3. 实现ts逻辑和HTML模板
4. 检查导入和依赖

优先通过使用 MCP 工具生成组件，Perigon提供执行任务的能力，通过执行angular组件生成任务(如果有)获取模板示例代码。

## 组件开发

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
  return this.userForm.controls.name;
}
```

**表单规范**：
- ✓ 使用类型化表单（Typed Forms）
- ✓ 提供清晰的验证消息
- ✓ 表单逻辑保留在组件中
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