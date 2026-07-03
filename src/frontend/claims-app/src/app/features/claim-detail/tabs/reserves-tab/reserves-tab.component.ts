import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { ClaimDetail, ReserveComponentSummary } from '../../../../core/models/claim.model';
import { ReservesApiService } from '../../../../core/services/reserves-api.service';
import { ReserveHistoryDto, ReserveSummaryDto } from '../../../../core/models/reserve.model';
import { AuthService } from '../../../../core/auth/auth.service';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-reserves-tab',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule
  ],
  template: `
    <!-- Top summary cards -->
    <div class="summary-cards animate-fade">
      <mat-card class="total-card">
        <mat-card-content>
          <span class="lbl">Outstanding Reserve Balance</span>
          <span class="val">{{ totalApprovedBalance | currency:'USD':'symbol':'1.2-2' }}</span>
        </mat-card-content>
      </mat-card>

      @for (comp of claim.reserveComponents; track comp.id) {
        <mat-card class="component-card">
          <mat-card-content>
            <span class="lbl">{{ comp.reserveType }}</span>
            <span class="val">{{ comp.currentAmount | currency:'USD':'symbol':'1.2-2' }}</span>
            <span class="status-indicator active">Active Line</span>
          </mat-card-content>
        </mat-card>
      }
    </div>

    <div class="reserves-grid mt-4">
      <!-- Left side: History of reserve changes -->
      <div class="history-pane">
        <mat-card class="inner-card">
          <mat-card-content class="p-0">
            <h3 class="pane-title">Reserve Transactions & Audit History</h3>
            
            <table mat-table [dataSource]="historyRecords" class="w-100">
              <!-- Created Date -->
              <ng-container matColumnDef="date">
                <th mat-header-cell *matHeaderCellDef>Date</th>
                <td mat-cell *matCellDef="let h">
                  {{ h.createdAt | date:'short' }}
                </td>
              </ng-container>

              <!-- Component & Type -->
              <ng-container matColumnDef="details">
                <th mat-header-cell *matHeaderCellDef>Component / Type</th>
                <td mat-cell *matCellDef="let h" class="details-cell">
                  <strong>{{ getComponentForHistory(h) }}</strong>
                  <div class="txn-type-desc">{{ h.transactionType }}</div>
                </td>
              </ng-container>

              <!-- Amount Column -->
              <ng-container matColumnDef="amount">
                <th mat-header-cell *matHeaderCellDef>Change Amount</th>
                <td mat-cell *matCellDef="let h" class="amount-cell" [ngClass]="getAmountClass(h)">
                  {{ getFormattedAmount(h) }}
                </td>
              </ng-container>

              <!-- Approval Status Column -->
              <ng-container matColumnDef="status">
                <th mat-header-cell *matHeaderCellDef>Workflow Status</th>
                <td mat-cell *matCellDef="let h">
                  <span class="workflow-chip" [ngClass]="h.approvalStatus.toLowerCase()">
                    {{ h.approvalStatus }}
                  </span>
                  @if (h.approvalStatus === 'PendingApproval') {
                    <div class="req-approver">Needs Approval</div>
                  }
                </td>
              </ng-container>

              <!-- Ledger Posting Column -->
              <ng-container matColumnDef="ledger">
                <th mat-header-cell *matHeaderCellDef>Ledger Status</th>
                <td mat-cell *matCellDef="let h">
                  <span class="ledger-chip" [ngClass]="h.postingStatus.toLowerCase()">
                    <mat-icon *if (h.postingStatus === 'Posted')>cloud_done</mat-icon>
                    {{ h.postingStatus }}
                  </span>
                  @if (h.postingJobId) {
                    <div class="posting-job-ref">Ref: {{ h.postingJobId.substring(0, 8) }}</div>
                  }
                </td>
              </ng-container>

              <!-- Action buttons (Approve/Reject/Retract) -->
              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef></th>
                <td mat-cell *matCellDef="let h" align="end">
                  <!-- Retract option for owner -->
                  @if (h.approvalStatus === 'PendingApproval' && h.submittedByUserId === currentUserId) {
                    <button mat-stroked-button color="accent" size="small" (click)="onRetract(h)">
                      Retract
                    </button>
                  }
                  
                  <!-- Approval options for supervisor/manager -->
                  @if (h.approvalStatus === 'PendingApproval' && canApprove(h)) {
                    <div class="workflow-actions-group">
                      <button mat-flat-button color="primary" class="approve-btn" (click)="onApprove(h)">
                        Approve
                      </button>
                      <button mat-stroked-button color="warn" class="reject-btn" (click)="onReject(h)">
                        Reject
                      </button>
                    </div>
                  }
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;" class="txn-row"></tr>
            </table>

            @if (historyRecords.length === 0) {
              <div class="empty-list-state">
                <mat-icon>history_toggle_off</mat-icon>
                <p>No reserve transactions recorded yet.</p>
              </div>
            }
          </mat-card-content>
        </mat-card>
      </div>

      <!-- Right side: Add new reserve adjustment form -->
      <div class="adjust-pane">
        <mat-card class="inner-card">
          <mat-card-content>
            <h3 class="pane-title">Create Reserve Adjustment</h3>
            
            <form [formGroup]="adjustForm" (ngSubmit)="onCreateAdjustment()">
              <div class="form-row">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>Component</mat-label>
                  <mat-select formControlName="component">
                    <mat-option value="Indemnity">Indemnity</mat-option>
                    <mat-option value="Expense">Expense</mat-option>
                    <mat-option value="ALAE">ALAE</mat-option>
                    <mat-option value="SubrogationRecoverable">Subrogation Recoverable</mat-option>
                  </mat-select>
                </mat-form-field>
              </div>

              <div class="form-row">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>Transaction Type</mat-label>
                  <mat-select formControlName="transactionType">
                    <mat-option value="Add">Add (Increase balance)</mat-option>
                    <mat-option value="Adjust">Adjust (Write exact balance)</mat-option>
                    <mat-option value="Reverse">Reverse (Reduce/Cancel balance)</mat-option>
                  </mat-select>
                </mat-form-field>
              </div>

              <div class="form-row">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>Adjustment Amount ($)</mat-label>
                  <input matInput type="number" formControlName="amount">
                  @if (adjustForm.get('amount')?.hasError('min')) {
                    <mat-error>Amount must be greater than 0.</mat-error>
                  }
                </mat-form-field>
              </div>

              <div class="form-row">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>Adjustment Reason</mat-label>
                  <textarea matInput formControlName="changeReason" rows="3" placeholder="Define the business driver for this reserve change..."></textarea>
                  @if (adjustForm.get('changeReason')?.hasError('minlength')) {
                    <mat-error>Reason must be at least 10 characters long.</mat-error>
                  }
                </mat-form-field>
              </div>

              <!-- Authority check hint -->
              <div class="authority-badge" [ngClass]="getAuthorityLevelClass()">
                <mat-icon>assignment_turned_in</mat-icon>
                <span>Estimated Level: <strong>{{ getEstimatedAuthorityLevel() }}</strong></span>
              </div>

              <button mat-raised-button color="accent" class="w-100 submit-btn" [disabled]="adjustForm.invalid">
                <mat-icon>save</mat-icon> Submit Adjustment
              </button>
            </form>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .summary-cards {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 15px;
    }

    .total-card {
      background: linear-gradient(135deg, rgba(26, 35, 126, 0.4) 0%, rgba(25, 118, 210, 0.2) 100%) !important;
      border: 1px solid rgba(25, 118, 210, 0.4) !important;
      
      .val {
        color: var(--accent-color) !important;
        font-size: 1.8rem !important;
      }
    }

    .component-card, .total-card {
      background: rgba(255, 255, 255, 0.015);
      border: 1px solid var(--border-color);
      border-radius: 8px;
      
      .lbl {
        display: block;
        font-size: 0.72rem;
        font-weight: 600;
        color: var(--text-secondary);
        text-transform: uppercase;
        letter-spacing: 0.8px;
      }

      .val {
        display: block;
        font-size: 1.5rem;
        font-weight: 700;
        margin: 8px 0;
        color: var(--text-primary);
        font-family: 'Roboto Mono', monospace;
      }

      .status-indicator {
        font-size: 0.7rem;
        font-weight: 600;
        text-transform: uppercase;
        color: #34c759;
      }
    }

    .reserves-grid {
      display: grid;
      grid-template-columns: 2.2fr 1fr;
      gap: 25px;
    }

    @media (max-width: 1000px) {
      .reserves-grid {
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

    .p-0 .pane-title {
      padding-left: 20px;
      padding-right: 20px;
    }

    .w-100 { width: 100%; }
    .mt-4 { margin-top: 24px; }

    table {
      background: transparent;
      
      th {
        color: var(--text-muted);
        font-weight: 600;
        font-size: 0.75rem;
        text-transform: uppercase;
        border-bottom: 1px solid var(--border-color);
        padding: 14px 16px;
      }

      td {
        border-bottom: 1px solid var(--border-color);
        padding: 14px 16px;
        color: var(--text-primary);
        font-size: 0.88rem;
        vertical-align: middle;
      }
    }

    .details-cell {
      strong {
        color: var(--text-primary);
        font-size: 0.9rem;
      }
      .txn-type-desc {
        font-size: 0.75rem;
        color: var(--text-secondary);
        margin-top: 2px;
      }
    }

    .amount-cell {
      font-family: 'Roboto Mono', monospace;
      font-weight: 600;
      
      &.amount-add { color: #34c759; }
      &.amount-reverse { color: #ff3b30; }
      &.amount-adjust { color: #007aff; }
    }

    .workflow-chip {
      padding: 3px 8px;
      border-radius: 10px;
      font-size: 0.7rem;
      font-weight: 600;
      text-transform: uppercase;
      display: inline-block;

      &.autoapproved, &.approved {
        background-color: rgba(52, 199, 89, 0.12);
        color: #34c759;
      }
      &.pendingapproval {
        background-color: rgba(255, 149, 0, 0.12);
        color: #ff9500;
      }
      &.rejected {
        background-color: rgba(255, 59, 48, 0.12);
        color: #ff3b30;
      }
      &.cancelled {
        background-color: rgba(142, 142, 147, 0.15);
        color: #8e8e93;
      }
    }

    .req-approver {
      font-size: 0.7rem;
      color: var(--accent-color);
      font-weight: 500;
      margin-top: 3px;
    }

    .ledger-chip {
      padding: 3px 8px;
      border-radius: 10px;
      font-size: 0.7rem;
      font-weight: 600;
      text-transform: uppercase;
      display: inline-flex;
      align-items: center;
      gap: 4px;

      mat-icon {
        font-size: 10px;
        width: 10px;
        height: 10px;
      }

      &.posted {
        background-color: rgba(52, 199, 89, 0.12);
        color: #34c759;
      }
      &.pending {
        background-color: rgba(0, 122, 255, 0.12);
        color: #007aff;
      }
      &.failed {
        background-color: rgba(255, 59, 48, 0.12);
        color: #ff3b30;
      }
      &.cancelled {
        background-color: rgba(142, 142, 147, 0.15);
        color: #8e8e93;
      }
    }

    .posting-job-ref {
      font-size: 0.68rem;
      color: var(--text-muted);
      margin-top: 3px;
      font-family: 'Roboto Mono', monospace;
    }

    .workflow-actions-group {
      display: flex;
      gap: 8px;

      button {
        height: 28px;
        line-height: 28px;
        padding: 0 10px;
        font-size: 0.78rem;
        font-weight: 600;
        border-radius: 4px;
      }

      .approve-btn {
        background-color: #34c759 !important;
        color: white;
      }
    }

    .empty-list-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 40px;
      color: var(--text-muted);

      mat-icon {
        font-size: 36px;
        width: 36px;
        height: 36px;
        margin-bottom: 10px;
      }

      p {
        margin: 0;
        font-size: 0.9rem;
      }
    }

    .form-row {
      margin-bottom: 8px;
    }

    .authority-badge {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 10px 12px;
      border-radius: 6px;
      margin-bottom: 15px;
      font-size: 0.8rem;
      
      mat-icon {
        font-size: 16px;
        width: 16px;
        height: 16px;
      }

      &.auth-auto {
        background-color: rgba(52, 199, 89, 0.1);
        color: #34c759;
        border: 1px solid rgba(52, 199, 89, 0.2);
      }
      &.auth-supervisor {
        background-color: rgba(255, 149, 0, 0.1);
        color: #ff9500;
        border: 1px solid rgba(255, 149, 0, 0.2);
      }
      &.auth-manager {
        background-color: rgba(255, 59, 48, 0.1);
        color: #ff3b30;
        border: 1px solid rgba(255, 59, 48, 0.2);
      }
    }

    .submit-btn {
      height: 48px;
      font-weight: 600;
    }
  `]
})
export class ReservesTabComponent implements OnInit {
  @Input() claim!: ClaimDetail;
  @Output() reservesChanged = new EventEmitter<void>();

