import { Component, Input } from '@angular/core';
import { AccountSummary } from '../models/accountSummary';
import { Holding } from '../models/holding';

@Component({
  selector: 'app-account-summary',
  templateUrl: './account-summary.component.html',
  styleUrls: ['./account-summary.component.scss']
})
export class AccountSummaryComponent {

  public displayedColumns = ['stockSymbol', 'stockDescription', 'quantity', 'price', 'currency', 'ageInDays', 'valueInGbp'];

  @Input()
  public accountSummary: AccountSummary | null = null;

  private _sortAscending = true;
  private _sortColumn: keyof Holding = 'stockDescription';

  public sort(column: keyof Holding) {

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
