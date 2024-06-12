import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccountHistoricalValues } from '../models/accountHistoricalValues';
import { AccountHistoricalValue } from '../models/accountHistoricalValue';

@Component({
  selector: 'app-account-value-history',
  templateUrl: './account-value-history.component.html',
  styleUrls: ['./account-value-history.component.scss']
})
export class AccountValueHistoryComponent implements OnInit {

  public displayedColumns = [
    'date',
    'accountCode',
    'valueInGbp',
    'totalPriceAgeInDays',
    'recordedTotalValueInGbp',
    'discrepancyPercentage',
    'differenceToPreviousDay',
    'differencePercentage',
    'comment',
    'controls'];

  @Input()
  public accountHistoricalValues: AccountHistoricalValues = {
    accountHistoricalValues: []
  };

  public accountHistoricalValuesToShow: AccountHistoricalValue[] = [];

  @Output()
  public dateSelected: EventEmitter<string> = new EventEmitter<string>();


  private showRecordedOnly = false;
  private showBigDifferenceOnly = false;

  ngOnInit(): void {
    this.accountHistoricalValuesToShow = [...this.accountHistoricalValues.accountHistoricalValues];
  }

  public recordedOnly($event: any) {

    this.showRecordedOnly = $event.checked;
    this.filterRows();
  }

  public bigDifferenceOnly($event: any) {
    this.showBigDifferenceOnly = $event.checked;
    this.filterRows();
  }

  filterRows() {

    let rowsToShow = [...this.accountHistoricalValues.accountHistoricalValues];

    if (this.showBigDifferenceOnly) {
      rowsToShow = [...rowsToShow.filter(v => Math.abs(v.differencePercentage) > 1)];
    }

    if (this.showRecordedOnly) {
      rowsToShow = [...rowsToShow.filter(v => v.recordedTotalValueInGbp !== null)];
    }

    this.accountHistoricalValuesToShow = rowsToShow;
  }

  public selectDate($event: any) {
    console.log($event);
    this.dateSelected.next($event);
  }
}
