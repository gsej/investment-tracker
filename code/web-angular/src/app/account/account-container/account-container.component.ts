import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Account } from '../../models/account';
import { AccountsService } from '../../accounts.service';
import { PortfolioViewModel } from '../../view-models/PortfolioViewModel';
import { AccountSelectorComponent } from '../../account-selector/account-selector.component';
import { BenchmarkSelectorComponent } from '../../benchmark-selector/benchmark-selector.component';
import { DateRangeSelectorComponent } from '../../date-range-selector/date-range-selector.component';
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
import { CardContentComponent, CardTitleComponent } from '@gsej/tailwind-components';

@Component({
  selector: 'app-account-container',
  standalone: true,
  imports: [
    CommonModule,
    AccountSelectorComponent,
    BenchmarkSelectorComponent,
    DateRangeSelectorComponent,
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

  public filteredHistory: HistoryViewModels | null = null;

  public accountCodes: string[] = [];

  public date!: string;

  public chartRange: { start: string; end: string } | null = null;
  public dateRange: { start: string; end: string } = { start: '', end: '' };

  public benchmarkHistory: StockHistoryViewModel | null = null;
  public availableStocks: { symbol: string; description: string }[] = [];

  showQualityData$!: Observable<boolean>;

  constructor(
    private changeDetectorRef: ChangeDetectorRef,
    private accountsService: AccountsService,
    private qualityService: QualityService) {
    this.setDateToToday();
  }

  ngOnInit(): void {

    this.showQualityData$ = this.qualityService.showQualityData$;

    this.accountsService.getStocks().subscribe(stocks => {
      this.availableStocks = stocks.filter(s => s.benchmark);
      this.changeDetectorRef.markForCheck();
    });

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

    this.accountsService.filteredHistory$.subscribe(history => {
      this.filteredHistory = history;
      this.changeDetectorRef.markForCheck();
    });

    this.accountsService.dateRange$.subscribe(range => {
      this.dateRange = range;
      this.changeDetectorRef.markForCheck();
    });

    this.accountsService.getAccounts().subscribe(accounts => {
      this.accounts = accounts;
      this.changeDetectorRef.markForCheck();
    });
  }

  onRangeChanged(range: { start: string; end: string }): void {
    this.accountsService.setDateRange(range);
  }

  setDateToToday() {
    this.date = new Date().toISOString().substring(0, 10);
  }

  accountSelected(accountCodes: string[]) {
    this.setDateToToday();
    this.accountCodes = accountCodes;
    this.chartRange = null;
    this.accountsService.selectAccounts(this.accountCodes);
  }

  onChartRangeSelected(event: { start: string; end: string }): void {
    this.accountsService.setDateRange({ ...event });
    this.chartRange = { ...event };
  }

  onBenchmarkChanged(symbol: string): void {
    if (!symbol) {
      this.benchmarkHistory = null;
      this.changeDetectorRef.markForCheck();
      return;
    }
    this.accountsService.getStockHistory(symbol, this.date).subscribe(h => {
      this.benchmarkHistory = h;
      this.changeDetectorRef.markForCheck();
    });
  }

  toggleShowQualityData() {
    this.qualityService.toggleShowQualityData();
  }
}
