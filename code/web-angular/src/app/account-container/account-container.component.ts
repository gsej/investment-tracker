import { Component, OnInit } from '@angular/core';
import { Account } from '../models/account';
import { AccountsService } from '../accounts.service';
import { AccountSummary } from '../models/accountSummary';
import { RecordedTotalValues } from '../models/recordedTotalValues';
import { AccountHistoricalValues } from '../models/accountHistoricalValues';

@Component({
  selector: 'app-account-container',
  templateUrl: './account-container.component.html',
  styleUrls: ['./account-container.component.scss']
})
export class AccountContainerComponent implements OnInit {


  public accounts: Account[] = []
  public accountSummary: AccountSummary | null = null;
  public recordedTotalValues: RecordedTotalValues | null = null;
  public accountHistoricalValues: AccountHistoricalValues | null = null;

  constructor(private accountsService: AccountsService) {
  }


  ngOnInit(): void {
    this.accountsService.getAccounts().subscribe(accounts => {
      this.accounts = accounts;
    })
  }

  accountsSelected(accountCodes: string[]) {
    this.accountsService.getAccountSummary(accountCodes)
      .subscribe(summary => {
        this.accountSummary = summary

        this.accountSummary.holdings.push({
          stockSymbol: "£",
          stockDescription: "Cash Balance",
          quantity: this.accountSummary.cashBalanceInGbp,
          stockPrice: {
            currency: "GBP",
            price: 1,
            ageInDays: 0
          },
          valueInGbp: this.accountSummary.cashBalanceInGbp,
          comment: ""
        });

      });

    if (accountCodes.length === 1) {
      this.accountsService.getRecordedTotalValues(accountCodes[0])
        .subscribe(recordedTotalValues => {
          this.recordedTotalValues = recordedTotalValues;
        });
    }
    else {
      this.recordedTotalValues = null;
    }

    if (accountCodes.length === 1) {
      this.accountsService.getAccountValueHistory(accountCodes[0])
        .subscribe(accountHistoricalValues => {
          this.accountHistoricalValues = accountHistoricalValues
        });
    }
    else {
      this.accountHistoricalValues = { accountHistoricalValues:[]};
    }
  }
}
