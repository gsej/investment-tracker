import { Component, Input } from '@angular/core';
import { AccountSummary } from '../models/accountSummary';

@Component({
  selector: 'app-account-summary',
  templateUrl: './account-summary.component.html',
  styleUrls: ['./account-summary.component.scss']
})
export class AccountSummaryComponent {

  public displayedColumns = ['stockSymbol', 'stockDescription', 'quantity', 'price', 'currency', 'ageInDays', 'valueInGbp'];

  @Input()
  public accountSummary: AccountSummary | null = null;
}
