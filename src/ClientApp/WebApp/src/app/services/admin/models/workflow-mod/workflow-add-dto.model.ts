/**
 * 工作流 AddDto
 */
export interface WorkflowAddDto {
  /** name */
  name: string;
  /** description */
  description?: string | null;
  /** definitionJson */
  definitionJson: string;
  /** version */
  version: number;
  /** isPublished */
  isPublished: boolean;
}
