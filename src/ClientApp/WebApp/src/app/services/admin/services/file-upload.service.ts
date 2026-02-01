import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { UploadResult } from '../models/share/upload-result.model';
/**
 * 文件上传管理
 */
@Injectable({ providedIn: 'root' })
export class FileUploadService extends BaseService {
  /**
   * 上传文件到 S3 对象存储
   * @param data any
   */
  uploadFile(data: any): Observable<UploadResult> {
    const _url = `/api/FileUpload/upload`;
    return this.request<UploadResult>('post', _url, data);
  }
  /**
   * 删除 S3 中的文件
   * @param objectKey string
   */
  deleteFile(objectKey: string | null): Observable<any> {
    const _url = `/api/FileUpload/delete?objectKey=${objectKey ?? ''}`;
    return this.request<any>('delete', _url);
  }
}