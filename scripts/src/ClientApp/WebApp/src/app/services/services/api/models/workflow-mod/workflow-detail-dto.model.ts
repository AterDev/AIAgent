/**
 * 工作流 DetailDto
 */
export interface WorkflowDetailDto {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** tenantId */
  tenantId: string;
  /** name */
  name?: string | null;
  /** description */
  description?: string | null;
  /** definitionJson */
  definitionJson?: string | null;
  /** version */
  version: number;
  /** isPublished */
  isPublished: boolean;
}
