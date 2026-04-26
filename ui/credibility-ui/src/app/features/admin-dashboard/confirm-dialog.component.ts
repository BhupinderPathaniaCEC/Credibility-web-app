import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  isDangerous?: boolean;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="dialog-overlay" *ngIf="isOpen" (click)="onCancel()">
      <div class="dialog-container" [class.dangerous]="data.isDangerous" (click)="$event.stopPropagation()">
        <h2 class="dialog-title">{{ data.title }}</h2>
        <p class="dialog-message">{{ data.message }}</p>
        
        <div class="dialog-actions">
          <button class="btn cancel-btn" (click)="onCancel()">
            {{ data.cancelText || 'Cancel' }}
          </button>
          <button class="btn confirm-btn" [class.dangerous]="data.isDangerous" (click)="onConfirm()">
            {{ data.confirmText || 'Confirm' }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dialog-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background-color: rgba(0, 0, 0, 0.5);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 1000;
    }

    .dialog-container {
      padding: 20px;
      min-width: 300px;
      background: white;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
    }

    .dialog-container.dangerous {
      border-left: 4px solid #dc3545;
    }

    .dialog-title {
      margin: 0 0 10px 0;
      font-size: 18px;
      font-weight: 600;
      color: #333;
    }

    .dialog-message {
      margin: 0 0 20px 0;
      color: #666;
      line-height: 1.5;
    }

    .dialog-actions {
      display: flex;
      gap: 10px;
      justify-content: flex-end;
    }

    .btn {
      padding: 8px 16px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 14px;
      font-weight: 500;
    }

    .cancel-btn {
      background-color: #f0f0f0;
      color: #333;
    }

    .cancel-btn:hover {
      background-color: #e0e0e0;
    }

    .confirm-btn {
      background-color: #0066cc;
      color: white;
    }

    .confirm-btn:hover {
      background-color: #0052a3;
    }

    .confirm-btn.dangerous {
      background-color: #dc3545;
    }

    .confirm-btn.dangerous:hover {
      background-color: #c82333;
    }
  `]
})
export class ConfirmDialogComponent {
  @Input() isOpen = false;
  @Input() data: ConfirmDialogData = {
    title: '',
    message: ''
  };
  @Output() confirmed = new EventEmitter<boolean>();

  onCancel(): void {
    this.isOpen = false;
    this.confirmed.emit(false);
  }

  onConfirm(): void {
    this.isOpen = false;
    this.confirmed.emit(true);
  }
}
