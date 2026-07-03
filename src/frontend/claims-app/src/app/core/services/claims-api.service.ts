import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ClaimSummary, ClaimDetail, CreateClaimRequest, TransitionStatusRequest, ClaimDocumentDto, ClaimPartyDto } from '../models/claim.model';
import { AuditLogEntry } from '../models/audit-log.model';

export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

import { ConfigService } from './config.service';

@Injectable({
  providedIn: 'root'
})
export class ClaimsApiService {
  private get apiUrl(): string {
    return `${this.config.apiUrl}/claims`;
  }

  constructor(private http: HttpClient, private config: ConfigService) {}

  getAll(filters: {
    status?: string;
    dateFrom?: string;
    dateTo?: string;
    assignedHandlerId?: string;
    causeOfLossCode?: string;
    policyId?: string;
    search?: string;
    pageNumber: number;
    pageSize: number;
  }): Observable<PaginatedResponse<ClaimSummary>> {
    let params = new HttpParams();
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, value.toString());
      }
    });

    return this.http.get<PaginatedResponse<ClaimSummary>>(this.apiUrl, { params });
  }

  getById(id: string): Observable<ClaimDetail> {
    return this.http.get<ClaimDetail>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateClaimRequest): Observable<any> {
    return this.http.post<any>(this.apiUrl, request);
  }

  transitionStatus(id: string, targetStatus: string, reason: string | null): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/status`, { targetStatus, reason });
  }

  addParty(id: string, party: Omit<ClaimPartyDto, 'id' | 'isActive'> & { partyRole: string, partyType: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/parties`, party);
  }

  removeParty(id: string, partyId: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}/parties/${partyId}`);
  }

  getAuditLog(id: string, pageNumber: number = 1, pageSize: number = 20): Observable<PaginatedResponse<AuditLogEntry>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<AuditLogEntry>>(`${this.apiUrl}/${id}/audit`, { params });
  }

  uploadDocument(id: string, documentType: string, file: File, notes?: string): Observable<ClaimDocumentDto> {
    const formData = new FormData();
    formData.append('documentType', documentType);
    formData.append('file', file);
    if (notes) {
      formData.append('notes', notes);
    }

    return this.http.post<ClaimDocumentDto>(`${this.apiUrl}/${id}/documents`, formData);
  }
}
