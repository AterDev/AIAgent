/**
 * 工作流 ItemDto
 */
export interface WorkflowItemDto {
  /** id */
  id: string;
  /** name */
  name?: string | null;
  /** version */
  version: number;
  /** isPublished */
  isPublished: boolean;
}
