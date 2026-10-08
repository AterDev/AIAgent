const assert = require('node:assert/strict');
const { HttpAgent } = require('@ag-ui/client');
const { EventType } = require('@ag-ui/core');

async function main() {
  const url = process.argv[2];
  if (!url?.startsWith('http://127.0.0.1:')) throw new Error('A local mock endpoint is required.');
  const agent = new HttpAgent({ url, threadId: 'sdk-test', initialMessages: [
    { id: 'user-sdk', role: 'user', content: 'hello' }
  ] });
  const events = [];
  await agent.runAgent({ runId: 'sdk-run', forwardedProps: {} }, {
    onEvent: ({ event }) => { events.push(event); }
  });
  assert.equal(events[0].type, EventType.RUN_STARTED);
  assert.equal(events.at(-1).type, EventType.RUN_FINISHED);
  assert.equal(events.find(e => e.type === EventType.TEXT_MESSAGE_CONTENT).delta, '你好 👋');
  const call = events.find(e => e.type === EventType.TOOL_CALL_START);
  const result = events.find(e => e.type === EventType.TOOL_CALL_RESULT);
  assert.equal(result.toolCallId, call.toolCallId);
  assert.equal(call.toolCallName, 'lookup');
  const metrics = events.find(e => e.type === EventType.CUSTOM && e.name === 'debug.metrics');
  assert.equal(metrics.value.totalTokens, 12);
  assert.equal(metrics.value.toolCallCount, 1);
  assert.equal(agent.messages.find(m => m.role === 'assistant' && m.content === '你好 👋').content, '你好 👋');
  console.log('AG-UI .NET host -> JS HttpAgent: text, tools, metrics and completion passed.');
}
main().catch(error => { console.error(error); process.exitCode = 1; });