  displayedColumns: string[] = ['date', 'details', 'amount', 'status', 'ledger', 'actions'];
  historyRecords: ReserveHistoryDto[] = [];
  adjustForm!: FormGroup;
  currentUserId!: string;
  userRole = 'handler';

  constructor(
    private fb: FormBuilder,
    private reservesApi: ReservesApiService,
    private auth: AuthService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.auth.currentUser?.id || '';
    this.userRole = this.auth.currentUser?.role || 'handler';
    this.initForm();
    this.loadHistory();
  }

  get totalApprovedBalance(): number {
    return this.claim.reserveComponents
      .reduce((sum, item) => sum + item.currentAmount, 0);
  }

  initForm(): void {
    this.adjustForm = this.fb.group({
      component: ['Indemnity', Validators.required],
      transactionType: ['Add', Validators.required],
      amount: [0, [Validators.required, Validators.min(0.01)]],
      changeReason: ['', [Validators.required, Validators.minLength(10)]]
    });
  }

  loadHistory(): void {
    this.reservesApi.getByClaimId(this.claim.id).subscribe({
      next: (summary) => {
        this.historyRecords = summary.history;
      },
      error: (err) => console.error('Failed to load transaction history', err)
    });
  }

  getComponentForHistory(history: ReserveHistoryDto): string {
    // Attempt to map from componentId to type name by looking at claim reserve lines
    const comp = this.claim.reserveComponents.find(c => c.id === history.reserveComponentId);
    return comp ? comp.reserveType : 'Unknown Line';
  }

