import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';

import { ClaimDetail } from '../../../../core/models/claim.model';
import { ClaimsApiService } from '../../../../core/services/claims-api.service';
import { ReferenceDataApiService, ClaimStatusTransitionDto } from '../../../../core/services/reference-data-api.service';
import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-overview-tab',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatIconModule
  ],
  template: `
    <div class="overview-grid">
      <!-- Left side: Claim description details -->
      <div class="details-pane">
        <mat-card class="inner-card">
          <mat-card-content>
            <h3 class="pane-title">Loss Event Information</h3>
            
            <div class="detail-row">
              <span class="lbl">Cause of Loss</span>
              <span class="val">{{ claim.lossEvent.causeOfLossName }} ({{ claim.lossEvent.causeOfLossCode }})</span>
            </div>

            <div class="detail-row">
              <span class="lbl">Estimated Loss</span>
              <span class="valHighlight">{{ (claim.lossEvent.estimatedLossAmount | currency:'USD') || 'Not Estimated' }}</span>
            </div>

            <div class="detail-row">
              <span class="lbl">Severity Level</span>
              <span class="val">{{ claim.severity || 'Standard' }}</span>
            </div>

            <div class="detail-row">
              <span class="lbl">Loss Location</span>
              <span class="val">{{ claim.lossEvent.lossLocation || 'N/A' }}</span>
            </div>

            <div class="detail-row">
              <span class="lbl">Police Report No.</span>
              <span class="val">{{ claim.lossEvent.policeReportNumber || 'None' }}</span>
            </div>

            <div class="description-block">
              <span class="lbl">Event Description</span>
              <p class="desc-text">{{ claim.lossEvent.lossDescription }}</p>
            </div>

            @if (claim.notes) {
              <div class="description-block">
                <span class="lbl">Handler Notes</span>
                <p class="desc-text">{{ claim.notes }}</p>
              </div>
            }
          </mat-card-content>
        </mat-card>
      </div>

      <!-- Right side: Status management & Actions -->
      <div class="actions-pane">
        <mat-card class="inner-card status-mgmt-card">
          <mat-card-content>
            <h3 class="pane-title">Status Management</h3>
            
            <div class="current-status-display">
              <span class="lbl">Current State</span>
              <span class="status-name">{{ claim.status }}</span>
            </div>

            <!-- Transition selector -->
            <div class="transition-box">
              <h4>Transition Claim Status</h4>
              
              <mat-form-field appearance="outline" class="w-100">
                <mat-label>Target Status</mat-label>
                <mat-select [(ngModel)]="targetStatus">
                  @for (status of nextAvailableStatuses; track status) {
                    <mat-option [value]="status">{{ status }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-100">
                <mat-label>Reason / Audit Note</mat-label>
                <textarea matInput [(ngModel)]="transitionReason" rows="2" placeholder="e.g. Investigation completed successfully, closing claim."></textarea>
              </mat-form-field>

              <button mat-raised-button color="primary" class="w-100 transition-btn" 
                [disabled]="!targetStatus || transitionReason.length < 5" 
                (click)="onTransitionStatus()">
                <mat-icon>swap_horiz</mat-icon> Apply Status Transition
              </button>
              
              @if (targetStatus && transitionReason.length < 5) {
                <span class="validation-tip">Reason (min 5 characters) is required for audits.</span>
              }
            </div>

            <div class="security-info">
              <mat-icon>security</mat-icon>
              <span>Transitions validate active permissions for roles: <strong>{{ userRole | uppercase }}</strong>.</span>
            </div>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .overview-grid {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: 25px;
    }
    
    @media (max-width: 900px) {
      .overview-grid {
        grid-template-columns: 1fr;
      }
    }

    .inner-card {
      background: rgba(255, 255, 255, 0.015);
      border: 1px solid var(--border-color);
      border-radius: 8px;
    }

    .pane-title {
      font-size: 1.1rem;
      font-weight: 600;
      color: var(--text-primary);
      margin-top: 0;
      margin-bottom: 20px;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 10px;
    }

    .detail-row {
      display: flex;
      justify-content: space-between;
      padding: 10px 0;
      border-bottom: 1px dashed rgba(255, 255, 255, 0.05);

      .lbl {
        color: var(--text-secondary);
        font-weight: 500;
      }
      .val {
        color: var(--text-primary);
        font-weight: 600;
      }
      .valHighlight {
        color: var(--accent-color);
        font-weight: 700;
        font-family: 'Roboto Mono', monospace;
      }
    }

    .description-block {
      margin-top: 20px;
      
      .lbl {
        color: var(--text-secondary);
        font-weight: 500;
        font-size: 0.85rem;
        text-transform: uppercase;
        letter-spacing: 0.5px;
      }

      .desc-text {
        margin: 8px 0 0 0;
        color: var(--text-primary);
        font-size: 0.95rem;
        line-height: 1.5;
        background: rgba(255, 255, 255, 0.02);
        padding: 12px;
        border-radius: 6px;
        border-left: 3px solid var(--primary-color);
      }
    }

    .status-mgmt-card {
      height: 100%;
    }

    .current-status-display {
      display: flex;
      justify-content: space-between;
      align-items: center;
      background: rgba(26, 26, 36, 0.5);
      border: 1px solid var(--border-color);
      padding: 12px 15px;
      border-radius: 6px;
      margin-bottom: 20px;

      .lbl {
        font-size: 0.85rem;
        color: var(--text-muted);
        text-transform: uppercase;
        font-weight: 600;
      }

      .status-name {
        color: var(--primary-color);
        font-weight: 700;
        font-size: 1.1rem;
      }
    }

    .transition-box {
      h4 {
        margin: 0 0 12px 0;
        font-size: 0.95rem;
        font-weight: 600;
        color: var(--text-primary);
      }

      .transition-btn {
        height: 48px;
        font-weight: 600;
      }

      .validation-tip {
        display: block;
        margin-top: 8px;
        color: var(--accent-color);
        font-size: 0.78rem;
        font-weight: 500;
      }
    }

    .security-info {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-top: 20px;
      background: rgba(255, 255, 255, 0.03);
      padding: 10px;
      border-radius: 6px;
      font-size: 0.8rem;
      color: var(--text-secondary);

      mat-icon {
        font-size: 16px;
        width: 16px;
        height: 16px;
        color: var(--primary-color);
      }
    }

    .w-100 { width: 100%; }
  `]
})
export class OverviewTabComponent implements OnInit {
  @Input() claim!: ClaimDetail;
  @Output() statusChanged = new EventEmitter<void>();

