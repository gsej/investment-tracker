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

  // TODO: remove single account support
  //_selectedAccount: string | null = null;

  _selectedAccounts: string[] = [];

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

  selectAccounts(accountCodes: string[]) {
    this._selectedAccounts = accountCodes;
    this.getPortfolio(accountCodes, this._today).subscribe(summary => {
      this._portfolioSubject.next(summary);
    });

    this.getHistory(accountCodes, this._today).subscribe(history => {
      this._historySubject.next(history);
    });

  }

  getPortfolio(accountCodes: string[], date: string): Observable<Portfolio> {
    return this.http.post<Portfolio>('http://localhost:5100/account/portfolio', { accountCodes: accountCodes, date: date })
  }

  getHistory(accountCodes: string[], queryDate: string): Observable<HistoryViewModels> {
    return this.http.post<HistoryViewModels>('http://localhost:5100/account/precalculated-history', { accountCodes: accountCodes, queryDate: queryDate })
  }
}