  getAmountClass(h: ReserveHistoryDto): string {
    if (h.transactionType === 'Add') return 'amount-add';
    if (h.transactionType === 'Reverse') return 'amount-reverse';
    return 'amount-adjust';
  }

  getFormattedAmount(h: ReserveHistoryDto): string {
    const sign = h.transactionType === 'Reverse' ? '-' : '+';
    return `${sign}${h.amount.toLocaleString('en-US', { style: 'currency', currency: 'USD' })}`;
  }

  getEstimatedAuthorityLevel(): string {
    const amt = this.adjustForm.get('amount')?.value || 0;
    if (amt <= 10000) return 'Auto-Approved';
    if (amt <= 100000) return 'Supervisor Required';
    return 'Manager Required';
  }

  getAuthorityLevelClass(): string {
    const amt = this.adjustForm.get('amount')?.value || 0;
    if (amt <= 10000) return 'auth-auto';
    if (amt <= 100000) return 'auth-supervisor';
    return 'auth-manager';
  }

  onCreateAdjustment(): void {
    if (this.adjustForm.invalid) return;

    const val = this.adjustForm.value;
    const request = {
      claimId: this.claim.id,
      component: val.component,
      amount: val.amount,
      transactionType: val.transactionType,
      changeReason: val.changeReason
    };

    this.reservesApi.create(request).subscribe({
      next: (res) => {
        const isAuto = res.value?.approvalStatus === 'AutoApproved';
        const msg = isAuto 
          ? 'Reserve adjustment successfully auto-approved!' 
          : 'Adjustment submitted. Reserve is pending supervisor approval.';
        
        this.snackBar.open(msg, 'Close', { duration: 5000 });
        this.adjustForm.reset({
          component: 'Indemnity',
          transactionType: 'Add',
          amount: 0,
          changeReason: ''
        });
        
        this.loadHistory();
        this.reservesChanged.emit();
      },
      error: (err) => {
        console.error('Failed to adjust reserve', err);
        const errMsg = err.error?.message || 'Aggregated limits check failed ($10M aggregate rule BR-R-05).';
        this.snackBar.open(`Error: ${errMsg}`, 'Close', { duration: 6000 });
      }
    });
  }

