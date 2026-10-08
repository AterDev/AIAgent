# Desktop：WPF / Avalonia

## 1. 桌面交互模型

- 把窗口视为可缩放工作区，不按单一分辨率设计。支持调整大小、DPI/文本缩放、最小尺寸和多显示器；仅在确有内容下限时设置合理 `MinWidth`/`MinHeight`。
- 优先保留系统标题栏、窗口命令、焦点视觉、主题和高对比度行为。除非产品确有无边框窗口需求，不自绘标题栏和窗口按钮。
- 鼠标不是唯一输入。所有核心任务支持键盘；高频命令提供标准快捷键和可发现入口，菜单/标签使用 access key；不要改变用户熟悉的 Copy、Paste、Save、Undo 等组合键。
- Escape 关闭临时层或取消未提交操作；Enter 只在安全且明确时触发默认动作。关闭含未保存更改的窗口前提供保存、放弃、取消路径。

## 2. 布局控件

| 布局需要 | WPF / Avalonia 首选 | 使用边界 |
|---|---|---|
| 通用表单、仪表盘、可伸缩行列 | `Grid` | 使用 `Auto`、`*` 与最小尺寸，避免大量固定像素 |
| 顶栏/侧栏/底栏围绕主内容 | `DockPanel` | 明确最后一个子项填充行为 |
| 少量控件单向排列 | `StackPanel` | 不用于需要跨行列对齐的大表单 |
| 空间不足自动换行 | `WrapPanel` | 适合标签、卡片或工具项，不保证表格式对齐 |
| 等尺寸项目 | `UniformGrid` | 不适合内容长度差异大的项 |
| 分层覆盖 | Avalonia `Panel` / WPF `Grid` | 仅用于 overlay、badge 等真实叠层 |
| 坐标型画布 | `Canvas` | 仅图表、绘图、设计器；不用来做普通窗口布局 |

滚动通常只放在主内容区域，避免嵌套 `ScrollViewer`。工具栏、主动作和错误摘要在内容滚动时仍应可发现。

## 3. 控件规范

- 命令：`Button` 配合 command；窗口级高频命令放 Menu/ToolBar/CommandBar 风格区域，右键菜单只是补充入口。
- 布尔：设置立即生效时 Avalonia 可用 `ToggleSwitch`；普通表单/选择使用 `CheckBox`。WPF 没有原生 ToggleSwitch 时，优先复用项目主题控件，不临时自绘。
- 单选/多选：`RadioButton` 表示少量互斥项，`CheckBox` 表示独立选择，`ComboBox` 用于较长单选列表。
- 集合：简单选择用 `ListBox`/`ListView`，层级数据用 `TreeView`，需要列比较、排序或编辑时才用 `DataGrid`/Avalonia `TreeDataGrid`。
- 导航：少量同级工作区可用 `TabControl`；层级资源使用 Tree/List-detail；不要用 tabs 表示必须依次完成的向导。
- 反馈：状态栏展示持续的非阻断状态；进度条表达等待；Dialog 只用于必须立即决定的阻断事项；Tooltip 只作补充解释。
- 表单：标签与控件对齐，单位紧邻数值输入，错误显示在字段附近并进入辅助功能树。只读值使用可选择文本或只读控件，不用 disabled 代替 read-only。

## 4. WPF 实现约束

- 使用 MVVM binding 和 `ICommand`/RoutedCommand 表达状态与动作，不在 code-behind 复制业务状态。
- 使用 `ValidationRule`、`IDataErrorInfo` 或 `INotifyDataErrorInfo` 暴露校验；错误模板不能只显示红色边框，应有文本、tooltip/summary 和可访问描述。
- 通过 `ResourceDictionary`、Style 与 ControlTemplate 统一主题；优先动态/系统资源，不硬编码颜色。自定义控件实现正确的 UI Automation peer。
- 保持逻辑 Tab 顺序；只有视觉顺序与逻辑树无法一致时才设置 `TabIndex`。为图标和无文本控件设置 `AutomationProperties.Name`。

## 5. Avalonia 实现约束

- 优先使用内置控件、FluentTheme、ControlTheme 和语义化资源。跨 Windows/macOS/Linux 时保留各平台键盘、菜单和字体行为，不假设所有平台像素相同。
- 使用 container queries、reflowing panels 或 window-width state 构建响应式布局；不要仅在启动时读取一次窗口尺寸。
- 使用 Avalonia 数据校验，让 `DataValidationErrors` 暴露可感知错误；标准控件已有 AutomationPeer，自定义控件必须补充 `AutomationProperties` 和 peer 行为。
- 确保 Tab/Shift+Tab 可达、焦点样式清楚，并在 Windows、macOS、Linux 中至少验证目标发布平台的主题、缩放和屏幕阅读器路径。

## 6. 桌面验收

- 仅键盘可完成核心任务，焦点不会丢失到隐藏/禁用元素。
- 窗口从允许的最小尺寸到宽屏均可操作，内容不会因 DPI/字体缩放被裁切。
- 长任务不冻结 UI；可取消的任务提供取消，进度与完成/失败状态明确。
- 列表选择、未保存更改、滚动与筛选在合理导航/刷新后保持。
- 高对比度、浅色/深色主题下不依赖硬编码颜色表达状态。

## 官方依据

- [WPF overview: layout, input, commands and controls](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview/)
- [WPF data binding and validation](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/)
- [Windows controls and patterns](https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/)
- [Windows keyboard interactions](https://learn.microsoft.com/en-us/windows/apps/develop/input/keyboard-interactions)
- [Windows accessibility best practices](https://learn.microsoft.com/en-us/windows/win32/winauto/accessibility-best-practices)
- [Avalonia controls](https://docs.avaloniaui.net/controls)
- [Avalonia layout](https://docs.avaloniaui.net/docs/layout)
- [Avalonia responsive layouts](https://docs.avaloniaui.net/docs/layout/responsive-layouts)
- [Avalonia accessibility](https://docs.avaloniaui.net/docs/app-development/accessibility)
- [Avalonia data validation](https://docs.avaloniaui.net/docs/app-development/data-validation)
