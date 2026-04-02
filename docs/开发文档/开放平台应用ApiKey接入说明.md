# 开放平台应用 ApiKey 接入说明

> 面向第三方接入方，说明如何使用应用 `ApiKey` 调用模型、Agent 与 RAG 检索能力。

## ✨ 适用范围

当前开放平台已支持以下接口：

- **模型直调**：`POST /api/v1/models/chat`
- **Agent 调用**：`POST /api/v1/agents/{id}/execute`
- **RAG 检索**：`POST /api/v1/rag/search`

## 🔐 认证方式

所有开放平台接口均使用 HTTP `Authorization` 头传递应用 ApiKey：

- Header 名称：`Authorization`
- Header 值格式：`Bearer sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx`

示例：

```http
Authorization: Bearer sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

## 🚀 接入前准备

第三方在调用前，需要由平台管理员完成以下配置：

1. 创建一个**应用**
2. 为应用生成一个**ApiKey**
3. 为应用授予允许使用的**模型权限**
4. 如需调用 Agent，确保 Agent 使用的模型也已授权给该应用

如果缺少模型权限，模型直调或 Agent 执行都会返回业务错误。

## 🧭 基础地址

开放平台接口统一位于：

`{ApiServiceBaseUrl}/api/v1`

例如：

- 本地开发：`https://localhost:xxxx/api/v1`
- 内网部署：`https://your-api-host/api/v1`

## 🪜 推荐接入流程

建议第三方按下面顺序完成接入：

1. 向平台管理员申请 `ApiKey`
2. 确认当前应用已授予目标模型权限
3. 先用一个最小模型请求验证鉴权和权限
4. 再接入 Agent 或 RAG 等上层能力
5. 为调用链补充超时、重试、日志与幂等控制

## ✅ 建议先做能力探测

第三方拿到 `ApiKey` 后，建议优先调用一次自身实际会使用的开放平台能力，例如：

- 简单问答场景调用 `POST /api/v1/models/chat`
- 知识检索场景调用 `POST /api/v1/rag/search`

这样可以一次性验证 **ApiKey 是否有效**、**模型权限是否已授予**、**目标能力是否已配置完成**，比单独增加一个“应用自检”接口更贴近真实接入流程。

## 🧩 公共接入约定

- 使用 **应用 ApiKey** 调用时，无需额外传 `applicationId`
- 对于模型直调和 Agent 执行接口，即使请求体中传入了 `applicationId`，服务端也会以**当前 ApiKey 对应的应用身份**为准
- 建议所有请求都显式设置 `Content-Type: application/json`
- 建议业务侧自行生成请求 ID，并写入日志，便于问题排查

## 🤖 模型直调

### 模型直调接口

`POST /api/v1/models/chat`

### 模型直调请求体字段

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `model` | `string` | 是 | 模型名称，例如 `deepseek-chat` |
| `provider` | `string` | 否 | 指定模型提供商名称 |
| `scene` | `string` | 否 | 业务场景标记，便于审计 |
| `messages` | `array` | 是 | 对话消息列表 |
| `temperature` | `number` | 否 | 采样温度 |
| `maxTokens` | `number` | 否 | 最大输出 tokens |

### 模型直调请求示例

```json
{
  "model": "deepseek-chat",
  "scene": "CustomerService",
  "messages": [
    {
      "role": "user",
      "content": "请用一句话介绍你自己"
    }
  ],
  "temperature": 0.2,
  "maxTokens": 256
}
```

### 模型直调成功响应示例

```json
{
  "success": true,
  "content": "你好，我是一个可以帮助你完成对话与任务处理的 AI 助手。",
  "toolCalls": [],
  "usage": {
    "promptTokens": 12,
    "completionTokens": 18,
    "totalTokens": 30
  },
  "errorMessage": null
}
```

### 失败场景

- `401 Unauthorized`：ApiKey 无效或格式错误
- `403 Forbidden`：无权访问目标资源
- `200 OK + success=false`：模型提供商调用失败
- 业务错误：应用未授权使用该模型、模型被禁用、配额超限

### 模型直调 curl 示例

```bash
curl -X POST "https://your-api-host/api/v1/models/chat" \
  -H "Authorization: Bearer sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "deepseek-chat",
    "scene": "CustomerService",
    "messages": [
      {
        "role": "user",
        "content": "请用一句话介绍你自己"
      }
    ]
  }'
```

## 🧠 Agent 调用

### Agent 调用接口

`POST /api/v1/agents/{agentId}/execute`

### Agent 调用请求体字段

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `inputJson` | `string` | 否 | 传给 Agent 的输入 JSON 字符串 |

### Agent 调用请求示例

```json
{
  "inputJson": "{\"prompt\":\"请总结今天的会议重点\"}"
}
```

### Agent 调用成功响应示例

