export interface ClaimSummary {
  id: string;
  claimNumber: string;
  policyNumber: string | null;
  clientName: string | null;
  lossDate: string;
  causeOfLossCode: string;
  causeOfLossName: string;
  status: ClaimStatus;
  totalReserves: number;
  assignedHandlerId: string | null;
  reportedDate: string;
}

export interface ClaimDetail {
  id: string;
  claimNumber: string;
  organizationEntityId: string;
  policyId: string | null;
  policyNumber: string | null;
  clientName: string | null;
  status: ClaimStatus;
  severity: string | null;
  reportedDate: string;
  assignedHandlerId: string | null;
  closedAt: string | null;
  closureReason: string | null;
  notes: string | null;
  managerOverrideFlag: boolean;
  lossEvent: LossEventDto;
  parties: ClaimPartyDto[];
  riskObjects: ClaimRiskObjectDto[];
  reserveComponents: ReserveComponentSummary[];
  documents: ClaimDocumentDto[];
  recentAuditEntries: AuditLogEntry[];
}

export interface LossEventDto {
  lossDate: string;
  lossDescription: string;
  lossLocation: string | null;
  causeOfLossCode: string;
  causeOfLossName: string;
  estimatedLossAmount: number | null;
  reportDate: string;
  policeReportNumber: string | null;
}

export interface ClaimPartyDto {
  id: string;
  partyRole: string;
  partyType: string;
  firstName: string | null;
  lastName: string | null;
  companyName: string | null;
  email: string | null;
  phone: string | null;
  notes: string | null;
  isActive: boolean;
}

export interface ClaimRiskObjectDto {
  id: string;
  assetType: string;
  assetDescription: string;
  damageDescription: string | null;
  isPrimary: boolean;
  assetReference: string | null;
}

export interface ClaimDocumentDto {
  id: string;
  documentType: string;
  documentName: string;
  contentType: string;
  fileSizeBytes: number;
  uploadedAt: string;
  uploadedByUserId: string | null;
  notes: string | null;
  downloadUrl: string;
}

export interface ReserveComponentSummary {
  id: string;
  reserveType: string;
  reserveTypeName: string;
  currentAmount: number;
  status: string;
  lastUpdatedAt: string;
}

export interface AuditLogEntry {
  id: string;
  action: string;
  performedByUserId: string;
  performedAt: string;
  details: string | null;
  previousValue: string | null;
  newValue: string | null;
}

export type ClaimStatus =
  | 'Draft'
  | 'Open'
  | 'UnderInvestigation'
  | 'PendingPayment'
  | 'Closed'
  | 'Reopened'
  | 'Withdrawn';

export interface CreateClaimRequest {
  organizationEntityId: string;
  policyId: string | null;
  lossDate: string;
  lossDescription: string;
  lossLocation: string | null;
  causeOfLossCode: string;
  estimatedLossAmount: number | null;
  severity: string | null;
  parties: CreatePartyRequest[];
  riskObjects: CreateRiskObjectRequest[];
  initialReserve: CreateReserveRequest | null;
}

export interface CreatePartyRequest {
  partyRole: string;
  partyType: string;
  firstName: string | null;
  lastName: string | null;
  companyName: string | null;
  email: string | null;
  phone: string | null;
  notes: string | null;
}

export interface CreateRiskObjectRequest {
  assetType: string;
  assetDescription: string;
  damageDescription: string | null;
  isPrimary: boolean;
  assetReference: string | null;
}

export interface CreateReserveRequest {
  component: string;
  amount: number;
  changeReason: string | null;
}

export interface TransitionStatusRequest {
  targetStatus: ClaimStatus;
  reason: string | null;
}

export interface ClaimsListResponse {
  items: ClaimSummary[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}
