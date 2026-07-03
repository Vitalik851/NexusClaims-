import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CauseOfLossCodeDto {
  code: string;
  name: string;
  perilCategory: string;
}

export interface ClaimStatusTransitionDto {
  fromStatus: string;
  toStatus: string;
  requiredPermission: string | null;
}

import { ConfigService } from './config.service';

@Injectable({
  providedIn: 'root'
})
export class ReferenceDataApiService {
  private get apiUrl(): string {
    return `${this.config.apiUrl}/referencedata`;
  }

  constructor(private http: HttpClient, private config: ConfigService) {}

  getCauseOfLossCodes(perilCategory?: string): Observable<CauseOfLossCodeDto[]> {
    let params = new HttpParams();
    if (perilCategory) {
      params = params.set('perilCategory', perilCategory);
    }
    return this.http.get<CauseOfLossCodeDto[]>(`${this.apiUrl}/cause-of-loss-codes`, { params });
  }

  getStatusTransitions(): Observable<ClaimStatusTransitionDto[]> {
    return this.http.get<ClaimStatusTransitionDto[]>(`${this.apiUrl}/status-transitions`);
  }
}
