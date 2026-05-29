import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Account } from '../../models/account';
import { AccountsService } from '../../accounts.service';
import { PortfolioViewModel } from '../../view-models/PortfolioViewModel';
import { AccountSelectorComponent } from '../../account-selector/account-selector.component';
import { HoldingsComponent } from '../holdings/holdings.component';
import { SummaryComponent } from '../summary/summary.component';
import { HistoryViewModels } from 'src/app/view-models/HistoryViewModels';
import { StockHistoryViewModel } from 'src/app/view-models/StockHistoryViewModel';
import { HistoryComponent } from '../history/history.component';
import { HistoryChartComponent } from '../chart/history-chart.component';
import { TrailingReturnsComponent } from '../trailing-returns/trailing-returns.component';
import { QualityService } from 'src/app/quality.service';
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardContentComponent, CardTitleComponent } from '@gsej/tailwind-components';

@Component({
  selector: 'app-account-container',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    AccountSelectorComponent,
    HoldingsComponent,
    HistoryComponent,
    HistoryChartComponent,
    TrailingReturnsComponent,
    SummaryComponent,
    CardContentComponent,
    CardTitleComponent],
  templateUrl: './account-container.component.html',
  styleUrls: ['./account-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AccountContainerComponent implements OnInit {

  public accounts: Account[] = [];
  public portfolio: PortfolioViewModel | null = null;

  private fullHistory: HistoryViewModels | null = null;
  public filteredHistory: HistoryViewModels | null = null;

  public accountCodes: string[] = [];

  public date!: string;

  public rangeStart: string = '';
  public rangeEnd: string = '';
  private dataStart: string = '';
  private dataEnd: string = '';

  public benchmarkSymbol: string = '';
  public benchmarkHistory: StockHistoryViewModel | null = null;

  get availableStocks(): { symbol: string; description: string }[] {
    if (!this.portfolio?.holdings?.length) return [];
    const seen = new Map<string, string>();
    for (const h of this.portfolio.holdings) {
      if (!seen.has(h.stockSymbol)) seen.set(h.stockSymbol, h.stockDescription);
    }
    return Array.from(seen.entries()).map(([symbol, description]) => ({ symbol, description }));
  }

  showQualityData$!: Observable<boolean>;

  constructor(
    private changeDetectorRef: ChangeDetectorRef,
    private accountsService: AccountsService,
    private qualityService: QualityService) {
    this.setDateToToday();
  }

  ngOnInit(): void {

    this.showQualityData$ = this.qualityService.showQualityData$;

    this.accountsService.portfolio$.subscribe(portfolio => {

      if (portfolio == null) {
        this.portfolio = null;
      }
      else {

        const portfolioViewModel = <PortfolioViewModel>{
          accountCodes: portfolio.accountCodes ?? [],
          holdings: portfolio.holdings.map(
            holding => {
              return {
                stockSymbol: holding.stockSymbol,
                stockDescription: holding.stockDescription,
                allocation: holding.allocation,
                quantity: holding.quantity,
                price: holding.stockPrice.price,
                currency: holding.stockPrice.currency,
                originalCurrency: holding.stockPrice.originalCurrency,
                priceAgeInDays: holding.stockPrice.ageInDays,
                valueInGbp: holding.valueInGbp,
                comment: holding.comment
              }
            }
          ),
          cashBalanceInGbp: portfolio.cashBalanceInGbp,
          totalValueInGbp: portfolio.totalValue.valueInGbp,
          allocations: portfolio.allocations,
          totalInvestmentsInGbp: portfolio.holdings.reduce((sum, holding) => {
            return sum + holding.valueInGbp;
          }, 0),
          totalPriceAgeInDays: portfolio.totalValue.totalPriceAgeInDays
        }

        this.portfolio = portfolioViewModel;
        this.changeDetectorRef.markForCheck();
      }
    });

    this.accountsService.history$.subscribe(history => {
      this.fullHistory = history;
      if (history?.items?.length) {
        this.dataStart = history.items[0].date;
        this.dataEnd = history.items[history.items.length - 1].date;
        this.rangeStart = this.dataStart;
        this.rangeEnd = this.dataEnd;
      } else {
        this.dataStart = '';
        this.dataEnd = '';
        this.rangeStart = '';
        this.rangeEnd = '';
      }
      this.applyFilter();
      this.changeDetectorRef.markForCheck();
    });

    this.accountsService.getAccounts().subscribe(accounts => {
      this.accounts = accounts;
      this.changeDetectorRef.markForCheck();
    });
  }

  private applyFilter(): void {
    if (!this.fullHistory) {
      this.filteredHistory = null;
      return;
    }
    const items = this.fullHistory.items.filter(item =>
      (!this.rangeStart || item.date >= this.rangeStart) &&
      (!this.rangeEnd || item.date <= this.rangeEnd)
    );
    const comments = this.fullHistory.comments.filter(comment =>
      (!this.rangeStart || comment.date >= this.rangeStart) &&
      (!this.rangeEnd || comment.date <= this.rangeEnd)
    );
    this.filteredHistory = { items, comments, trailingReturns: this.fullHistory.trailingReturns };
  }

  onRangeChange(): void {
    this.applyFilter();
    this.changeDetectorRef.markForCheck();
  }

  setRange(years: number | null): void {
    if (years === null) {
      this.rangeStart = this.dataStart;
    } else {
      const start = new Date();
      start.setFullYear(start.getFullYear() - years);
      const candidate = start.toISOString().substring(0, 10);
      this.rangeStart = candidate < this.dataStart ? this.dataStart : candidate;
    }
    this.rangeEnd = this.dataEnd;
    this.applyFilter();
    this.changeDetectorRef.markForCheck();
  }

  setRangeMonths(months: number): void {
    const start = new Date();
    start.setMonth(start.getMonth() - months);
    const candidate = start.toISOString().substring(0, 10);
    this.rangeStart = candidate < this.dataStart ? this.dataStart : candidate;
    this.rangeEnd = this.dataEnd;
    this.applyFilter();
    this.changeDetectorRef.markForCheck();
  }

  setRangeYtd(): void {
    const ytd = new Date(new Date().getFullYear(), 0, 1).toISOString().substring(0, 10);
    this.rangeStart = ytd < this.dataStart ? this.dataStart : ytd;
    this.rangeEnd = this.dataEnd;
    this.applyFilter();
    this.changeDetectorRef.markForCheck();
  }

  setDateToToday() {
    this.date = new Date().toISOString().substring(0, 10);
  }

  accountSelected(accountCodes: string[]) {
    this.setDateToToday();
    this.accountCodes = accountCodes;
    this.accountsService.selectAccounts(this.accountCodes);
  }

  onChartRangeSelected(event: { start: string; end: string }): void {
    this.rangeStart = event.start;
    this.rangeEnd = event.end;
    this.applyFilter();
    this.changeDetectorRef.markForCheck();
  }

  onBenchmarkChange(): void {
    if (!this.benchmarkSymbol) {
      this.benchmarkHistory = null;
      this.changeDetectorRef.markForCheck();
      return;
    }
    this.accountsService.getStockHistory(this.benchmarkSymbol, this.date).subscribe(h => {
      this.benchmarkHistory = h;
      this.changeDetectorRef.markForCheck();
    });
  }

  toggleShowQualityData() {
    this.qualityService.toggleShowQualityData();
  }
}
