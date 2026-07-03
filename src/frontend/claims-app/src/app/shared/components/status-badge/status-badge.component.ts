import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span class="status-badge" [ngClass]="badgeClass">
      <span class="dot"></span>
      {{ formatStatus(status) }}
    </span>
  `,
  styles: [`
    .status-badge {
      display: inline-flex;
      align-items: center;
      padding: 4px 10px;
      border-radius: 12px;
      font-size: 0.75rem;
      font-weight: 600;
      letter-spacing: 0.3px;
      text-transform: uppercase;
      gap: 6px;
    }
    .dot {
      width: 6px;
      height: 6px;
      border-radius: 50%;
    }
    
    /* Status Styles */
    .status-draft {
      background-color: rgba(142, 142, 147, 0.15);
      color: #8e8e93;
      .dot { background-color: #8e8e93; }
    }
    .status-open {
      background-color: rgba(52, 199, 89, 0.15);
      color: #34c759;
      .dot { background-color: #34c759; }
    }
    .status-underinvestigation {
      background-color: rgba(255, 149, 0, 0.15);
      color: #ff9500;
      .dot { background-color: #ff9500; }
    }
    .status-pendingpayment {
      background-color: rgba(0, 122, 255, 0.15);
      color: #007aff;
      .dot { background-color: #007aff; }
    }
    .status-closed {
      background-color: rgba(255, 59, 48, 0.15);
      color: #ff3b30;
      .dot { background-color: #ff3b30; }
    }
    .status-reopened {
      background-color: rgba(175, 82, 222, 0.15);
      color: #af52de;
      .dot { background-color: #af52de; }
    }
    .status-withdrawn {
      background-color: rgba(142, 142, 147, 0.25);
      color: #aeaeae;
      .dot { background-color: #aeaeae; }
    }
  `]
})
export class StatusBadgeComponent {
  @Input() status: string = '';

  get badgeClass(): string {
    if (!this.status) return 'status-draft';
    return `status-${this.status.toLowerCase()}`;
  }

  formatStatus(status: string): string {
    if (!status) return '';
    // Format camelCase or long status names for nicer display
    if (status === 'UnderInvestigation') return 'Investigation';
    if (status === 'PendingPayment') return 'Pending Pay';
    return status;
  }
}
