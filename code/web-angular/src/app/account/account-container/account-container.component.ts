import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Account } from '../../models/account';
import { AccountsService } from '../../accounts.service';
import { PortfolioViewModel } from '../../view-models/PortfolioViewModel';
import { AccountSelectorComponent } from '../../account-selector/account-selector.component';
import { HoldingsComponent } from '../holdings/holdings.component';
import { SummaryComponent } from '../summary/summary.component';
import { HistoryViewModels } from 'src/app/view-models/HistoryViewModels';
import { HistoryComponent } from '../history/history.component';
import { HistoryChartComponent } from '../chart/history-chart.component';
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
    HoldingsComponent,
    HistoryComponent,
    HistoryChartComponent,
    SummaryComponent,
    CardContentComponent,
    CardTitleComponent],
  templateUrl: './account-container.component.html',
  styleUrls: ['./account-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AccountContainerComponent implements OnInit {

  public accounts: Account[] = []
  public portfolio: PortfolioViewModel | null = null;
  public history: HistoryViewModels | null = null;

  public accountCode!: string;
  public date!: string;

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
          accountCode: portfolio.accountCode,
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
      if (history == null) {
        this.history = null;
      }
      else {
        this.history = history;
      }
      this.changeDetectorRef.markForCheck();
    });

    this.accountsService.getAccounts().subscribe(accounts => {
      this.accounts = accounts;
      this.changeDetectorRef.markForCheck();
    });
  }

  setDateToToday() {
    this.date = new Date().toISOString().substring(0, 10);
  }

  accountSelected(accountCodes: string[]) {

    this.setDateToToday();
    this.accountCode = accountCodes[0];
    this.accountsService.selectAccount(this.accountCode);
  }


  dateSelected(date: string) {
    console.log("container, date selected: " + date);
// TODO: reenebale this
    this.date = date;
    //  this.getSummary();
  }


  toggleShowQualityData() {
    this.qualityService.toggleShowQualityData();
  }
}
