import { Injectable } from '@angular/core';
import { Account } from './models/account';
import { Portfolio } from "./models/portfolio";
import { BehaviorSubject, Observable, combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { HistoryViewModels } from './view-models/HistoryViewModels';
import { StockHistoryViewModel } from './view-models/StockHistoryViewModel';
import { StockViewModel } from './view-models/StockViewModel';

@Injectable({
  providedIn: 'root'
})
export class AccountsService {

  _today: string;

  _selectedAccounts: string[] = [];

  private _portfolioSubject: BehaviorSubject<Portfolio | null> = new BehaviorSubject<Portfolio | null>(null);
  public portfolio$: Observable<Portfolio | null> = this._portfolioSubject.asObservable();

  private _historySubject: BehaviorSubject<HistoryViewModels | null> = new BehaviorSubject<HistoryViewModels | null>(null);
  public history$: Observable<HistoryViewModels | null> = this._historySubject.asObservable();

  private _dateRangeSubject = new BehaviorSubject<{ start: string; end: string }>({ start: '', end: '' });
  public dateRange$ = this._dateRangeSubject.asObservable();

  public filteredHistory$: Observable<HistoryViewModels | null> = combineLatest([
    this._historySubject,
    this._dateRangeSubject
  ]).pipe(
    map(([history, range]) => {
      if (!history) return null;
      if (!range.start && !range.end) return history;
      const items = history.items.filter(item =>
        (!range.start || item.date >= range.start) &&
        (!range.end || item.date <= range.end)
      );
      const comments = history.comments.filter(comment =>
        (!range.start || comment.date >= range.start) &&
        (!range.end || comment.date <= range.end)
      );
      return { items, comments, trailingReturns: history.trailingReturns };
    })
  );

  constructor(private http: HttpClient) {
    this._today = new Date().toISOString().substring(0, 10);
  }

  getAccounts(): Observable<Account[]> {
    return this.http.get<any>('http://localhost:5100/accounts').pipe(map((result: any) => result.accounts));
  }

  selectAccounts(accountCodes: string[]) {
    this._selectedAccounts = accountCodes;

    if (accountCodes.length === 0) {
      this._portfolioSubject.next(null);
      this._historySubject.next(null);
      return;
    }

    this.getPortfolio(accountCodes, this._today).subscribe(summary => {
      this._portfolioSubject.next(summary);
    });

    this.getHistory(accountCodes, this._today).subscribe(history => {
      this._historySubject.next(history);
    });
  }

  setDateRange(range: { start: string; end: string }): void {
    this._dateRangeSubject.next(range);
  }

  getPortfolio(accountCodes: string[], date: string): Observable<Portfolio> {
    return this.http.post<Portfolio>('http://localhost:5100/account/portfolio', { accountCodes: accountCodes, date: date })
  }

  getHistory(accountCodes: string[], queryDate: string): Observable<HistoryViewModels> {
    return this.http.post<HistoryViewModels>('http://localhost:5100/account/precalculated-history', { accountCodes: accountCodes, queryDate: queryDate })
  }

  getStocks(): Observable<StockViewModel[]> {
    return this.http.get<StockViewModel[]>('http://localhost:5100/stocks');
  }

  getStockHistory(stockSymbol: string, queryDate: string): Observable<StockHistoryViewModel> {
    return this.http.post<StockHistoryViewModel>('http://localhost:5100/stock/history', { stockSymbol, queryDate });
  }
}
