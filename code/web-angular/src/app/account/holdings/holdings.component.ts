import { Component, Input } from '@angular/core';
import { PortfolioViewModel } from '../../view-models/PortfolioViewModel';
import { HoldingViewModel } from '../../view-models/HoldingViewModel';
import { DomSanitizer } from '@angular/platform-browser';
//import { SortIndicatorComponent } from '../../sort-indicator/sort-indicator.component';
import { CommonModule } from '@angular/common';
import { HeaderComponent } from 'src/app/components/header/header.component';
import { CardComponent } from 'src/app/components/card/card.component';
import { CardHeaderComponent } from 'src/app/components/card-header/card-header.component';
import { CardContentComponent } from 'src/app/components/card-content/card-content.component';
import { formatQuantity, formatCurrency } from 'src/app/utils/formatters';
import { SeparatorComponent } from 'src/app/components/separator/separator.component';

@Component({
  selector: 'app-holdings',
  standalone: true,
  imports: [
    CommonModule,
   // SortIndicatorComponent,
    HeaderComponent,
    CardComponent,
    CardHeaderComponent,
    CardContentComponent,
    SeparatorComponent
  ],
  templateUrl: './holdings.component.html',
  styleUrls: ['./holdings.component.scss']
})
export class HoldingsComponent {

  public formatQuantity = formatQuantity;
  public formatCurrency = formatCurrency;

  constructor(sanitizer: DomSanitizer) {

  //   iconRegistry.addSvgIcon(
  //     'thumbs-up',
  //     sanitizer.bypassSecurityTrustResourceUrl('assets/img/examples/thumbup-icon.svg'));
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
}
