// <auto-generate>
import { Injectable, Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'enumText'
})
@Injectable({ providedIn: 'root' })
export class EnumTextPipe implements PipeTransform {
  transform(value: unknown, type: string): string {
    let result = '';
    switch (type) {
      case 'AgentExecutionStatus':
        switch (value) {
          case 0: result = 'Running'; break;
          case 1: result = 'Completed'; break;
          case 2: result = 'Failed'; break;
          case 3: result = 'Canceled'; break;
          default: result = '默认'; break;
        }
        break;

      case 'AuthType':
        switch (value) {
          case 0: result = 'None'; break;
          case 1: result = 'ApiKey'; break;
          case 2: result = 'OAuth'; break;
          case 3: result = 'Token'; break;
          default: result = '默认'; break;
        }
        break;

      case 'ChatMessageRole':
        switch (value) {
          case 0: result = 'User'; break;
          case 1: result = 'AI'; break;
          case 2: result = 'System'; break;
          case 3: result = 'Tool'; break;
          default: result = '默认'; break;
        }
        break;

      case 'ChatMessageType':
        switch (value) {
          case 0: result = 'Text'; break;
          case 1: result = 'Image'; break;
          case 2: result = 'File'; break;
          default: result = '默认'; break;
        }
        break;

      case 'TransportType':
        switch (value) {
          case 0: result = 'Http'; break;
          case 1: result = 'Stdio'; break;
          case 2: result = 'SSE'; break;
          case 3: result = 'Websocket'; break;
          default: result = '默认'; break;
        }
        break;

      case 'RagDocumentStatus':
        switch (value) {
          case 0: result = 'Pending'; break;
          case 1: result = 'Parsing'; break;
          case 2: result = 'Vectorizing'; break;
          case 3: result = 'Completed'; break;
          case 4: result = 'Failed'; break;
          case 5: result = 'Cancelled'; break;
          default: result = '默认'; break;
        }
        break;

      case 'McpToolType':
        switch (value) {
          case 0: result = 'Builtin'; break;
          case 1: result = 'External'; break;
          case 2: result = 'Custom'; break;
          default: result = '默认'; break;
        }
        break;

      case 'ToolCallStatus':
        switch (value) {
          case 0: result = 'Success'; break;
          case 1: result = 'Failed'; break;
          case 2: result = 'Timeout'; break;
          case 3: result = 'Canceled'; break;
          default: result = '默认'; break;
        }
        break;

      case 'InvocationStatus':
        switch (value) {
          case 0: result = 'Success'; break;
          case 1: result = 'Failed'; break;
          case 2: result = 'Timeout'; break;
          case 3: result = 'Canceled'; break;
          default: result = '默认'; break;
        }
        break;

      case 'QuotaPeriodType':
        switch (value) {
          case 0: result = 'Minute'; break;
          case 1: result = 'Hour'; break;
          case 2: result = 'Day'; break;
          case 3: result = 'Month'; break;
          default: result = '默认'; break;
        }
        break;

      case 'StepExecutionStatus':
        switch (value) {
          case 1: result = 'Pending'; break;
          case 2: result = 'Running'; break;
          case 3: result = 'Completed'; break;
          case 4: result = 'Failed'; break;
          case 5: result = 'Retrying'; break;
          case 6: result = 'Skipped'; break;
          default: result = '默认'; break;
        }
        break;

      case 'WorkflowExecutionMode':
        switch (value) {
          case 0: result = 'Normal'; break;
          case 1: result = 'Resumed'; break;
          default: result = '默认'; break;
        }
        break;

      case 'WorkflowExecutionStatus':
        switch (value) {
          case 0: result = 'Running'; break;
          case 1: result = 'Completed'; break;
          case 2: result = 'Failed'; break;
          case 3: result = 'Canceled'; break;
          case 4: result = 'Pending'; break;
          case 5: result = 'Retrying'; break;
          case 6: result = 'Abandoned'; break;
          default: result = '默认'; break;
        }
        break;


      default:
        break;
    }
    return result;
  }
}
