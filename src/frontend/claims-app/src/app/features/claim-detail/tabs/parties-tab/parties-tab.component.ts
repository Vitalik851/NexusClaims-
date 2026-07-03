import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { ClaimDetail, ClaimPartyDto } from '../../../../core/models/claim.model';
import { ClaimsApiService } from '../../../../core/services/claims-api.service';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-parties-tab',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule
  ],
  template: `
    <div class="parties-grid">
      <!-- Left side: List of active parties -->
      <div class="list-pane">
        <mat-card class="inner-card">
          <mat-card-content class="p-0">
            <h3 class="pane-title">Active Claim Parties</h3>
            
            <table mat-table [dataSource]="activeParties" class="w-100">
              <!-- Name Column -->
              <ng-container matColumnDef="name">
                <th mat-header-cell *matHeaderCellDef>Name / Company</th>
                <td mat-cell *matCellDef="let p" class="name-cell">
                  {{ p.partyType === 'Person' ? p.firstName + ' ' + p.lastName : p.companyName }}
                </td>
              </ng-container>

              <!-- Role Column -->
              <ng-container matColumnDef="role">
                <th mat-header-cell *matHeaderCellDef>Role</th>
                <td mat-cell *matCellDef="let p">
                  <span class="role-chip" [ngClass]="p.partyRole.toLowerCase()">{{ p.partyRole }}</span>
                </td>
              </ng-container>

              <!-- Contact Column -->
              <ng-container matColumnDef="contact">
                <th mat-header-cell *matHeaderCellDef>Contact Details</th>
                <td mat-cell *matCellDef="let p" class="contact-details">
                  <div>{{ p.email || '—' }}</div>
                  <div class="phone">{{ p.phone || '—' }}</div>
                </td>
              </ng-container>

              <!-- Actions Column -->
              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef></th>
                <td mat-cell *matCellDef="let p" align="end">
                  <button mat-icon-button color="warn" (click)="onRemoveParty(p)" matTooltip="Remove Party">
                    <mat-icon>delete</mat-icon>
                  </button>
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
            </table>

            @if (activeParties.length === 0) {
              <div class="empty-list-state">
                <mat-icon>people_outline</mat-icon>
                <p>No active parties assigned to this claim.</p>
              </div>
            }
          </mat-card-content>
        </mat-card>
      </div>

      <!-- Right side: Add new party form -->
      <div class="add-pane">
        <mat-card class="inner-card">
          <mat-card-content>
            <h3 class="pane-title">Add Claim Party</h3>
            
            <form [formGroup]="partyForm" (ngSubmit)="onAddParty()">
              <div class="form-row">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>Role</mat-label>
                  <mat-select formControlName="partyRole">
                    <mat-option value="Claimant">Claimant</mat-option>
                    <mat-option value="Insured">Insured</mat-option>
                    <mat-option value="ThirdParty">Third Party</mat-option>
                    <mat-option value="Witness">Witness</mat-option>
                    <mat-option value="Attorney">Attorney</mat-option>
                  </mat-select>
                </mat-form-field>
              </div>

              <div class="form-row">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>Type</mat-label>
                  <mat-select formControlName="partyType">
                    <mat-option value="Person">Person</mat-option>
                    <mat-option value="Company">Company</mat-option>
                  </mat-select>
                </mat-form-field>
              </div>

              @if (partyForm.get('partyType')?.value === 'Person') {
                <div class="form-row animate-fade">
                  <mat-form-field appearance="outline" class="w-100">
                    <mat-label>First Name</mat-label>
                    <input matInput formControlName="firstName">
                  </mat-form-field>
                </div>
                <div class="form-row animate-fade">
                  <mat-form-field appearance="outline" class="w-100">
                    <mat-label>Last Name</mat-label>
                    <input matInput formControlName="lastName">
                  </mat-form-field>
                </div>
              } @else {
                <div class="form-row animate-fade">
                  <mat-form-field appearance="outline" class="w-100">
                    <mat-label>Company Name</mat-label>
                    <input matInput formControlName="companyName">
                  </mat-form-field>
                </div>
              }

              <div class="form-row">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>Email</mat-label>
                  <input matInput type="email" formControlName="email">
                </mat-form-field>
              </div>

              <div class="form-row">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>Phone</mat-label>
                  <input matInput formControlName="phone">
                </mat-form-field>
              </div>

              <div class="form-row">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>Notes</mat-label>
                  <input matInput formControlName="notes">
                </mat-form-field>
              </div>

              <button mat-raised-button color="accent" class="w-100 submit-btn" [disabled]="partyForm.invalid">
                <mat-icon>add</mat-icon> Add Active Party
              </button>
            </form>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .parties-grid {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: 25px;
    }
    
    @media (max-width: 900px) {
      .parties-grid {
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

    table {
      background: transparent;
      
      th {
        color: var(--text-muted);
        font-weight: 600;
        font-size: 0.75rem;
        text-transform: uppercase;
        border-bottom: 1px solid var(--border-color);
        padding: 12px 20px;
      }

      td {
        border-bottom: 1px solid var(--border-color);
        padding: 12px 20px;
        color: var(--text-primary);
        font-size: 0.9rem;
      }
    }

    .name-cell {
      font-weight: 600;
    }

    .role-chip {
      padding: 3px 8px;
      border-radius: 10px;
      font-size: 0.72rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.3px;
      display: inline-block;

      &.claimant {
        background-color: rgba(52, 199, 89, 0.15);
        color: #34c759;
      }
      &.insured {
        background-color: rgba(0, 122, 255, 0.15);
        color: #007aff;
      }
      &.witness {
        background-color: rgba(255, 149, 0, 0.15);
        color: #ff9500;
      }
      &.attorney {
        background-color: rgba(175, 82, 222, 0.15);
        color: #af52de;
      }
      &.thirdparty {
        background-color: rgba(142, 142, 147, 0.15);
        color: #aeaeae;
      }
    }

    .contact-details {
      font-size: 0.82rem;
      color: var(--text-secondary);
      
      .phone {
        margin-top: 3px;
        color: var(--text-muted);
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

    .submit-btn {
      height: 48px;
      font-weight: 600;
      margin-top: 15px;
    }

    .animate-fade {
      animation: fadeIn 0.25s ease-out;
    }

    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(5px); }
      to { opacity: 1; transform: translateY(0); }
    }
  `]
})
export class PartiesTabComponent implements OnInit {
  @Input() claim!: ClaimDetail;
  @Output() partiesChanged = new EventEmitter<void>();

