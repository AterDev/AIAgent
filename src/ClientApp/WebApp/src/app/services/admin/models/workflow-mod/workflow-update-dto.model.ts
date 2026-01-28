/**
 * 工作流 UpdateDto
 */
export interface WorkflowUpdateDto {
  /** name */
  name?: string | null;
  /** description */
  description?: string | null;
  /** definitionJson */
  definitionJson?: string | null;
  /** version */
  version?: number | null;
  /** isPublished */
  isPublished?: boolean | null;
}