  canApprove(h: ReserveHistoryDto): boolean {
    // Validate self-approval block (BR-R-03): submitter cannot be approver
    if (h.submittedByUserId === this.currentUserId) return false;

    // Validate authority tier level (BR-R-02)
    if (h.amount > 100000) {
      return this.userRole === 'manager'; // Only Manager can approve > $100K
    }
    if (h.amount > 10000) {
      return this.userRole === 'supervisor' || this.userRole === 'manager'; // Supervisor/Manager approve > $10K
    }
    return true; // Under $10K (though it should auto-approve, fallback)
  }

  onApprove(h: ReserveHistoryDto): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Approve Reserve Change',
        message: `Confirm approval of reserve adjustment for ${this.getComponentForHistory(h)} in amount of ${h.amount.toLocaleString('en-US', {style:'currency', currency:'USD'})}?`
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.reservesApi.approve(h.reserveHistoryId).subscribe({
          next: () => {
            this.snackBar.open('Reserve adjustment successfully approved.', 'Close', { duration: 4000 });
            this.loadHistory();
            this.reservesChanged.emit();
          },
          error: (err) => {
            console.error('Approve failed', err);
            const errMsg = err.error?.message || 'Authority check failed.';
            this.snackBar.open(`Error: ${errMsg}`, 'Close', { duration: 5000 });
          }
        });
      }
    });
  }

  onReject(h: ReserveHistoryDto): void {
    // Open prompt dialog or standard confirm to ask for reason
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Reject Reserve Change',
        message: 'Are you sure you want to reject this reserve adjustment proposal?',
        confirmText: 'Reject',
        isDestructive: true
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        // We'll pass a default rejection reason for simplicity or we could prompt
        const reason = 'Rejected by Supervisor / Manager during review.';
        this.reservesApi.reject(h.reserveHistoryId, reason).subscribe({
          next: () => {
            this.snackBar.open('Reserve proposal rejected.', 'Close', { duration: 4000 });
            this.loadHistory();
            this.reservesChanged.emit();
          },
          error: (err) => {
            console.error('Reject failed', err);
            this.snackBar.open('Rejection failed.', 'Close', { duration: 4000 });
          }
        });
      }
    });
  }

  onRetract(h: ReserveHistoryDto): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Retract Proposal',
        message: 'Are you sure you want to cancel and retract your submitted reserve adjustment?',
        confirmText: 'Retract',
        isDestructive: true
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.reservesApi.retract(h.reserveHistoryId).subscribe({
          next: () => {
            this.snackBar.open('Adjustment proposal retracted.', 'Close', { duration: 4000 });
            this.loadHistory();
          },
          error: (err) => console.error('Retract failed', err)
        });
      }
    });
  }
}
