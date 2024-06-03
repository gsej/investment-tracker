import { Component, Input } from '@angular/core';
import { AccountHistoricalValues } from '../models/accountHistoricalValues';

@Component({
  selector: 'app-account-value-history',
  templateUrl: './account-value-history.component.html',
  styleUrls: ['./account-value-history.component.scss']
})
export class AccountValueHistoryComponent {

  public displayedColumns = [
    'date',
    'accountCode',
    'valueInGbp',
    'totalPriceAgeInDays',
    'recordedTotalValueInGbp',
    'discrepancyPercentage',
    'comment'];

  @Input()
  public accountHistoricalValues: AccountHistoricalValues | null = null;
}
