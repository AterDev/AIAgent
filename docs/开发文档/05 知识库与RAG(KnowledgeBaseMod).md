# 05 知识库与 RAG（KnowledgeBaseMod）

## 目标

实现知识库管理、文档解析与训练、向量化、检索全流程，支持混合检索。

## 步骤

1. **知识库与文档管理基础**
   - RagDocument 与 RagChunk 实体落位（Definition/Entity/KnowledgeBaseMod）。
   - 文档状态机：pending → parsing → vectorizing → completed/failed。
   - 知识库支持公开/私有两类，并提供启用状态。
   - 文档通过分类标签与角色字段区分（均支持多值）。

2. **文件解析与训练**
   - 支持 TXT/Markdown/PDF/Office/图片。
   - 统一解析接口 IDocumentParser。
   - 分块规则：512-1024 tokens，20% 重叠，语义边界优先。
   - 解析与训练过程调用大语言模型时，提示词复用 SystemConfig 配置。
   - 大语言模型调用通过 CoreMod 封装执行。

3. **向量化与存储**
   - 调用 embedding 模型（优先 text-embedding-3-small/large）。
   - Qdrant 作为向量库，元数据携带 TenantId/DocumentId/Tags。
   - 与 PostgreSQL 的文档元数据保持一致性。

4. **混合检索**
   - 向量检索 + 关键词检索融合（70/30 权重）。
   - 相似度阈值与 Top-K 可配置。

5. **异步管道**
   - 文档解析与向量化通过 NATS 异步任务执行。
   - 失败重试与错误记录。

6. **缓存与性能**
   - Redis 缓存热检索结果。
   - 分块、向量批处理写入。

## 验收要点

- 多格式文档可上传并完成向量化。
- 检索结果包含来源、标签与相关度。
- 知识库公开/私有与启用状态生效。
