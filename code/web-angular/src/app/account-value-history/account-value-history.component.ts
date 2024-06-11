import { Component, EventEmitter, Input, Output } from '@angular/core';
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
    'comment',
    'controls'];

  @Input()
  public accountHistoricalValues: AccountHistoricalValues | null = null;

  @Output()
  public dateSelected: EventEmitter<string> = new EventEmitter<string>();

  public selectDate($event :any) {
    console.log($event);
    this.dateSelected.next($event);
  }
}