```json
{
  "executionId": "00000000-0000-0000-0000-000000000000"
}
```

返回 `202 Accepted` 表示任务已成功入队。

### 使用说明

- Agent 执行会自动以**当前应用 ApiKey 对应的应用身份**去调用模型
- 因此 Agent 所使用的模型，必须已授权给当前应用
- 如果 Agent 内部包含工具调用，工具执行能力由平台内部链路负责处理

### Agent 调用 curl 示例

```bash
curl -X POST "https://your-api-host/api/v1/agents/{agentId}/execute" \
  -H "Authorization: Bearer sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" \
  -H "Content-Type: application/json" \
  -d '{
    "inputJson": "{\"prompt\":\"请总结今天的会议重点\"}"
  }'
```

### Agent 执行结果说明

- 当前开放平台接口会先返回 `executionId`
- 本文档覆盖的开放平台范围内，当前已公开的是**提交执行接口**
- 如果第三方需要“主动轮询执行结果”或“任务完成回调”，需要由平台侧另外提供对应接口或回调方案

## 📚 RAG 检索

### RAG 检索接口

`POST /api/v1/rag/search`

### RAG 检索请求体字段

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `query` | `string` | 是 | 检索关键词 |
| `collectionId` | `guid` | 否 | 指定知识库集合 |
| `topK` | `number` | 否 | 返回数量，默认 `5` |

### RAG 检索请求示例

```json
{
  "query": "什么是默认知识库",
  "topK": 5
}
```

### RAG 检索成功响应示例

```json
{
  "items": [
    {
      "documentId": "00000000-0000-0000-0000-000000000000",
      "content": "这是命中的知识片段内容",
      "score": 0.92
    }
  ]
}
```

### RAG 检索 curl 示例

```bash
curl -X POST "https://your-api-host/api/v1/rag/search" \
  -H "Authorization: Bearer sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "什么是默认知识库",
    "topK": 5
  }'
```

## 💻 TypeScript 最小示例

下面示例展示如何使用 `fetch` 调用模型直调接口：

```ts
const apiKey = "sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
const baseUrl = "https://your-api-host/api/v1";

const response = await fetch(`${baseUrl}/models/chat`, {
  method: "POST",
  headers: {
    Authorization: `Bearer ${apiKey}`,
    "Content-Type": "application/json"
  },
  body: JSON.stringify({
    model: "deepseek-chat",
    scene: "CustomerService",
    messages: [
      {
        role: "user",
        content: "请用一句话介绍你自己"
      }
    ]
  })
});

const result = await response.json();
console.log(result);
```

## 🧱 C# 最小示例

下面示例展示如何使用 `HttpClient` 调用模型直调接口：

```csharp
using System.Net.Http.Headers;
using System.Net.Http.Json;

var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://your-api-host/api/v1/")
};

httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", "sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");

var response = await httpClient.PostAsJsonAsync("models/chat", new
{
    model = "deepseek-chat",
    scene = "CustomerService",
    messages = new[]
    {
        new
        {
            role = "user",
            content = "请用一句话介绍你自己"
        }
    }
});

response.EnsureSuccessStatusCode();
var result = await response.Content.ReadAsStringAsync();
Console.WriteLine(result);
```

## ❓ 常见问题

### 是否必须传 `applicationId`

不必须。使用应用 `ApiKey` 调用开放平台接口时，服务端会自动识别当前应用身份。

### 为什么返回 `200`，但业务仍然失败

模型调用链可能已经成功到达服务端，但下游模型提供商执行失败，或者当前应用没有对应模型权限。此时应优先检查响应体中的 `success`、`errorMessage` 等业务字段。

### Agent 为什么只返回 `executionId`

因为 Agent 执行是异步提交模型。当前开放平台公开能力以“提交任务”为主，如需查询结果，需要平台侧额外提供查询或回调方案。

## ⚠️ 错误处理建议

第三方接入时，建议至少处理以下状态：

| 状态码 | 说明 | 建议处理方式 |
| --- | --- | --- |
| `200` | 请求成功 | 正常解析结果 |
| `202` | 异步任务已接收 | 保存 `executionId`，后续查询执行结果 |
| `400` | 请求体不合法 | 修正参数后重试 |
| `401` | ApiKey 无效 | 检查密钥是否正确或是否过期 |
| `403` | 无权限 | 检查 `applicationId`、模型权限、资源归属 |
| `500` | 服务端错误 | 记录请求 ID 并联系平台管理员 |

## 📝 接入建议

1. 先用一个最小的模型或 RAG 请求验证 ApiKey 与能力配置是否可用
2. 模型直调优先用于简单问答与生成场景
3. Agent 更适合多步骤推理、工具调用与复杂任务编排
4. RAG 检索适合知识库问答、资料召回与检索增强生成
5. 对模型与 Agent 调用增加超时、重试和幂等控制
