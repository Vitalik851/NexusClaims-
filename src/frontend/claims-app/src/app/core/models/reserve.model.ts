export interface ReserveComponent {
  id: string;
  claimId: string;
  reserveType: string;
  reserveTypeName: string;
  currentAmount: number;
  originalAmount: number;
  status: ReserveStatus;
  approvedByUserId: string | null;
  approvedAt: string | null;
  notes: string | null;
  createdAt: string;
  lastUpdatedAt: string;
  history: ReserveHistoryEntry[];
}

export interface ReserveHistoryEntry {
  id: string;
  changeType: string;
  previousAmount: number;
  newAmount: number;
  changedByUserId: string;
  changedAt: string;
  reason: string | null;
}

export type ReserveStatus = 'Active' | 'Closed' | 'PendingApproval';

export interface CreateReserveComponentRequest {
  claimId: string;
  reserveType: string;
  initialAmount: number;
  notes: string | null;
}

export interface UpdateReserveAmountRequest {
  newAmount: number;
  reason: string;
}

export interface ReserveApprovalRequest {
  approved: boolean;
  comments: string | null;
}

export interface ReserveHistoryDto {
  reserveHistoryId: string;
  reserveComponentId: string;
  claimId: string;
  transactionType: string;
  amount: number;
  previousBalance: number;
  newBalance: number;
  approvalStatus: string;
  approvedByUserId: string | null;
  approvedAt: string | null;
  rejectedByUserId: string | null;
  rejectedAt: string | null;
  rejectionReason: string | null;
  changeReason: string | null;
  postingStatus: string;
  postingJobId: string | null;
  changeSequence: number;
  submittedByUserId: string;
  createdAt: string;
}

export interface ReserveSummaryDto {
  claimId: string;
  components: any[];
  history: ReserveHistoryDto[];
}
