import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
  private config: any = null;

  constructor(private http: HttpClient) {}

  loadConfig(): Promise<void> {
    return firstValueFrom(
      this.http.get('./config.json')
    ).then(
      (data) => {
        this.config = data;
      },
      (err) => {
        console.error('Could not load config.json, using fallback API URL.', err);
        this.config = { apiUrl: 'https://localhost:7081/api' };
      }
    );
  }

  get apiUrl(): string {
    return this.config ? this.config.apiUrl : 'https://localhost:7081/api';
  }
}
