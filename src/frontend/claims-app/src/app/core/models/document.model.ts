export interface ClaimDocument {
  id: string;
  claimId: string;
  documentType: DocumentType;
  documentName: string;
  contentType: string;
  fileSizeBytes: number;
  uploadedAt: string;
  uploadedByUserId: string | null;
  notes: string | null;
  downloadUrl: string;
}

export type DocumentType =
  | 'LossReport'
  | 'PoliceReport'
  | 'MedicalRecord'
  | 'Photo'
  | 'Invoice'
  | 'Estimate'
  | 'Correspondence'
  | 'LegalDocument'
  | 'Other';

export interface UploadDocumentRequest {
  claimId: string;
  documentType: string;
  notes: string | null;
  file: File;
}

export interface DocumentListResponse {
  items: ClaimDocument[];
  totalCount: number;
}
