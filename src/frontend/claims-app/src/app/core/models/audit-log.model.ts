export interface AuditLogEntry {
  id: string;
  claimId: string;
  action: AuditAction;
  performedByUserId: string;
  performedByUserName: string | null;
  performedAt: string;
  entityType: string;
  entityId: string;
  details: string | null;
  previousValue: string | null;
  newValue: string | null;
}

export type AuditAction =
  | 'Created'
  | 'Updated'
  | 'Deleted'
  | 'StatusChanged'
  | 'ReserveCreated'
  | 'ReserveUpdated'
  | 'ReserveApproved'
  | 'DocumentUploaded'
  | 'DocumentDeleted'
  | 'PartyAdded'
  | 'PartyUpdated'
  | 'NoteAdded'
  | 'Assigned'
  | 'Reassigned';

export interface AuditLogResponse {
  items: AuditLogEntry[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface AuditLogQuery {
  claimId: string;
  action: string | null;
  fromDate: string | null;
  toDate: string | null;
  pageNumber: number;
  pageSize: number;
}
