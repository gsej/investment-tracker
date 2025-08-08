import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { PortfolioViewModel } from '../../view-models/PortfolioViewModel';
import { HoldingViewModel } from '../../view-models/HoldingViewModel';
import { DomSanitizer } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { formatQuantity, formatCurrency } from 'src/app/utils/formatters';
import { Observable } from 'rxjs';
import { QualityService } from 'src/app/quality.service';
import { CardComponent, CardContentComponent, CardHeaderComponent, SeparatorComponent } from '@gsej/tailwind-components';

@Component({
  selector: 'app-holdings',
  standalone: true,
  imports: [
    CommonModule,
    CardComponent,
    CardHeaderComponent,
    CardContentComponent,
    SeparatorComponent
  ],
  templateUrl: './holdings.component.html',
  styleUrls: ['./holdings.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HoldingsComponent {

  public formatQuantity = formatQuantity;
  public formatCurrency = formatCurrency;

  showQualityData$!: Observable<boolean>;

  constructor(qualityService: QualityService, sanitizer: DomSanitizer) {
    this.showQualityData$ = qualityService.showQualityData$;
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
