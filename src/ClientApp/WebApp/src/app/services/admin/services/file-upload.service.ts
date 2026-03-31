import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { StorageType } from '../models/perigon/storage-type.model';
import { UploadResult } from '../models/share/upload-result.model';
/**
 * 文件上传管理
 */
@Injectable({ providedIn: 'root' })
export class FileUploadService extends BaseService {
  /**
   * 上传文件到存储服务商配置的存储位置
   * @param data any
   */
  uploadFile(data: any): Observable<UploadResult> {
    const _url = `/api/FileUpload/upload`;
    return this.request<UploadResult>('post', _url, data);
  }
  /**
   * 删除文件
   * @param filePath string
   * @param isCloud boolean
   */
  deleteFile(filePath: string | null, isCloud: boolean | null): Observable<any> {
    const _url = `/api/FileUpload/delete?filePath=${filePath ?? ''}&isCloud=${isCloud ?? ''}`;
    return this.request<any>('delete', _url);
  }
}