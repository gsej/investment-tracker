import { Injectable } from '@angular/core';
import { Account } from './models/account';
import { Portfolio } from "./models/portfolio";
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { HistoryViewModels } from './view-models/HistoryViewModels';

@Injectable({
  providedIn: 'root'
})
export class AccountsService {

  _today: string;

  _selectedAccount: string | null = null;

  private _portfolioSubject: BehaviorSubject<Portfolio | null> = new BehaviorSubject<Portfolio | null>(null);
  public portfolio$: Observable<Portfolio | null> = this._portfolioSubject.asObservable();

  private _historySubject: BehaviorSubject<HistoryViewModels | null> = new BehaviorSubject<HistoryViewModels | null>(null);
  public history$: Observable<HistoryViewModels | null> = this._historySubject.asObservable();

  constructor(private http: HttpClient) {
    this._today = new Date().toISOString().substring(0, 10);
  }

  getAccounts(): Observable<Account[]> {
    return this.http.get<any>('http://localhost:5100/accounts').pipe(map((result: any) => result.accounts));
  }

  selectAccount(accountCode: string) {
    this._selectedAccount = accountCode;
    this.getPortfolio(accountCode, this._today).subscribe(summary => {
      this._portfolioSubject.next(summary);
    });


    this.getHistory(accountCode, this._today).subscribe(history => {
      this._historySubject.next(history);
    });

  }

  getPortfolio(accountCode: string, date: string): Observable<Portfolio> {
    return this.http.post<Portfolio>('http://localhost:5100/account/portfolio', { accountCode: accountCode, date: date })
  }

  getHistory(accountCode: string, queryDate: string): Observable<HistoryViewModels> {
    return this.http.post<HistoryViewModels>('http://localhost:5100/account/history', { accountCode: accountCode, queryDate: queryDate })
  }
}
