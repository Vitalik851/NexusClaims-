import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';

import { ClaimDetail, ClaimDocumentDto } from '../../../../core/models/claim.model';
import { ClaimsApiService } from '../../../../core/services/claims-api.service';

@Component({
  selector: 'app-documents-tab',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule
  ],
  template: `
    <div class="documents-grid">
      <!-- Left side: List of existing documents -->
      <div class="list-pane">
        <mat-card class="inner-card">
          <mat-card-content class="p-0">
            <h3 class="pane-title">Uploaded Claim Documents</h3>
            
            <table mat-table [dataSource]="claim.documents" class="w-100">
              <!-- Name Column -->
              <ng-container matColumnDef="name">
                <th mat-header-cell *matHeaderCellDef>File Name</th>
                <td mat-cell *matCellDef="let d" class="doc-name-cell">
                  <mat-icon class="doc-icon">{{ getDocIcon(d.contentType) }}</mat-icon>
                  <div>
                    <strong>{{ d.documentName }}</strong>
                    <div class="doc-size">{{ formatBytes(d.fileSizeBytes) }}</div>
                  </div>
                </td>
              </ng-container>

              <!-- Type Column -->
              <ng-container matColumnDef="type">
                <th mat-header-cell *matHeaderCellDef>Type</th>
                <td mat-cell *matCellDef="let d">
                  <span class="type-badge">{{ d.documentType }}</span>
                </td>
              </ng-container>

              <!-- Uploaded Column -->
              <ng-container matColumnDef="date">
                <th mat-header-cell *matHeaderCellDef>Uploaded At</th>
                <td mat-cell *matCellDef="let d">
                  {{ d.uploadedAt | date:'short' }}
                </td>
              </ng-container>

              <!-- Actions Column -->
              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef></th>
                <td mat-cell *matCellDef="let d" align="end">
                  <a mat-stroked-button color="primary" [href]="getDownloadUrl(d)" target="_blank">
                    <mat-icon>download</mat-icon> Download
                  </a>
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
            </table>

            @if (claim.documents.length === 0) {
              <div class="empty-list-state">
                <mat-icon>folder_open</mat-icon>
                <p>No documents uploaded for this claim yet.</p>
              </div>
            }
          </mat-card-content>
        </mat-card>
      </div>

      <!-- Right side: File Upload form -->
      <div class="upload-pane">
        <mat-card class="inner-card">
          <mat-card-content>
            <h3 class="pane-title">Upload New File</h3>
            
            <form [formGroup]="uploadForm" (ngSubmit)="onUpload()">
              <div class="form-row">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>Document Type</mat-label>
                  <mat-select formControlName="documentType">
                    <mat-option value="PoliceReport">Police Report</mat-option>
                    <mat-option value="MedicalReport">Medical Report</mat-option>
                    <mat-option value="Invoice">Invoice / Bill</mat-option>
                    <mat-option value="Photo">Damage Photo</mat-option>
                    <mat-option value="Correspondence">Correspondence</mat-option>
                    <mat-option value="Other">Other Document</mat-option>
                  </mat-select>
                </mat-form-field>
              </div>

              <!-- Drag and drop or file select input -->
              <div class="file-drop-zone" (click)="fileInput.click()">
                <mat-icon>cloud_upload</mat-icon>
                @if (selectedFile) {
                  <span class="file-name">{{ selectedFile.name }}</span>
                  <span class="file-size">({{ formatBytes(selectedFile.size) }})</span>
                } @else {
                  <span>Drag & drop or click to upload file</span>
                  <span class="file-restrictions">PDF, JPEG, PNG, DOCX, XLSX, TXT, CSV (Max: 10MB)</span>
                }
                <input 
                  #fileInput 
                  type="file" 
                  style="display: none" 
                  (change)="onFileSelected($event)">
              </div>

              <div class="form-row mt-3">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>Notes / Remarks</mat-label>
                  <textarea matInput formControlName="notes" rows="3" placeholder="Add optional document context..."></textarea>
                </mat-form-field>
              </div>

              <button mat-raised-button color="accent" class="w-100 submit-btn" 
                [disabled]="uploadForm.invalid || !selectedFile || uploading">
                <mat-icon>cloud_done</mat-icon> 
                {{ uploading ? 'Uploading file...' : 'Upload Document' }}
              </button>
            </form>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .documents-grid {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: 25px;
    }

    @media (max-width: 900px) {
      .documents-grid {
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
    .mt-3 { margin-top: 15px; }

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

    .doc-name-cell {
      display: flex;
      align-items: center;
      gap: 15px;
      
      .doc-icon {
        font-size: 28px;
        width: 28px;
        height: 28px;
        color: var(--primary-color);
      }

      .doc-size {
        font-size: 0.75rem;
        color: var(--text-muted);
        margin-top: 2px;
      }
    }

    .type-badge {
      background-color: rgba(255, 255, 255, 0.05);
      border: 1px solid var(--border-color);
      padding: 2px 8px;
      border-radius: 6px;
      font-size: 0.75rem;
      color: var(--text-secondary);
    }

    .file-drop-zone {
      border: 2px dashed var(--border-color);
      border-radius: 8px;
      padding: 30px 20px;
      text-align: center;
      cursor: pointer;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 10px;
      background: rgba(255, 255, 255, 0.01);
      transition: background-color 0.2s ease, border-color 0.2s ease;

      &:hover {
        background-color: rgba(255, 255, 255, 0.03);
        border-color: var(--primary-color);
      }

      mat-icon {
        font-size: 40px;
        width: 40px;
        height: 40px;
        color: var(--text-muted);
      }

      span {
        font-size: 0.88rem;
        color: var(--text-secondary);
        font-weight: 500;
      }

      .file-name {
        color: var(--primary-color);
        font-weight: 600;
      }

      .file-size {
        font-size: 0.75rem;
        color: var(--text-muted);
      }

      .file-restrictions {
        font-size: 0.75rem;
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
  `]
})
export class DocumentsTabComponent implements OnInit {
  @Input() claim!: ClaimDetail;
  @Output() documentsChanged = new EventEmitter<void>();

