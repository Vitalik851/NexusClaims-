import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

import { ClaimsApiService } from '../../core/services/claims-api.service';
import { ClaimDetail } from '../../core/models/claim.model';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

// Import sub tabs
import { OverviewTabComponent } from './tabs/overview-tab/overview-tab.component';
import { PartiesTabComponent } from './tabs/parties-tab/parties-tab.component';
import { ReservesTabComponent } from './tabs/reserves-tab/reserves-tab.component';
import { DocumentsTabComponent } from './tabs/documents-tab/documents-tab.component';
import { AuditLogTabComponent } from './tabs/audit-log-tab/audit-log-tab.component';

@Component({
  selector: 'app-claim-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatTabsModule,
    MatIconModule,
    MatButtonModule,
    StatusBadgeComponent,
    LoadingSpinnerComponent,
    OverviewTabComponent,
    PartiesTabComponent,
    ReservesTabComponent,
    DocumentsTabComponent,
    AuditLogTabComponent
  ],
  templateUrl: './claim-detail.component.html',
  styleUrls: ['./claim-detail.component.scss']
})
export class ClaimDetailComponent implements OnInit {
  claimId!: string;
  claim: ClaimDetail | null = null;
  loading = true;

  constructor(
    private route: ActivatedRoute,
    private claimsApi: ClaimsApiService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.claimId = params.get('id') || '';
      if (this.claimId) {
        this.loadClaim();
      }
    });
  }

  loadClaim(): void {
    this.loading = true;
    this.claimsApi.getById(this.claimId).subscribe({
      next: (data) => {
        this.claim = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load claim detail', err);
        this.loading = false;
      }
    });
  }

  reloadClaim(): void {
    this.loadClaim();
  }

  getHandlerName(handlerId: string | null): string {
    if (!handlerId) return 'Unassigned';
    // Match simulated users in mock auth
    if (handlerId.startsWith('a1')) return 'John Handler';
    if (handlerId.startsWith('b2')) return 'Sarah Supervisor';
    if (handlerId.startsWith('c3')) return 'Michael Manager';
    return `System (ID: ${handlerId.substring(0, 8)})`;
  }
}
