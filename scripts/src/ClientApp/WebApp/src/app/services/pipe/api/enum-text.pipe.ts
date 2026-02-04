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
      case 'McpToolType':
        switch (value) {
          case 0: result = 'Builtin'; break;
          case 1: result = 'External'; break;
          case 2: result = 'Custom'; break;
          default: result = '默认'; break;
        }
        break;


      default:
        break;
    }
    return result;
  }
}
