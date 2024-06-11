import { Component, OnInit } from '@angular/core';
import { Account } from '../models/account';
import { AccountsService } from '../accounts.service';
import { RecordedTotalValues } from '../models/recordedTotalValues';
import { AccountHistoricalValues } from '../models/accountHistoricalValues';
import { AccountSummaryViewModel } from '../view-models/accountSummaryViewModel';

@Component({
  selector: 'app-account-container',
  templateUrl: './account-container.component.html',
  styleUrls: ['./account-container.component.scss']
})
export class AccountContainerComponent implements OnInit {

  public accounts: Account[] = []
  public accountSummary: AccountSummaryViewModel | null = null;
  public recordedTotalValues: RecordedTotalValues | null = null;
  public accountHistoricalValues: AccountHistoricalValues | null = null;

  public accountCode!: string;
  public date!: string;

  constructor(private accountsService: AccountsService) {
    this.setDateToToday();
  }


  ngOnInit(): void {
    this.accountsService.getAccounts().subscribe(accounts => {
      this.accounts = accounts;
    })
  }

  setDateToToday() {
    this.date = new Date().toISOString().substring(0, 10);

  }

  accountSelected(accountCode: string) {

    this.setDateToToday();

    this.accountCode = accountCode;

    this.getSummary();

    this.accountsService.getAccountValueHistory(accountCode)
      .subscribe(accountHistoricalValues => {
        this.accountHistoricalValues = accountHistoricalValues
      });
  }

  getSummary() {
    this.accountsService.getAccountSummary(this.accountCode, this.date)
      .subscribe(summary => {

        this.accountSummary = {
          holdings: summary.holdings.map(
            holding => {
              return {
                stockSymbol: holding.stockSymbol,
                stockDescription: holding.stockDescription,
                quantity: holding.quantity,
                price: holding.stockPrice.price,
                currency: holding.stockPrice.currency,
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

      });
  }

  dateSelected(date: string) {
    console.log("container, date selected: " + date);

    this.date = date;
    this.getSummary();
  }
}
