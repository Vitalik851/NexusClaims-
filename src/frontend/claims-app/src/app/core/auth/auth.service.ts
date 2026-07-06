import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface AppUser {
  id: string;
  name: string;
  role: UserRole;
  organizationEntityId: string;
}

export type UserRole = 'handler' | 'supervisor' | 'manager';

const MOCK_USERS: Record<UserRole, AppUser> = {
  handler: {
    id: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    name: 'John Handler',
    role: 'handler',
    organizationEntityId: '00000000-0000-0000-0000-000000000001',
  },
  supervisor: {
    id: 'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    name: 'Sarah Supervisor',
    role: 'supervisor',
    organizationEntityId: '00000000-0000-0000-0000-000000000001',
  },
  manager: {
    id: 'c3d4e5f6-a7b8-9012-cdef-123456789012',
    name: 'Michael Manager',
    role: 'manager',
    organizationEntityId: '00000000-0000-0000-0000-000000000001',
  },
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly currentUserSubject = new BehaviorSubject<AppUser | null>(null);
  readonly currentUser$: Observable<AppUser | null> = this.currentUserSubject.asObservable();

  constructor() {
    this.login('handler');
  }

  get currentUser(): AppUser | null {
    return this.currentUserSubject.value;
  }

  login(role: UserRole): void {
    const user = MOCK_USERS[role];
    this.currentUserSubject.next(user);
    sessionStorage.setItem('cms_token', this.generateFakeToken(user));
    sessionStorage.setItem('cms_user', JSON.stringify(user));
  }

  logout(): void {
    this.currentUserSubject.next(null);
    sessionStorage.removeItem('cms_token');
    sessionStorage.removeItem('cms_user');
  }

  getToken(): string | null {
    return sessionStorage.getItem('cms_token');
  }

  hasRole(role: UserRole): boolean {
    return this.currentUser?.role === role;
  }

  hasAnyRole(...roles: UserRole[]): boolean {
    return this.currentUser != null && roles.includes(this.currentUser.role);
  }

  getAvailableRoles(): UserRole[] {
    return ['handler', 'supervisor', 'manager'];
  }

  getMockUsers(): Record<UserRole, AppUser> {
    return { ...MOCK_USERS };
  }

  private generateFakeToken(user: AppUser): string {
    const toBase64Url = (str: string) => btoa(str).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
    
    const header = toBase64Url(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
    const payload = toBase64Url(
      JSON.stringify({
        'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': user.id,
        'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': user.name,
        'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': user.role,
        OrganizationId: user.organizationEntityId,
        iat: Math.floor(Date.now() / 1000),
        exp: Math.floor(Date.now() / 1000) + 3600,
      })
    );
    const signature = toBase64Url('fake-signature-for-dev');
    return `${header}.${payload}.${signature}`;
  }
}