  displayedColumns: string[] = ['name', 'type', 'date', 'actions'];
  uploadForm!: FormGroup;
  selectedFile: File | null = null;
  uploading = false;

  constructor(
    private fb: FormBuilder,
    private claimsApi: ClaimsApiService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.uploadForm = this.fb.group({
      documentType: ['PoliceReport', Validators.required],
      notes: ['']
    });
  }

  getDownloadUrl(doc: ClaimDocumentDto): string {
    // If the download link is relative, direct it to the backend server
    if (doc.downloadUrl && doc.downloadUrl.startsWith('/uploads')) {
      return `https://localhost:7081${doc.downloadUrl}`;
    }
    return doc.downloadUrl;
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      // Validate file size (10MB limit)
      if (file.size > 10 * 1024 * 1024) {
        this.snackBar.open('File exceeds maximum size of 10MB.', 'Close', { duration: 4000 });
        return;
      }

      // Validate file type
      const allowedTypes = [
        'application/pdf',
        'image/jpeg',
        'image/png',
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document', // DOCX
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', // XLSX
        'text/plain',
        'text/csv'
      ];

      if (!allowedTypes.includes(file.type)) {
        this.snackBar.open('Unsupported file format. Please upload PDF, JPEG, PNG, DOCX, XLSX, TXT, CSV.', 'Close', { duration: 5000 });
        return;
      }

      this.selectedFile = file;
    }
  }

  onUpload(): void {
    if (this.uploadForm.invalid || !this.selectedFile) return;

    this.uploading = true;
    const val = this.uploadForm.value;

    this.claimsApi.uploadDocument(this.claim.id, val.documentType, this.selectedFile, val.notes).subscribe({
      next: () => {
        this.snackBar.open('Document successfully uploaded and saved.', 'Close', { duration: 4000 });
        this.selectedFile = null;
        this.uploadForm.reset({
          documentType: 'PoliceReport',
          notes: ''
        });
        this.uploading = false;
        this.documentsChanged.emit();
      },
      error: (err) => {
        console.error('Upload failed', err);
        const errMsg = err.error?.message || 'Failed to upload document.';
        this.snackBar.open(`Error: ${errMsg}`, 'Close', { duration: 5000 });
        this.uploading = false;
      }
    });
  }

  getDocIcon(mimeType: string): string {
    if (mimeType.includes('pdf')) return 'picture_as_pdf';
    if (mimeType.includes('image')) return 'image';
    if (mimeType.includes('word') || mimeType.includes('document')) return 'article';
    if (mimeType.includes('sheet') || mimeType.includes('excel') || mimeType.includes('csv')) return 'table_chart';
    return 'insert_drive_file';
  }

  formatBytes(bytes: number, decimals: number = 2): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  }
}