  displayedColumns: string[] = ['name', 'role', 'contact', 'actions'];
  partyForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private claimsApi: ClaimsApiService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.initForm();
  }

  get activeParties(): ClaimPartyDto[] {
    return this.claim.parties.filter(p => p.isActive);
  }

  initForm(): void {
    this.partyForm = this.fb.group({
      partyRole: ['Claimant', Validators.required],
      partyType: ['Person', Validators.required],
      firstName: [''],
      lastName: [''],
      companyName: [''],
      email: ['', [Validators.email]],
      phone: [''],
      notes: ['']
    });

    this.partyForm.get('partyType')?.valueChanges.subscribe(type => {
      const firstName = this.partyForm.get('firstName');
      const lastName = this.partyForm.get('lastName');
      const companyName = this.partyForm.get('companyName');

      if (type === 'Person') {
        firstName?.setValidators([Validators.required]);
        lastName?.setValidators([Validators.required]);
        companyName?.clearValidators();
      } else {
        companyName?.setValidators([Validators.required]);
        firstName?.clearValidators();
        lastName?.clearValidators();
      }

      firstName?.updateValueAndValidity();
      lastName?.updateValueAndValidity();
      companyName?.updateValueAndValidity();
    });

    this.partyForm.get('partyType')?.setValue('Person');
  }

  onAddParty(): void {
    if (this.partyForm.invalid) return;

    const val = this.partyForm.value;
    const request = {
      partyRole: val.partyRole,
      partyType: val.partyType,
      firstName: val.partyType === 'Person' ? val.firstName : null,
      lastName: val.partyType === 'Person' ? val.lastName : null,
      companyName: val.partyType === 'Company' ? val.companyName : null,
      email: val.email || null,
      phone: val.phone || null,
      notes: val.notes || null
    };

    this.claimsApi.addParty(this.claim.id, request).subscribe({
      next: () => {
        this.snackBar.open('Party successfully added to the claim.', 'Close', { duration: 4000 });
        this.partyForm.reset({
          partyRole: 'Claimant',
          partyType: 'Person',
          firstName: '',
          lastName: '',
          companyName: '',
          email: '',
          phone: '',
          notes: ''
        });
        this.partiesChanged.emit();
      },
      error: (err) => {
        console.error('Failed to add party', err);
        const errMsg = err.error?.message || 'Intake validation failed.';
        this.snackBar.open(`Error: ${errMsg}`, 'Close', { duration: 5000 });
      }
    });
  }

  onRemoveParty(party: ClaimPartyDto): void {
    // Confirm dialog
    const name = party.partyType === 'Person' ? `${party.firstName} ${party.lastName}` : party.companyName;
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Remove Claim Party',
        message: `Are you sure you want to remove ${name} (${party.partyRole}) from the claim?`,
        confirmText: 'Remove',
        isDestructive: true
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.claimsApi.removeParty(this.claim.id, party.id).subscribe({
          next: () => {
            this.snackBar.open('Party successfully removed from claim.', 'Close', { duration: 4000 });
            this.partiesChanged.emit();
          },
          error: (err) => {
            console.error('Failed to remove party', err);
            // Will trigger if rule BR-C-03 / BR-ST-02 is violated (e.g. deleting the last Claimant)
            const errMsg = err.error?.message || 'Action blocked. Claim must have at least one active Claimant.';
            this.snackBar.open(`Delete Blocked: ${errMsg}`, 'Close', { duration: 6000 });
          }
        });
      }
    });
  }
}
