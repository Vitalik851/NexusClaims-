export interface Party {
  id: string;
  claimId: string;
  partyRole: PartyRole;
  partyType: PartyType;
  firstName: string | null;
  lastName: string | null;
  companyName: string | null;
  email: string | null;
  phone: string | null;
  address: PartyAddress | null;
  notes: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface PartyAddress {
  street: string | null;
  city: string | null;
  state: string | null;
  postalCode: string | null;
  country: string | null;
}

export type PartyRole =
  | 'Insured'
  | 'Claimant'
  | 'Witness'
  | 'ThirdParty'
  | 'Attorney'
  | 'MedicalProvider'
  | 'Adjuster';

export type PartyType = 'Individual' | 'Company';

export interface CreatePartyRequest {
  partyRole: string;
  partyType: string;
  firstName: string | null;
  lastName: string | null;
  companyName: string | null;
  email: string | null;
  phone: string | null;
  notes: string | null;
}

export interface UpdatePartyRequest {
  partyRole: string;
  firstName: string | null;
  lastName: string | null;
  companyName: string | null;
  email: string | null;
  phone: string | null;
  notes: string | null;
  isActive: boolean;
}
