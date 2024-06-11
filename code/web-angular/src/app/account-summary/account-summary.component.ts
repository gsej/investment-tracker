import { Component, Input } from '@angular/core';
import { AccountSummaryViewModel } from '../view-models/accountSummaryViewModel';
import { HoldingViewModel } from '../view-models/holdingViewModel';
import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
  selector: 'app-account-summary',
  templateUrl: './account-summary.component.html',
  styleUrls: ['./account-summary.component.scss']
})
export class AccountSummaryComponent {

   constructor(iconRegistry: MatIconRegistry, sanitizer: DomSanitizer) {
    iconRegistry.addSvgIcon(
        'thumbs-up',
        sanitizer.bypassSecurityTrustResourceUrl('assets/img/examples/thumbup-icon.svg'));
  }

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

  @Input()
  public date!: string;

  public sortAscending = true;
  public sortColumn: keyof HoldingViewModel = 'stockDescription';

  public sort(column: keyof HoldingViewModel) {

    console.log(`sorting by ${this.sortColumn}`);

    this.sortAscending = !this.sortAscending;
    this.sortColumn = column;

    if (this.accountSummary) {
      let holdings = [...this.accountSummary.holdings];

      if (this.sortAscending) {
        holdings.sort((a, b) => {
          if (typeof a[this.sortColumn] === 'number') {
            return Number(a[this.sortColumn]) - Number(b[this.sortColumn]);
          } else {
            return String(a[this.sortColumn]).localeCompare(String(b[this.sortColumn]));
          }
        });
      } else {
        holdings.sort((a, b) => {
          if (typeof a[this.sortColumn] === 'number') {
            return Number(b[this.sortColumn]) - Number(a[this.sortColumn]);
          } else {
            return String(b[this.sortColumn]).localeCompare(String(a[this.sortColumn]));

          }
        });
      }

      this.accountSummary.holdings = holdings;
    }
  }
}
