import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatIconModule } from '@angular/material/icon';

import { ClaimsApiService } from '../../../../core/services/claims-api.service';
import { AuditLogEntry } from '../../../../core/models/audit-log.model';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-audit-log-tab',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatPaginatorModule,
    MatIconModule,
    LoadingSpinnerComponent
  ],
  template: `
    <mat-card class="inner-card">
      <mat-card-content class="p-0">
        <h3 class="pane-title">Claim Timeline & System Audit Logs</h3>

        @if (loading) {
          <app-loading-spinner [diameter]="40" message="Loading audit timeline..."></app-loading-spinner>
        } @else {
          
          <div class="timeline-container">
            @for (log of auditLogs; track log.id) {
              <div class="timeline-item">
                <div class="timeline-icon-box" [ngClass]="getEventColorClass(log.action)">
                  <mat-icon>{{ getEventIcon(log.action) }}</mat-icon>
                </div>
                
                <div class="timeline-content">
                  <div class="timeline-header">
                    <span class="event-type">{{ log.action }}</span>
                    <span class="timestamp">{{ log.performedAt | date:'medium' }}</span>
                  </div>
                  
                  <p class="description">{{ log.details }}</p>
                  
                  @if (log.performedByUserId) {
                    <div class="user-stamp">
                      By User ID: <code>{{ log.performedByUserId }}</code>
                    </div>
                  }

                  @if (log.previousValue || log.newValue) {
                    <div class="diff-block">
                      @if (log.previousValue) {
                        <div class="diff-line diff-old">
                          <span class="diff-indicator">-</span>
                          <code>{{ log.previousValue }}</code>
                        </div>
                      }
                      @if (log.newValue) {
                        <div class="diff-line diff-new">
                          <span class="diff-indicator">+</span>
                          <code>{{ log.newValue }}</code>
                        </div>
                      }
                    </div>
                  }
                </div>
              </div>
            }

            @if (auditLogs.length === 0) {
              <div class="empty-timeline-state">
                <mat-icon>pending_actions</mat-icon>
                <p>No audit entries recorded for this claim.</p>
              </div>
            }
          </div>

          <mat-paginator 
            [length]="totalCount"
            [pageSize]="pageSize"
            [pageIndex]="pageNumber - 1"
            [pageSizeOptions]="[5, 10, 25]"
            (page)="onPageChange($event)"
            showFirstLastButtons>
          </mat-paginator>

        }
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
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
      padding-left: 20px;
      padding-right: 20px;
    }

    .p-0 {
      padding: 0;
    }

    .timeline-container {
      padding: 10px 30px 30px 30px;
      position: relative;

      &::before {
        content: '';
        position: absolute;
        top: 0;
        bottom: 0;
        left: 49px;
        width: 2px;
        background-color: var(--border-color);
      }
    }

    .timeline-item {
      display: flex;
      gap: 25px;
      margin-bottom: 25px;
      position: relative;

      &:last-child {
        margin-bottom: 0;
      }
    }

    .timeline-icon-box {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background-color: rgba(255, 255, 255, 0.05);
      border: 1px solid var(--border-color);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 2;

      mat-icon {
        font-size: 18px;
        width: 18px;
        height: 18px;
      }

      &.event-created {
        background-color: rgba(52, 199, 89, 0.15);
        color: #34c759;
        border-color: rgba(52, 199, 89, 0.3);
      }

      &.event-status {
        background-color: rgba(0, 122, 255, 0.15);
        color: #007aff;
        border-color: rgba(0, 122, 255, 0.3);
      }

      &.event-reserve {
        background-color: rgba(255, 149, 0, 0.15);
        color: #ff9500;
        border-color: rgba(255, 149, 0, 0.3);
      }

      &.event-document {
        background-color: rgba(175, 82, 222, 0.15);
        color: #af52de;
        border-color: rgba(175, 82, 222, 0.3);
      }
    }

    .timeline-content {
      flex: 1;
      background: rgba(255, 255, 255, 0.01);
      border: 1px solid var(--border-color);
      border-radius: 8px;
      padding: 15px;

      .timeline-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        border-bottom: 1px solid rgba(255, 255, 255, 0.04);
        padding-bottom: 6px;
        margin-bottom: 8px;

        .event-type {
          font-weight: 700;
          font-size: 0.8rem;
          letter-spacing: 0.5px;
          text-transform: uppercase;
        }

        .timestamp {
          font-size: 0.78rem;
          color: var(--text-muted);
        }
      }

      .description {
        margin: 0;
        color: var(--text-primary);
        font-size: 0.9rem;
        line-height: 1.45;
      }

      .user-stamp {
        margin-top: 10px;
        font-size: 0.72rem;
        color: var(--text-secondary);
        
        code {
          background-color: rgba(255, 255, 255, 0.05);
          padding: 2px 4px;
          border-radius: 4px;
        }
      }
    }

    .diff-block {
      margin-top: 10px;
      background-color: #0b0b0f;
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 6px;
      padding: 10px;
      font-family: 'Roboto Mono', monospace;
      font-size: 0.8rem;
      display: flex;
      flex-direction: column;
      gap: 5px;
    }

    .diff-line {
      display: flex;
      gap: 10px;
      
      .diff-indicator {
        font-weight: bold;
        width: 10px;
      }
      
      &.diff-old {
        color: #ff3b30;
      }
      
      &.diff-new {
        color: #34c759;
      }
    }

    .empty-timeline-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 50px;
      color: var(--text-muted);

      mat-icon {
        font-size: 40px;
        width: 40px;
        height: 40px;
        margin-bottom: 12px;
      }

      p {
        margin: 0;
        font-size: 0.95rem;
      }
    }

    mat-paginator {
      background: transparent;
      border-top: 1px solid var(--border-color);
    }
  `]
})
export class AuditLogTabComponent implements OnInit {
  @Input() claimId!: string;

  auditLogs: AuditLogEntry[] = [];
  loading = true;
  totalCount = 0;
  pageNumber = 1;
  pageSize = 10;

  constructor(private claimsApi: ClaimsApiService) {}

  ngOnInit(): void {
    this.loadAuditLogs();
  }

  loadAuditLogs(): void {
    this.loading = true;
    this.claimsApi.getAuditLog(this.claimId, this.pageNumber, this.pageSize).subscribe({
      next: (res) => {
        this.auditLogs = res.items;
        this.totalCount = res.totalCount;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load audit logs', err);
        this.loading = false;
      }
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadAuditLogs();
  }

  getEventIcon(action: string): string {
    const act = (action || '').toUpperCase();
    if (act.includes('CREATED') || act.includes('INTAKE')) return 'add_circle';
    if (act.includes('STATUS')) return 'swap_horiz';
    if (act.includes('RESERVE')) return 'monetization_on';
    if (act.includes('DOCUMENT')) return 'cloud_upload';
    if (act.includes('PARTY')) return 'person_add';
    return 'settings';
  }

  getEventColorClass(action: string): string {
    const act = (action || '').toUpperCase();
    if (act.includes('CREATED') || act.includes('INTAKE')) return 'event-created';
    if (act.includes('STATUS')) return 'event-status';
    if (act.includes('RESERVE')) return 'event-reserve';
    if (act.includes('DOCUMENT')) return 'event-document';
    return '';
  }
}
