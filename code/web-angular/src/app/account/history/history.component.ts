import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { PortfolioViewModel } from '../../view-models/PortfolioViewModel';
import { HoldingViewModel } from '../../view-models/HoldingViewModel';
//import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';
//import { SortIndicatorComponent } from '../../sort-indicator/sort-indicator.component';
import { CommonModule, formatPercent } from '@angular/common';
import { AccountsService } from '../../accounts.service';
import { HeaderComponent } from 'src/app/components/header/header.component';
import { CardComponent } from 'src/app/components/card/card.component';
import { CardHeaderComponent } from 'src/app/components/card-header/card-header.component';
import { CardContentComponent } from 'src/app/components/card-content/card-content.component';
import { formatQuantity, formatCurrency, formatPercentage } from 'src/app/utils/formatters';
import { SeparatorComponent } from 'src/app/components/separator/separator.component';
import { HistoryViewModels } from 'src/app/view-models/HistoryViewModels';
import { HistoryViewModel } from 'src/app/view-models/HistoryViewModel';
import { Observable } from 'rxjs';
import { QualityService } from 'src/app/quality.service';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [
    CommonModule,
    //SortIndicatorComponent,
    HeaderComponent,
    CardComponent,
    CardHeaderComponent,
    CardContentComponent,
    SeparatorComponent
  ],
  templateUrl: './history.component.html',
  styleUrls: ['./history.component.scss']
})
export class HistoryComponent implements OnChanges {


  public formatQuantity = formatQuantity;
  public formatCurrency = formatCurrency;
  public formatPercentage = formatPercentage;


  private showRecordedOnly = false;
  private showBigDifferenceOnly = false;

  showQualityData$!: Observable<boolean>;

  constructor(qualityService: QualityService, sanitizer: DomSanitizer) {

    this.showQualityData$ = qualityService.showQualityData$;

    //this._accountsService = accountService;
    // iconRegistry.addSvgIcon(
    //   'thumbs-up',
    //   sanitizer.bypassSecurityTrustResourceUrl('assets/img/examples/thumbup-icon.svg'));
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.history) {
      this.itemsToShow = [...this.history.items];
    }

  }

  @Input()
  public history: HistoryViewModels | null = { items: [] };

  @Input()
  public date!: string;

  public sortAscending = true;
  public sortColumn: keyof HistoryViewModel = 'date';

  public itemsToShow: HistoryViewModel[] = [];

  public sort(column: keyof HistoryViewModel) {

    console.log(`sorting by ${this.sortColumn}`);

    this.sortAscending = !this.sortAscending;
    this.sortColumn = column;

    if (this.history) {
      let items = [...this.history.items];

      if (this.sortAscending) {
        items.sort((a, b) => {
          if (typeof a[this.sortColumn] === 'number') {
            return Number(a[this.sortColumn]) - Number(b[this.sortColumn]);
          } else {
            return String(a[this.sortColumn]).localeCompare(String(b[this.sortColumn]));
          }
        });
      } else {
        items.sort((a, b) => {
          if (typeof a[this.sortColumn] === 'number') {
            return Number(b[this.sortColumn]) - Number(a[this.sortColumn]);
          } else {
            return String(b[this.sortColumn]).localeCompare(String(a[this.sortColumn]));

          }
        });
      }

      this.history.items = items;
    }
  }


  public recordedOnly($event: any) {

    this.showRecordedOnly = $event.currentTarget.checked;
    this.filterRows();
  }

  public bigDifferenceOnly($event: any) {
    this.showBigDifferenceOnly = $event.currentTarget.checked;
    this.filterRows();
  }

  filterRows() {


    let itemsToShow = [];

    if (this.history) {


      itemsToShow = [...this.history.items];

      if (this.showBigDifferenceOnly) {
        itemsToShow = [...itemsToShow.filter(v => Math.abs(v.differenceRatio) > 0.01)];
      }

      if (this.showRecordedOnly) {
        itemsToShow = [...itemsToShow.filter(v => v.recordedTotalValueInGbp !== null)];
      }

      this.itemsToShow = itemsToShow;
    }
  }

}
