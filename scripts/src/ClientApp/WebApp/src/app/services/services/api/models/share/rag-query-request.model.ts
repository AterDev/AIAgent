export interface RagQueryRequest {
  /** query */
  query: string;
  /** collectionId */
  collectionId?: string | null;
  /** topK */
  topK: number;
}
