import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule],
  template: `
    <div class="spinner-container" [ngClass]="{'overlay': overlay}">
      <mat-progress-spinner 
        mode="indeterminate" 
        [diameter]="diameter" 
        color="accent">
      </mat-progress-spinner>
      @if (message) {
        <p class="spinner-message">{{ message }}</p>
      }
    </div>
  `,
  styles: [`
    .spinner-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 24px;
      min-height: 120px;
    }
    .spinner-container.overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background-color: rgba(26, 26, 36, 0.7); /* Professional dark backdrop */
      z-index: 10;
      border-radius: 8px;
    }
    .spinner-message {
      margin-top: 16px;
      color: var(--text-secondary);
      font-size: 0.9rem;
      font-weight: 500;
    }
  `]
})
export class LoadingSpinnerComponent {
  @Input() diameter: number = 40;
  @Input() overlay: boolean = false;
  @Input() message: string = '';
}
