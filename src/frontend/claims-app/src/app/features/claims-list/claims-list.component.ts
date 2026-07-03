import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

import { ClaimsApiService, PaginatedResponse } from '../../core/services/claims-api.service';
import { ReferenceDataApiService, CauseOfLossCodeDto } from '../../core/services/reference-data-api.service';
import { ClaimSummary } from '../../core/models/claim.model';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-claims-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatDatepickerModule,
    MatNativeDateModule,
    StatusBadgeComponent,
    LoadingSpinnerComponent
  ],
  templateUrl: './claims-list.component.html',
  styleUrls: ['./claims-list.component.scss']
})
export class ClaimsListComponent implements OnInit {
  claims: ClaimSummary[] = [];
  causeOfLossCodes: CauseOfLossCodeDto[] = [];
  loading = true;
  totalCount = 0;

  displayedColumns: string[] = [
    'claimNumber',
    'policyNumber',
    'clientName',
    'lossDate',
    'causeOfLoss',
    'totalReserves',
    'status',
    'actions'
  ];

  filters = {
    status: '',
    dateFrom: null as Date | null,
    dateTo: null as Date | null,
    causeOfLossCode: '',
    search: '',
    pageNumber: 1,
    pageSize: 10
  };

  private searchSubject = new Subject<string>();

  constructor(
    private claimsApi: ClaimsApiService,
    private refDataApi: ReferenceDataApiService
  ) {}

  ngOnInit(): void {
    this.loadReferenceData();
    this.loadClaims();

    // Debounce search input to avoid hitting database on every keystroke
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(() => {
      this.filters.pageNumber = 1;
      this.loadClaims();
    });
  }

  loadReferenceData(): void {
    this.refDataApi.getCauseOfLossCodes().subscribe({
      next: (codes) => this.causeOfLossCodes = codes,
      error: (err) => console.error('Failed to load cause of loss codes', err)
    });
  }

  loadClaims(): void {
    this.loading = true;

    // Convert dates to ISO strings before calling API
    const apiFilters = {
      ...this.filters,
      dateFrom: this.filters.dateFrom ? this.filters.dateFrom.toISOString() : undefined,
      dateTo: this.filters.dateTo ? this.filters.dateTo.toISOString() : undefined
    };

    this.claimsApi.getAll(apiFilters).subscribe({
      next: (response) => {
        this.claims = response.items;
        this.totalCount = response.totalCount;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load claims', err);
        this.loading = false;
      }
    });
  }

  onFilterChange(): void {
    // If search is changed, trigger debounced subject. Otherwise trigger direct reload
    this.searchSubject.next(this.filters.search);
    if (!this.filters.search) {
      this.filters.pageNumber = 1;
      this.loadClaims();
    }
  }

  onPageChange(event: PageEvent): void {
    this.filters.pageNumber = event.pageIndex + 1;
    this.filters.pageSize = event.pageSize;
    this.loadClaims();
  }

  resetFilters(): void {
    this.filters = {
      status: '',
      dateFrom: null,
      dateTo: null,
      causeOfLossCode: '',
      search: '',
      pageNumber: 1,
      pageSize: 10
    };
    this.loadClaims();
  }
}