  targetStatus: string = '';
  transitionReason: string = '';
  nextAvailableStatuses: string[] = [];
  userRole = 'handler';

  constructor(
    private claimsApi: ClaimsApiService,
    private refDataApi: ReferenceDataApiService,
    private auth: AuthService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.userRole = this.auth.currentUser?.role || 'handler';
    this.loadAllowedTransitions();
  }

  loadAllowedTransitions(): void {
    // Standard status lists for fallback or direct lookup
    const allStates = ['Draft', 'Open', 'UnderInvestigation', 'PendingPayment', 'Closed', 'Reopened', 'Withdrawn'];
    
    this.refDataApi.getStatusTransitions().subscribe({
      next: (transitions) => {
        // Filter allowed toStatus states based on currentStatus
        const allowed = transitions
          .filter(t => t.fromStatus === this.claim.status)
          .map(t => t.toStatus);
        
        // Failsafe: if DB seed not loaded or empty, allow all states except current
        this.nextAvailableStatuses = allowed.length > 0 
          ? allowed 
          : allStates.filter(s => s !== this.claim.status);
      },
      error: () => {
        this.nextAvailableStatuses = allStates.filter(s => s !== this.claim.status);
      }
    });
  }

  onTransitionStatus(): void {
    if (!this.targetStatus) return;

    this.claimsApi.transitionStatus(this.claim.id, this.targetStatus, this.transitionReason).subscribe({
      next: () => {
        this.snackBar.open(`Claim status changed to ${this.targetStatus}`, 'Close', { duration: 4000 });
        this.targetStatus = '';
        this.transitionReason = '';
        this.statusChanged.emit();
      },
      error: (err) => {
        console.error('Failed to transition status', err);
        const errMsg = err.error?.message || 'Unauthorized status transition. Transition blocked.';
        this.snackBar.open(`Transition Error: ${errMsg}`, 'Close', { duration: 6000 });
      }
    });
  }
}
