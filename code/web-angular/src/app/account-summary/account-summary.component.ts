import { Component, Input } from '@angular/core';
import { AccountSummaryViewModel } from '../view-models/accountSummaryViewModel';
import { HoldingViewModel } from '../view-models/holdingViewModel';

@Component({
  selector: 'app-account-summary',
  templateUrl: './account-summary.component.html',
  styleUrls: ['./account-summary.component.scss']
})
export class AccountSummaryComponent {

  public displayedColumns = [
    'stockSymbol',
    'stockDescription',
    'quantity', 'price',
    'currency',
    'ageInDays',
    'valueInGbp',
    'comment'];

  @Input()
  public accountSummary: AccountSummaryViewModel | null = null;

  private _sortAscending = true;
  private _sortColumn: keyof HoldingViewModel = 'stockDescription';

  public sort(column: keyof HoldingViewModel) {

    console.log(`sorting by ${this._sortColumn}`);

    this._sortAscending = !this._sortAscending;
    this._sortColumn = column;

    if (this.accountSummary) {
      let holdings = [...this.accountSummary.holdings];

      if (this._sortAscending) {
        holdings.sort((a, b) => {
          if (typeof a[this._sortColumn] === 'number') {
            return Number(a[this._sortColumn]) - Number(b[this._sortColumn]);
          } else {
            return String(a[this._sortColumn]).localeCompare(String(b[this._sortColumn]));
          }
        });
      } else {
        holdings.sort((a, b) => {
          if (typeof a[this._sortColumn] === 'number') {
            return Number(b[this._sortColumn]) - Number(a[this._sortColumn]);
          } else {
            return String(b[this._sortColumn]).localeCompare(String(a[this._sortColumn]));

          }
        });
      }

      this.accountSummary.holdings = holdings;
    }
  }
}
