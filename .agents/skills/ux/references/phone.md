# Phone：Android Material

## 1. 技术与设计系统

- 新 Android UI 优先采用 Material 3；现有项目使用 Jetpack Compose 时使用 `androidx.compose.material3`，使用 Views 时沿用 Material Components for Android，不为套用本规范强制改写技术栈。
- 使用 MaterialTheme 的 color scheme、typography、shape 和 spacing token；不要在各 screen/composable 中散落颜色和字号。文字使用 `sp`，尺寸和间距使用 `dp`，不使用物理像素。
- 优先标准 Material 组件，它们提供触控尺寸、状态和辅助语义基础。自定义手势/控件必须补齐 semantics、pressed/selected/disabled/error 状态和替代操作。

## 2. 页面骨架与自适应

- Compose 使用 `Scaffold` 组织 app bar、内容、FAB、snackbar 与导航；正确消费 `innerPadding`/window insets，避免状态栏、导航栏、刘海和输入法遮挡内容。
- 按应用窗口而不是设备型号或方向决定布局。使用 Window Size Classes；compact 可用单栏与 navigation bar，空间增加时切换 navigation rail、drawer 或 list-detail/supporting pane。
- 不锁定方向、不限制可调整大小或宽高比。平板、折叠屏、多窗口下避免把正文、表单和按钮无限拉宽；采用受限内容宽度或多窗格。
- 配置、窗口尺寸或折叠姿态变化后保存任务状态、输入、选择、滚动和导航位置。

## 3. Material 控件规则

| 场景 | Compose Material 3 首选 | 关键规则 |
|---|---|---|
| 页面结构 | `Scaffold` + TopAppBar | 标题说明当前上下文，动作按重要度排序 |
| 少量顶级目的地 | `NavigationBar` | 当前目的地明确，标签不能只靠图标猜测 |
| 中等/展开窗口导航 | `NavigationRail` / drawer | 随窗口尺寸适配，不按“手机/平板”硬编码 |
| 页面主要动作 | `Button` | 使用动词+对象；同屏避免多个高强调按钮 |
| 高频、全局主要创建动作 | `FloatingActionButton` | 仅一个明确主动作；不是普通“更多”菜单 |
| 布尔设置 | `Switch` | 立即生效；表单多选用 `Checkbox` |
| 互斥选项 | `RadioButton` / segmented button | 少量选项直接展示，较多选项用菜单/选择器 |
| 文本输入 | `TextField` / `OutlinedTextField` | 持续可见 label、输入类型、IME action、错误说明 |
| 内容集合 | `LazyColumn` / grid + ListItem/Card | 稳定 key、清晰选中态；卡片按单一对象分组 |
| 临时补充内容 | Bottom sheet | 不替代深层导航或必须持续查看的内容 |
| 阻断决定 | `AlertDialog` | 标题直述问题，按钮说明结果，避免多层 dialog |
| 短暂反馈 | `Snackbar` | 可提供短撤销动作；关键错误留在页面状态中 |
| 进度 | determinate/indeterminate indicator | 能计算时才显示百分比；不阻塞无关内容 |

## 4. 触摸、返回与输入

- 所有可操作目标至少 48×48dp；视觉图标可以更小，但可点击容器必须满足尺寸并与相邻目标留出空间。
- 点击立即产生视觉反馈。防止快速重复提交；长任务在生命周期变化后仍有一致状态。
- 系统 Back 行为应可预测：先关闭键盘/临时层，再返回上一导航状态；不要把返回变成“退出并丢失输入”。未保存的重要数据需明确处理。
- 滑动、长按、拖拽不能是唯一操作方式；提供可见按钮、菜单或辅助功能 action。危险手势应支持撤销或确认。
- 键盘类型和 IME action 匹配字段；输入法弹出后当前字段、错误和主要动作仍可见。不要在每次字符变化时抢焦点或显示 toast。
- 权限在用户触发相关功能时请求，先解释用途；拒绝后提供可恢复路径，不循环弹窗。

## 5. 可访问性与内容

- 使用标准 Compose/Material 组件的默认 semantics；自定义内容通过 `Modifier.semantics`、`clickable`、`toggleable`、`selectable` 等正确表达角色、状态和动作。
- 图像型按钮提供简短 `contentDescription`；装饰图像使用空描述。不要朗读屏幕上已有的重复文字。
- 支持系统字体缩放，不用固定高度裁切文本；图标、数值和标签在长翻译下仍能换行或重排。
- Material color roles 必须成对使用，例如 `primary` 上使用 `onPrimary`；定制颜色后重新验证对比度。支持动态颜色时仍保证品牌与状态可辨认。
- 内容变化的播报要克制；错误、进度完成和重要状态应可感知，但滚动列表和每次输入不应造成噪声。
- 除触摸外验证 TalkBack、外接键盘/鼠标、字体放大和横竖/多窗口变化。

## 6. 手机端验收

- 单手常用动作可达，但不以牺牲语义顺序和大屏适配为代价。
- 48dp 目标、system bars、display cutout 和 IME 均不会造成误触或遮挡。
- 返回、旋转、进后台和进程重建后，任务不会静默丢失。
- TalkBack 能读出控件名称、角色、状态、错误和可执行动作。
- compact、medium、expanded 窗口中内容层级和主要任务都完整。

## 官方依据

- [Material Design 3 in Compose](https://developer.android.com/develop/ui/compose/designsystems/material3)
- [Material components in Compose](https://developer.android.com/develop/ui/compose/components)
- [Accessibility in Jetpack Compose](https://developer.android.com/develop/ui/compose/accessibility)
- [Compose accessibility API defaults and 48dp targets](https://developer.android.com/develop/ui/compose/accessibility/api-defaults)
- [Get started with adaptive apps](https://developer.android.com/develop/adaptive-apps/guides/get-started-with-adaptive-apps)
- [Adaptive do's and don'ts](https://developer.android.com/develop/adaptive-apps/guides/adaptive-dos-and-donts)
- [Material 3 window insets](https://developer.android.com/develop/ui/compose/system/material-insets)
