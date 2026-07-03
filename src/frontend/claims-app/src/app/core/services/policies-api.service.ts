import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PolicySearchResult, PolicyCoverageDto } from '../models/policy.model';

import { ConfigService } from './config.service';

@Injectable({
  providedIn: 'root'
})
export class PoliciesApiService {
  private get apiUrl(): string {
    return `${this.config.apiUrl}/policies`;
  }

  constructor(private http: HttpClient, private config: ConfigService) {}

  search(query: string): Observable<PolicySearchResult[]> {
    const params = new HttpParams().set('query', query);
    return this.http.get<PolicySearchResult[]>(`${this.apiUrl}/search`, { params });
  }

  getCoverage(policyId: string): Observable<PolicyCoverageDto[]> {
    return this.http.get<PolicyCoverageDto[]>(`${this.apiUrl}/${policyId}/coverage`);
  }
}
