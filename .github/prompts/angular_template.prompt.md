# angular_template

使用`Razor`语法编写模板内容，以便Razor引擎生成对应的前端页面代码。

## 具体说明

- 前端框架:
  - Angular 20+
  - Angular Material 最新版本
  - Bootstrap 5 栅格系统用于布局

- 生成内容：
  - 列表页面组件: index.html.razor/index.ts.razor
  - 详情页面组件: detail.html.razor/detail.ts.razor
  - 添加页面组件: add.html.razor/add.ts.razor
  - 编辑页面组件: edit.html.razor/edit.ts.razor
  - 详情页面组件: detail.html.razor/detail.ts.razor

- 要求：
  - 充分利用razor中可使用`charp`语言的逻辑处理能力，通过方法封装来简化模板内容，避免复杂的模板语法拼接。
  - 使用最新的angular material组件库。直接使用组件样式，不需要自定义样式，布局使用`bootstrap-grid`提供的栅格系统。
  - 默认优先使用signal处理响应式数据，以及angular20+推荐的写法。
  - 组件文件名称不需要加Component后缀。
  - 特别注意 razor语法中`@`符号的使用，避免和Angular模板语法冲突。
  - 生成后要检查代码的正确性，确保可以直接使用。
  - 生成前端内容时，避免与razor语法冲突，如angular前端模板也会使用`@for/@if`等。


导入以下内容以支持多语言，以及请求服务:

```typescript
import { AdminClient } from 'src/app/services/admin/admin-client';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
```

如当前是用户实体，则使用`this.adminClient.user.xxx`进行调用：

- 添加是调用`this.adminClient.user.add(dto:UserAddDto)`
- 编辑是调用`this.adminClient.user.update(id:string,dto:UserUpdateDto)`
- 获取详情是调用`this.adminClient.user.detail(id:string)`
- 获取列表是调用`this.adminClient.user.filter(filterDto:UserFilterDto)`
- 删除是调用`this.adminClient.user.delete(id:string)`

对应的类型分别是：

- 列表筛选使用`UserFilterDto`
- 列表项使用`UserItemDto`
- 添加使用`UserAddDto`
- 编辑使用`UserUpdateDto`
- 详情使用`UserDetailDto`

DTO的路径根据它的模块和名称组合，路径示例`src/app/services/admin/models/user-mod/user-add-dto.model.ts`。


## 执行步骤

1. 通过perigon MCP工具的`创建razor模板`，获取Razor模板中可使用的变量和上下文信息。
2. 根据规范和页面具体要求生成页面组件代码，要充分利用razor语法带来的csharp逻辑功能，避免复杂的模板拼接或语法冲突。
3. 通过perigon MCP工具的`通过razor模板生成代码示例`，来验证生成的模板是否可用，并修正可能存在的问题。
4. 将验证过的模板内容保存到`templates\Angular`目录下。

## 列表页面

列表由筛选组件(toolbar)和table组件组成。

- 筛选组件通过`XXXFilterDto`中包含的字段生成对应的筛选组件，如字符串使用输入框，枚举使用下拉框，布尔值使用开关等。添加按钮(add组件)放在筛选行的最右侧。
- 表格组件通过`XXXItemDto`中的字段生成对应的列，包含操作列，支持分页功能。操作列包含查看详情(detail/{id})，编辑(edit)，删除按钮
- 删除要求弹出确认对话框，使用已经封装好的组件`ConfirmDialogComponent`。
- 编辑默认也使用弹窗的方式调用编辑组件`edit`。
- 添加时弹窗调用添加组件`add`，添加成功后刷新列表。
- 对枚举类型的显示，使用`EnumTextPipe`进行展示；对日期类型，使用`date`管道进行格式化显示，默认格式为`yyyy-MM-dd HH:mm:ss`。

## 添加页面

- 同时支持弹窗和路由两种方式打开。根据是否有弹窗的数据决定。
- 根据`XXXAddDto`生成对应的表单组件。并实现添加功能，添加成功后关闭弹窗或跳转回列表页面。
- 要有完整的表单验证，根据`XXXAddDto`中的验证特性生成对应的验证规则。

## 编辑页面
- 同时支持弹窗和路由两种方式打开。根据是否有弹窗的数据决定。
- 要先根据id获取详情数据进行回填，id通过路由参数或弹窗数据传入。
- 根据`XXXUpdateDto`生成对应的表单组件。并实现编辑功能，编辑成功后关闭弹窗或跳转回列表页面。
- 要有完整的表单验证，根据`XXXUpdateDto`中的验证特性生成对应的验证规则。

## 详情页面

- 同时支持弹窗和路由两种方式打开。根据是否有弹窗的数据决定。
- 根据id获取详情数据进行展示，id通过路由参数或弹窗数据传入。
- 根据`XXXDetailDto`生成对应的详情展示组件。
- 包含返回按钮，返回列表页面。

## 表单内容说明

- ts中使用 `get`来定义控件，在html使用[formControl]绑定。
- 表单使用最新的响应式表单。
- 使用统一的 `getValidatorMessage`方法获取验证错误信息，支持多语言翻译。
- 对枚举类型控件的支持，可使用 `ToKeyValuePipe` 管道转换为键值对数组进行绑定，以及使用`EnumTextPipe`进行展示，以更方便的构造枚举类型的下拉框和显示，注意导入正确路径的管道。
- 对大于200长度的控件使用textarea输入框。
- 对bool和或时间类型选择合适的组件，如checkbox和datepicker。

表单处理的示例代码:

```typescript
  get username() {return this.loginForm.get('username') as FormControl;}
  get password() {return this.loginForm.get('password') as FormControl;}

    getValidatorMessage(control: FormControl | null): string {
    if (!control || !control.errors) {
      return '';
    }

    const errors = control.errors;
    const errorKeys = Object.keys(errors);
    if (errorKeys.length === 0) {
      return '';
    }

    const key = errorKeys[0];
    const params = errors[key];
    const translationKey = `validation.${key.toLowerCase()}`;
    return this.translate.instant(translationKey, params);
  }
```

```html
<form [formGroup]="loginForm" (ngSubmit)="doLogin()" class="login-form">
  <mat-form-field appearance="outline">
    <mat-label>{{i18nKeys.login.username|translate}}</mat-label>
    <input matInput type="text" placeholder="用户名4位以上" [formControl]="username" required>
    @if (username.invalid && username.touched) {
    <mat-error>{{getValidatorMessage(username)}}</mat-error>
    }
  </mat-form-field>
  <mat-form-field appearance="outline">
    <mat-label>{{i18nKeys.login.password|translate}}</mat-label>
    <input type="password" matInput placeholder="密码长度6位(含6位)以上" [formControl]="password" required>
    @if (password.invalid && password.touched) {
    <mat-error>{{getValidatorMessage(password)}}</mat-error>
    }
  </mat-form-field>
  
  <div class="login-btn-row">
    <button mat-flat-button  type="submit"
      [disabled]="loginForm.invalid">{{i18nKeys.login.login|translate}}</button>
  </div>
</form>
```

枚举下拉处理示例：
```html
 <mat-select placeholder="选择分类" formControlName="articleCatalog">
  @for (item of UserArticleCatalog | toKeyValue; track item) {
    <mat-option [value]="item.value">
      {{item.value| enumText:'UserArticleCatalog'}}
    </mat-option>
  }
</mat-select>
```