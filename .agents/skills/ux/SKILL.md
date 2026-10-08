---
name: ux
description: >-
  为 Perigon 项目设计、实现和审查 UI 与用户交互，覆盖 Web（Angular Material + Bootstrap flex/grid）、Desktop（WPF 或 Avalonia）和 Phone（Android Material）。Use when creating or revising pages, forms, navigation, dialogs, lists, tables, responsive layouts, component choices, loading/empty/error states, keyboard or touch behavior, accessibility, localization, or visual interaction consistency; also use when reviewing frontend or XAML UI for usability defects.
---

# UX

以任务完成效率、平台惯例、状态完整性和可访问性为设计约束。不要只让界面“看起来现代”；必须说明用户如何发现、操作、纠错和完成任务。

## 工作流

1. 确认目标用户、核心任务、目标平台、输入方式和现有设计系统。先检查仓库中的主题、共享组件和相邻页面，避免引入第二套视觉语言。
2. 写出主流程及加载、空数据、无结果、局部失败、整页失败、无权限、离线、提交中、提交成功等适用状态。不要只设计理想状态。
3. 先确定信息层级、导航、内容分组和动作优先级，再选择控件和颜色。每个页面只突出一个主要动作；次要与危险动作必须降低视觉权重。
4. 优先使用平台原生或框架标准控件。只有标准控件无法表达业务语义时才创建自定义控件，并补齐焦点、键盘/触摸、禁用、错误和辅助功能语义。
5. 让系统始终可感知：即时操作立即反馈；异步操作显示进度并防止重复提交；失败信息说明发生了什么以及用户下一步能做什么。
6. 按目标平台读取对应 reference，落实布局、控件与交互规范。跨平台产品分别采用平台惯例，不要求像素级一致。
7. 用真实内容、长文本、多语言、缩放、窗口/屏幕尺寸、键盘或触摸操作验证。实现后检查所有状态，不以静态截图作为完成标准。

## Reference 路由

| 任务 | 必读 reference |
|---|---|
| 所有 UI/交互设计与审查 | [references/common.md](references/common.md) |
| Angular、HTML、响应式后台或 Web 页面 | [references/web.md](references/web.md) |
| WPF、Avalonia、XAML、桌面窗口与键盘交互 | [references/desktop.md](references/desktop.md) |
| Android、手机/平板、Material 3、触摸交互 | [references/phone.md](references/phone.md) |

只加载目标平台的 reference；跨平台任务再加载多个。Angular 实现还必须同时遵循 `../perigon/references/angular.md`，UX skill 不取代项目的结构、i18n 或编码规范。

## 输出要求

- 设计或计划：给出页面区域、控件选择、主次动作、状态与响应式/窗口变化规则，并指出关键取舍。
- 实现：复用现有主题和共享组件；同时实现正常、等待、空、错误、禁用和成功状态中适用的部分。
- 审查：按“阻碍任务完成 > 数据损失或误操作 > 可访问性 > 一致性 > 视觉微调”排序问题，并给出具体替代控件或交互。
- 不确定时：先查看相邻实现和官方组件能力，不凭印象发明组件 API、尺寸或平台行为。

## 完成门槛

- 主流程无死路，返回/取消/重试行为明确。
- 控件语义与任务匹配，危险操作不能被轻易误触。
- 键盘焦点或触摸目标可用，文本缩放、对比度和辅助名称可验证。
- 加载、空、错误和提交状态不会导致布局跳变、重复提交或数据静默丢失。
- 布局适应目标窗口，不依赖单一截图尺寸或绝对坐标。
- 文案可本地化，布局能容纳更长文本，颜色和图标不是唯一的信息载体。
