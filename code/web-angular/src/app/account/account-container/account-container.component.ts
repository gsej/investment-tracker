import { Component, OnInit } from '@angular/core';
import { Account } from '../../models/account';
import { AccountsService } from '../../accounts.service';
//import { RecordedTotalValues } from '../../models/recordedTotalValues';
import { PortfolioViewModel } from '../../view-models/PortfolioViewModel';
import { AllocationViewModel } from "src/app/view-models/AllocationViewModel";
//import { AccountAnnualPerformances } from '../../models/accountAnnualPerformances';
//import { MatCardModule } from '@angular/material/card';
import { AccountSelectorComponent } from '../../account-selector/account-selector.component';
import { HoldingsComponent } from '../holdings/holdings.component';
import { SummaryComponent } from '../summary/summary.component';
import { ChartComponent } from '../chart/chart.component';
import { CardComponent } from 'src/app/components/card/card.component';
import { CardContentComponent } from 'src/app/components/card-content/card-content.component';
import { CardTitleComponent } from 'src/app/components/card-title/card-title.component';
import { HistoryViewModels } from 'src/app/view-models/HistoryViewModels';
import { HistoryComponent } from '../history/history.component';

@Component({
  selector: 'app-account-container',
  standalone: true,
  imports: [AccountSelectorComponent,
    HoldingsComponent,
    HistoryComponent,
    ChartComponent,
    SummaryComponent,
    CardComponent,
    CardContentComponent,
    CardTitleComponent],
  templateUrl: './account-container.component.html',
  styleUrls: ['./account-container.component.scss']
})
export class AccountContainerComponent implements OnInit {

  public accounts: Account[] = []
  public portfolio: PortfolioViewModel | null = null;
  public history: HistoryViewModels | null = null;
  //public recordedTotalValues: RecordedTotalValues | null = null;
  //public accountHistoricalValues: AccountHistoricalValues | null = null;
  //public accountAnnualPerformances: AccountAnnualPerformances | null = null;

  public accountCode!: string;
  public date!: string;

  constructor(private accountsService: AccountsService) {
    this.setDateToToday();
  }

  ngOnInit(): void {

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
      }
    });

    this.accountsService.history$.subscribe(history => {

      if (history == null) {
        this.history = null;
      }
      else {


        this.history = history;
      }
    });


    this.accountsService.getAccounts().subscribe(accounts => {
      this.accounts = accounts;
    });
  }

  setDateToToday() {
    this.date = new Date().toISOString().substring(0, 10);
  }

  accountSelected(accountCode: string) {

    this.setDateToToday();
    this.accountCode = accountCode;
    this.accountsService.selectAccount(accountCode);

    // this.accountHistoricalValues = {
    //   accountHistoricalValues: []
    // };


    //   this.getSummary();

    // this.accountsService.getAccountValueHistory(accountCode)
    //   .subscribe(accountHistoricalValues => {
    //     this.accountHistoricalValues = accountHistoricalValues
    //   });

    // this.accountsService.getAccountAnnualPerformance(accountCode, this.date)
    // .subscribe(accountAnnualPerformances => {
    //   this.accountAnnualPerformances = accountAnnualPerformances
    // });
  }

  // getSummary() {
  //   this.accountsService.getAccountSummary(this.accountCode, this.date)
  //     .subscribe(summary => {
  //       this.accountSummary = {
  //         holdings: summary.holdings.map(
  //           holding => {
  //             return {
  //               stockSymbol: holding.stockSymbol,
  //               stockDescription: holding.stockDescription,
  //               quantity: holding.quantity,
  //               price: holding.stockPrice.price,
  //               currency: holding.stockPrice.currency,
  //               originalCurrency: holding.stockPrice.originalCurrency,
  //               priceAgeInDays: holding.stockPrice.ageInDays,
  //               valueInGbp: holding.valueInGbp,
  //               comment: holding.comment
  //             }
  //           }
  //         ),
  //         cashBalanceInGbp: summary.cashBalanceInGbp,
  //         totalValueInGbp: summary.totalValue.valueInGbp,
  //         totalPriceAgeInDays: summary.totalValue.totalPriceAgeInDays
  //       }
  //     });
  // }

  dateSelected(date: string) {
    console.log("container, date selected: " + date);

    this.date = date;
    //  this.getSummary();
  }
}
