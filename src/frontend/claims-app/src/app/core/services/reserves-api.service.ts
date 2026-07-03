import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ReserveHistoryDto, ReserveSummaryDto } from '../models/reserve.model';

import { ConfigService } from './config.service';

@Injectable({
  providedIn: 'root'
})
export class ReservesApiService {
  private get apiUrl(): string {
    return `${this.config.apiUrl}/reserves`;
  }

  constructor(private http: HttpClient, private config: ConfigService) {}

  getByClaimId(claimId: string): Observable<ReserveSummaryDto> {
    return this.http.get<ReserveSummaryDto>(`${this.apiUrl}/claim/${claimId}`);
  }

  create(request: {
    claimId: string;
    component: string;
    amount: number;
    transactionType: string;
    changeReason: string;
  }): Observable<any> {
    return this.http.post<any>(this.apiUrl, request);
  }

  approve(historyId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${historyId}/approve`, {});
  }

  reject(historyId: string, rejectionReason: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${historyId}/reject`, { rejectionReason });
  }

  retract(historyId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${historyId}/retract`, {});
  }
}
