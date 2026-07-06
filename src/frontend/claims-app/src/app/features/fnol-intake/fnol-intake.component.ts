import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatStepperModule } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { ClaimsApiService } from '../../core/services/claims-api.service';
import { PoliciesApiService } from '../../core/services/policies-api.service';
import { ReferenceDataApiService, CauseOfLossCodeDto } from '../../core/services/reference-data-api.service';
import { PolicySearchResult } from '../../core/models/policy.model';
import { CreatePartyRequest, CreateRiskObjectRequest } from '../../core/models/claim.model';
import { debounceTime, switchMap } from 'rxjs/operators';
import { of } from 'rxjs';

// Custom validator to check that date is not in the future
function noFutureDateValidator(control: AbstractControl): { [key: string]: boolean } | null {
  if (control.value) {
    const inputDate = new Date(control.value);
    const today = new Date();
    // Clear time
    today.setHours(0, 0, 0, 0);
    inputDate.setHours(0, 0, 0, 0);

    if (inputDate > today) {
      return { 'futureDate': true };
    }
  }
  return null;
}

@Component({
  selector: 'app-fnol-intake',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatCardModule,
    MatStepperModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatAutocompleteModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatCheckboxModule,
    MatSnackBarModule
  ],
  templateUrl: './fnol-intake.component.html',
  styleUrls: ['./fnol-intake.component.scss']
})
export class FnolIntakeComponent implements OnInit {
  firstFormGroup!: FormGroup;
  partyForm!: FormGroup;
  riskForm!: FormGroup;
  fourthFormGroup!: FormGroup;

  causeOfLossCodes: CauseOfLossCodeDto[] = [];
  searchedPolicies: PolicySearchResult[] = [];
  selectedPolicy: PolicySearchResult | null = null;
  
  parties: CreatePartyRequest[] = [];
  riskObjects: CreateRiskObjectRequest[] = [];
  
  today = new Date();

  constructor(
    private fb: FormBuilder,
    private claimsApi: ClaimsApiService,
    private policiesApi: PoliciesApiService,
    private refDataApi: ReferenceDataApiService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.initForms();
    this.loadReferenceData();
    this.setupPolicySearch();
  }

  initForms(): void {
    this.firstFormGroup = this.fb.group({
      policyNumber: ['', Validators.required],
      causeOfLossCode: ['', Validators.required],
      lossDate: [new Date(), [Validators.required, noFutureDateValidator]],
      lossLocation: [''],
      estimatedLossAmount: [null],
      severity: ['Standard'],
      lossDescription: ['', [Validators.required, Validators.minLength(20)]]
    });

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

    this.riskForm = this.fb.group({
      assetType: ['Vehicle', Validators.required],
      assetDescription: ['', Validators.required],
      damageDescription: [''],
      isPrimary: [false],
      assetReference: ['']
    });

    this.fourthFormGroup = this.fb.group({
      component: ['Indemnity'],
      amount: [0, [Validators.min(0)]],
      changeReason: ['Initial reserve established during intake.']
    });

    // Handle conditional validation for party form type
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

    // Trigger initial type validation
    this.partyForm.get('partyType')?.setValue('Person');
  }

  loadReferenceData(): void {
    this.refDataApi.getCauseOfLossCodes().subscribe(codes => this.causeOfLossCodes = codes);
  }

  setupPolicySearch(): void {
    this.firstFormGroup.get('policyNumber')?.valueChanges.pipe(
      debounceTime(300),
      switchMap(value => {
        if (value && value.length >= 2) {
          return this.policiesApi.search(value);
        }
        return of([]);
      })
    ).subscribe(policies => {
      this.searchedPolicies = policies;
    });
  }

  onPolicySelected(policyNumber: string): void {
    const matched = this.searchedPolicies.find(p => p.policyNumber === policyNumber);
    if (matched) {
      this.selectedPolicy = matched;
      
      // Auto-add the client as Insured party to speed up intake
      const alreadyHasInsured = this.parties.some(p => p.partyRole === 'Insured');
      if (!alreadyHasInsured) {
        this.parties.push({
          partyRole: 'Insured',
          partyType: 'Company',
          companyName: matched.insuredName,
          firstName: null,
          lastName: null,
          email: null,
          phone: null,
          notes: 'Auto-populated from policy client details.'
        });
      }
    }
  }

