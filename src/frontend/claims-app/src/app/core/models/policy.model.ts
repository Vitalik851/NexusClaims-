export interface Policy {
  id: string;
  policyNumber: string;
  insuredName: string;
  effectiveDate: string;
  expirationDate: string;
  lineOfBusiness: string;
  status: PolicyStatus;
  premium: number;
  deductible: number;
  coverageLimit: number;
}

export type PolicyStatus = 'Active' | 'Expired' | 'Cancelled' | 'Pending';

export interface PolicySearchResult {
  id: string;
  policyNumber: string;
  insuredName: string;
  lineOfBusiness: string;
  effectiveDate: string;
  expirationDate: string;
  status: PolicyStatus;
}

export interface PolicySearchRequest {
  policyNumber: string | null;
  insuredName: string | null;
  lineOfBusiness: string | null;
  pageNumber: number;
  pageSize: number;
}

export interface PolicySearchResponse {
  items: PolicySearchResult[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface PolicyCoverageDto {
  coverageType: string;
  limitAmount: number;
  deductibleAmount: number;
}
