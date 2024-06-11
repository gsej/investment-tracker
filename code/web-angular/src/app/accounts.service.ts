import { Injectable } from '@angular/core';
import { Account } from './models/account';
import { AccountSummary } from "./models/accountSummary";
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { RecordedTotalValues } from './models/recordedTotalValues';
import { AccountHistoricalValues } from './models/accountHistoricalValues';


@Injectable({
  providedIn: 'root'
})
export class AccountsService {

  _today: string;
  constructor(private http: HttpClient) {
    this._today = new Date().toISOString().substring(0, 10);
  }

  getAccounts(): Observable<Account[]> {
    return this.http.get<any>('http://localhost:5100/accounts').pipe(map((result: any) => result.accounts));
  }

  getAccountSummary(accountCode: string, date: string): Observable<AccountSummary> {
    return this.http.post<AccountSummary>('http://localhost:5100/account/summary', { accountCode: accountCode, date: date })
  }

  getRecordedTotalValues(accountCode: string): Observable<RecordedTotalValues> {
    return this.http.post<RecordedTotalValues>('http://localhost:5100/account/recorded-total-values', { accountCode: accountCode })
  }

  getAccountValueHistory(accountCode: string): Observable<AccountHistoricalValues> {
    return this.http.post<AccountHistoricalValues>('http://localhost:5100/account/account-value-history', { accountCode: accountCode })
  }
}
