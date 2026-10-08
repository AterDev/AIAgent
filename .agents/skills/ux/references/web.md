# Web：Angular Material + Bootstrap

## 1. 职责边界

- 使用 Angular Material 提供交互控件、主题、状态与可访问性语义。
- 当前模板直接引入 `src/assets/bootstrap-grid.min.css`（文件头为 Bootstrap Grid 5.1.3），不是通过 npm 安装完整 Bootstrap。优先使用该文件实际包含的 flex、display、spacing 和 responsive utilities 组织页面；不要假定 Bootstrap JavaScript 或 modal、dropdown、tooltip、button 等组件存在。
- 优先复用 `src/app/modules/share` 中已有 Material 聚合导入、共享组件、管道和 i18n。静态翻译键遵循项目的 `I18N_KEYS` 规则。
- 使用语义 HTML 建立标题、区域、列表、表单和按钮结构；`div` 加 click handler 不能替代真实 button/link。

## 2. 响应式布局

- 默认使用 `d-flex`、`flex-column`/`flex-row`、`flex-wrap`、`flex-grow-*`、`flex-shrink-*`、`align-items-*`、`justify-content-*`、`gap-*` 及其响应式变体构建布局。
- 从最窄布局开始：使用 `d-flex flex-column` 保证单列任务完整，再从合适断点切换为 `flex-md-row`、`align-items-md-*` 等宽屏排列。
- 尽量避免 `.container`、`.row`、`.col-*`。它们的固定容器宽度、负 margin、gutter、嵌套和自动换行行为容易造成溢出、额外空白或组件宽度异常，不能作为普通页面、表单和动作区的默认方案。
- 只有确实需要严格的二维十二列栅格、且 flex 无法清晰表达时才使用 `.row`/`.col-*`；使用前说明必要性，并验证窄屏、嵌套、滚动容器、dialog/card 内部和长文本下没有负 margin 或宽度问题。
- 常见表单优先使用 `d-flex flex-column gap-*`；需要并排的同组短字段时，在局部容器使用 `d-flex flex-column flex-md-row flex-wrap gap-*`，并为子项设置合理的 grow/shrink/min-width。不要用空列、`&nbsp;` 或大量独立 margin 对齐。
- 动作区在窄屏使用 `d-flex flex-column`，从合适断点切换为 `flex-md-row`；保持 DOM 顺序与键盘/阅读顺序一致，不用 CSS order 制造不同语义顺序。
- 表格在窄屏不能只靠水平滚动解决所有问题：保留关键列、允许详情展开或转换为可扫读列表；批量操作和选择状态必须仍然可发现。

## 3. Angular Material 控件规则

| 场景 | 首选 | 关键规则 |
|---|---|---|
| 页面主要动作 | `matButton` 的高强调变体 | 每个页面/区域仅一个视觉主动作 |
| 工具栏次要动作 | 文本按钮或 `mat-icon-button` | 图标按钮提供可访问名称和 tooltip |
| 文本/数字输入 | `mat-form-field` + `matInput` | 始终有可见 label；hint 与 `mat-error` 各司其职 |
| 少量互斥选项 | `mat-radio-group` | 标签完整，方向与阅读顺序一致 |
| 多选 | `mat-checkbox` | 父子选择支持 indeterminate 状态 |
| 立即生效的布尔设置 | `mat-slide-toggle` | 标签描述开启后的状态；不是提交动作 |
| 较多选项 | `mat-select` | 需要搜索或远程结果时改用 autocomplete |
| 日期 | `mat-datepicker` | 允许键盘输入并说明格式/错误 |
| 结构化数据 | `mat-table` + sort/paginator | 表头、排序状态、空态与分页标签可访问 |
| 少量同级视图 | `mat-tab-group` | 不把线性步骤或主要导航塞进 tabs |
| 分步表单 | `mat-stepper` | 仅在步骤有真实顺序和可理解进度时使用 |
| 阻断决定/短表单 | `MatDialog` | 打开后聚焦合理位置，关闭后回到触发元素 |
| 非阻断短反馈 | `MatSnackBar` | 不能作为唯一错误通道；动作简短可撤销 |
| 页面级导航 | sidenav/nav list | 当前项可识别；窄屏收起但入口保持可发现 |

不要用 placeholder 代替 label，不要让 tooltip 承载完成任务所必需的信息，不要把 card 当作所有内容的默认容器。

## 4. 表单与数据交互

- 字段标签使用业务语言，必填规则在开始输入前可知。类型、范围和跨字段规则在前后端一致，错误消息说明如何修复。
- 提交失败时保留输入；提交中阻止重复请求；成功后根据任务给出明确的返回列表、查看详情或继续创建路径。
- 搜索和筛选区区分“立即筛选”和“点击应用”模式，不混用。筛选结果更新后公布结果数量；清除筛选是明确动作。
- 数据表的行点击不能是编辑/查看的唯一入口；提供可聚焦的链接或按钮。批量动作只在存在选择时出现或启用，并显示选中数量。
- 首次加载用 progress/skeleton，后台刷新保留旧内容；空结果不显示一张只有表头的空表。

## 5. 焦点与可访问性

- 首选 Material/CDK 已有能力；自定义 overlay、菜单或组合控件时使用 Angular CDK a11y 的 focus trap、focus monitor、live announcer 等能力，不手写不完整版本。
- 异步状态消息使用合适的 live region，但避免逐键输入时频繁播报。路由切换后让页面标题/主标题和焦点变化对辅助技术可理解。
- 测试 Tab、Shift+Tab、Enter、Space、Escape、方向键和浏览器缩放；所有 hover 行为需有 focus 等价状态。
- Bootstrap 自定义校验样式/tooltip 当前存在辅助技术限制；优先 Angular Material 的可访问错误展示或原生/服务端可感知反馈。

## 官方依据

- [Angular Material components](https://material.angular.dev/components)
- [Angular Material theming](https://material.angular.dev/guide/theming)
- [Angular CDK accessibility](https://material.angular.dev/cdk/a11y/overview)
- [Bootstrap 5.1 grid](https://getbootstrap.com/docs/5.1/layout/grid/)
- [Bootstrap 5.1 flex utilities](https://getbootstrap.com/docs/5.1/utilities/flex/)
- [Bootstrap 5.1 form layout](https://getbootstrap.com/docs/5.1/forms/layout/)
- [Bootstrap 5.1 validation accessibility note](https://getbootstrap.com/docs/5.1/forms/validation/)
