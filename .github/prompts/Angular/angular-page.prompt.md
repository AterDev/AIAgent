# angular页面生成

生成在`src/app/pages/实体名称/`目录下的页面代码。目录命名遵循angular小写加中划线命名法。

生成基本的curd页面，包括列表页面、添加页面、编辑页面、详情页面。

使用最新的angular material组件库,直接使用组件样式，不需要自定义样式，布局使用`bootstrap-grid`提供的栅格系统。

列表页: index.html/index.ts/index.scss
添加页: add.html/add.ts/add.scss
编辑页: edit.html/edit.ts/edit.scss
详情页: detail.html/detail.ts/detail.scss

按照angular 最佳实践进行代码组织和编写，即组**件在单独的目录**中，每个组件包含.ts,.html,.scss三个文件。

## 页面内容生成

根据模板内容生成对应的页面代码。然后基于实际需求进行调整，包括代码上的纠错，修复明显的问题，补充遗漏的功能等。

## 请求服务和DTO说明

在`src/app/services/`目录下，是根据后台swagger生成的请求服务代码，按接口服务进行划分，如
`admin`的接口服务就对应`src/app/services/admin/admin-client.ts`.

以及这个服务所依赖的相关数据类型DTO，在`src/app/services/admin/models/`目录下，根据当前的模块进行进一步的子目录划分。

## 流程控制

- 了解项目结构和技术栈以及代码规范
- 根据模板内容生成对应的页面代码
- 检查 生成的代码，修复明显的问题，补充遗漏的功能，确保代码符合规范，支持中英文语言。