  addParty(): void {
    if (this.partyForm.invalid) return;

    const formVal = this.partyForm.value;
    const newParty: CreatePartyRequest = {
      partyRole: formVal.partyRole,
      partyType: formVal.partyType,
      firstName: formVal.partyType === 'Person' ? formVal.firstName : null,
      lastName: formVal.partyType === 'Person' ? formVal.lastName : null,
      companyName: formVal.partyType === 'Company' ? formVal.companyName : null,
      email: formVal.email || null,
      phone: formVal.phone || null,
      notes: formVal.notes || null
    };

    this.parties.push(newParty);
    
    // Reset inputs, preserving default role/type
    this.partyForm.patchValue({
      firstName: '',
      lastName: '',
      companyName: '',
      email: '',
      phone: '',
      notes: ''
    });
    this.partyForm.markAsUntouched();
  }

  removeParty(index: number): void {
    this.parties.splice(index, 1);
  }

  hasClaimant(): boolean {
    return this.parties.some(p => p.partyRole === 'Claimant');
  }

  addRiskObject(): void {
    if (this.riskForm.invalid) return;

    const formVal = this.riskForm.value;
    const newRisk: CreateRiskObjectRequest = {
      assetType: formVal.assetType,
      assetDescription: formVal.assetDescription,
      damageDescription: formVal.damageDescription || null,
      isPrimary: formVal.isPrimary,
      assetReference: formVal.assetReference || null
    };

    this.riskObjects.push(newRisk);
    this.riskForm.reset({
      assetType: 'Vehicle',
      assetDescription: '',
      damageDescription: '',
      isPrimary: false,
      assetReference: ''
    });
  }

  removeRiskObject(index: number): void {
    this.riskObjects.splice(index, 1);
  }

  submitClaim(): void {
    if (this.firstFormGroup.invalid) return;

    const step1 = this.firstFormGroup.value;
    const step4 = this.fourthFormGroup.value;

    const rawCause = (step1.causeOfLossCode || '').toString().trim().toLowerCase();
    let mappedCode = 'COL-OTHER';
    if (rawCause.includes('fire')) mappedCode = 'COL-FIRE';
    else if (rawCause.includes('flood')) mappedCode = 'COL-FLOOD';
    else if (rawCause.includes('theft')) mappedCode = 'COL-THEFT';
    else if (rawCause.includes('collision') || rawCause.includes('veh-col')) mappedCode = 'COL-VEH-COL';
    else if (rawCause.includes('comp') || rawCause.includes('veh-comp')) mappedCode = 'COL-VEH-COMP';
    else if (rawCause.includes('liab') || rawCause.includes('liability')) mappedCode = 'COL-LIAB';
    else if (rawCause.includes('equip')) mappedCode = 'COL-EQUIP';
    else if (rawCause.includes('wind') || rawCause.includes('storm')) mappedCode = 'COL-WIND';
    else if (rawCause.includes('injury')) mappedCode = 'COL-INJURY';
    else if (rawCause.includes('other') || rawCause.includes('unknown')) mappedCode = 'COL-OTHER';

    const upperCause = rawCause.toUpperCase();
    if (['COL-FIRE', 'COL-FLOOD', 'COL-THEFT', 'COL-VEH-COL', 'COL-VEH-COMP', 'COL-LIAB', 'COL-EQUIP', 'COL-WIND', 'COL-INJURY', 'COL-OTHER'].includes(upperCause)) {
      mappedCode = upperCause;
    }

    const claimRequest = {
      organizationEntityId: '11111111-1111-1111-1111-111111111111', // Enriched by API but defined for DTO structure
      policyId: this.selectedPolicy ? this.selectedPolicy.id : null,
      lossDate: step1.lossDate.toISOString(),
      lossDescription: step1.lossDescription,
      lossLocation: step1.lossLocation || null,
      causeOfLossCode: mappedCode,
      estimatedLossAmount: step1.estimatedLossAmount,
      severity: step1.severity,
      parties: this.parties,
      riskObjects: this.riskObjects,
      initialReserve: (this.selectedPolicy && step4.amount > 0) ? {
        component: step4.component,
        amount: step4.amount,
        changeReason: step4.changeReason
      } : null
    };

    this.claimsApi.create(claimRequest).subscribe({
      next: (res) => {
        this.snackBar.open('Claim successfully registered!', 'Close', { duration: 4000 });
        this.router.navigate(['/']);
      },
      error: (err) => {
        console.error('Failed to submit claim', err);
        let msg = `Stat: ${err.status} - ${err.message}`;
        if (err.error) msg += ` - Err: ${typeof err.error === 'object' ? JSON.stringify(err.error) : err.error}`;
        this.snackBar.open(msg.substring(0, 500), 'Close', { duration: 15000 });
      }
    });
  }
}
