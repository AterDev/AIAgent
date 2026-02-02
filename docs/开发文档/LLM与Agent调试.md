# LLM与Agent调试

实现在管理端可以调用当前系统中的LLM和Agent。

## 后端功能说明

1. 后端应该提供通用的LLM调用接口，并支持stream和非stream两种调用方式。
2. 后端应该提供通用的Agent调用接口，支持传入不同的工具和参数，Agent不支持非stream调用。 

### 实现

LLM相关内容在`ModelMod`模块中实现：

直接使用`OpenAI` IChatClient进行LLM调用，支持常见的LLM模型参数配置。

Agent相关内容在`AIAgentMod`模块中实现：

使用`Microsft Agent Framework`进行Agent调用，支持传入不同的工具和参数。


## 前端实现说明

在前端`model-debug`和`agent-debug`两个页面组件中实现LLM和Agent的调试功能，要实现常规的输入输出展示功能，能够正常渲染文本、图片等内容(markdown)，并且支持stream流式输出。
支持停止当前请求功能。

注意：有良好的多语言支持和组件控件的含义说明。

