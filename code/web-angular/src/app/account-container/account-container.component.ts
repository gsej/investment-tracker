import { Component, OnInit } from '@angular/core';
import { Account } from '../models/account';
import { AccountsService } from '../accounts.service';
import { RecordedTotalValues } from '../models/recordedTotalValues';
import { AccountHistoricalValues } from '../models/accountHistoricalValues';
import { PortfolioViewModel } from '../view-models/portfolioViewModel';
import { AccountAnnualPerformances } from '../models/accountAnnualPerformances';
import { MatCardModule } from '@angular/material/card';
import { AccountSelectorComponent } from '../account-selector/account-selector.component';

import { PortfolioComponent } from '../portfolio/portfolio.component';

@Component({
  selector: 'app-account-container',
  standalone: true,
  imports: [MatCardModule, AccountSelectorComponent, PortfolioComponent],
  templateUrl: './account-container.component.html',
  styleUrls: ['./account-container.component.scss']
})
export class AccountContainerComponent implements OnInit {

  public accounts: Account[] = []
  public portfolio: PortfolioViewModel | null = null;
  public recordedTotalValues: RecordedTotalValues | null = null;
  public accountHistoricalValues: AccountHistoricalValues | null = null;
  public accountAnnualPerformances: AccountAnnualPerformances | null = null;

  public accountCode!: string;
  public date!: string;

  constructor(private accountsService: AccountsService) {
    this.setDateToToday();
  }

  ngOnInit(): void {

    this.accountsService.portfolio$.subscribe(summary => {

      if (summary == null) {
        this.portfolio = null;
      }
      else {
        this.portfolio = {
          accountCode: summary.accountCode,
          holdings: summary.holdings.map(
            holding => {
              return {
                stockSymbol: holding.stockSymbol,
                stockDescription: holding.stockDescription,
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
          cashBalanceInGbp: summary.cashBalanceInGbp,
          totalValueInGbp: summary.totalValue.valueInGbp,
          totalPriceAgeInDays: summary.totalValue.totalPriceAgeInDays
        }
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
