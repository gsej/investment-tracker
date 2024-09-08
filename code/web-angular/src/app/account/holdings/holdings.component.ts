import { Component, Input } from '@angular/core';
import { PortfolioViewModel } from '../../view-models/portfolioViewModel';
import { HoldingViewModel } from '../../view-models/holdingViewModel';
import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';
import { SortIndicatorComponent } from '../../sort-indicator/sort-indicator.component';
import { CommonModule } from '@angular/common';
import { AccountsService } from '../../accounts.service';
import { HeaderComponent } from 'src/app/components/header/header.component';

@Component({
  selector: 'app-holdings',
  standalone: true,
  imports: [CommonModule, SortIndicatorComponent, HeaderComponent],
  templateUrl: './holdings.component.html',
  styleUrls: ['./holdings.component.scss']
})
export class HoldingsComponent {

  private _accountsService: AccountsService;

  constructor(iconRegistry: MatIconRegistry, sanitizer: DomSanitizer, accountService: AccountsService) {

    this._accountsService = accountService;
    iconRegistry.addSvgIcon(
      'thumbs-up',
      sanitizer.bypassSecurityTrustResourceUrl('assets/img/examples/thumbup-icon.svg'));
  }

  @Input()
  public portfolio: PortfolioViewModel | null = null;

  @Input()
  public date!: string;

  public sortAscending = true;
  public sortColumn: keyof HoldingViewModel = 'stockDescription';

  public sort(column: keyof HoldingViewModel) {

    console.log(`sorting by ${this.sortColumn}`);

    this.sortAscending = !this.sortAscending;
    this.sortColumn = column;

    if (this.portfolio) {
      let holdings = [...this.portfolio.holdings];

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

      this.portfolio.holdings = holdings;
    }
  }

  public formatQuantity(value: number | null) {
    if (value === null) {
      return '';
    }

    return new Intl.NumberFormat('en-US', {
      maximumFractionDigits: 2
    }).format(value);
  }

  public formatCurrency(value: number | null) {
    if (value === null) {
      return '';
    }

    return new Intl.NumberFormat('en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);
  }
}
