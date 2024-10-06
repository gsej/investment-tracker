import { Component, Input } from '@angular/core';

import { CommonModule, formatPercent } from '@angular/common';
import { CardComponent } from 'src/app/components/card/card.component';
import { CardTitleComponent } from 'src/app/components/card-title/card-title.component';
import { CardContentComponent } from 'src/app/components/card-content/card-content.component';
import { CardHeaderComponent } from 'src/app/components/card-header/card-header.component';
import { SeparatorComponent } from 'src/app/components/separator/separator.component';
import { PortfolioViewModel } from 'src/app/view-models/PortfolioViewModel';
import { formatQuantity, formatCurrency, formatPercentage } from 'src/app/utils/formatters';

@Component({
  selector: 'app-summary',
  standalone: true,
  imports: [CommonModule, CardComponent, CardTitleComponent, CardContentComponent, CardHeaderComponent, SeparatorComponent],
  templateUrl: './summary.component.html',
  styleUrls: ['./summary.component.scss']
})
export class SummaryComponent {

  public formatQuantity = formatQuantity;
  public formatCurrency = formatCurrency;
  public formatPercentage = formatPercentage;

  @Input()
  public portfolio: PortfolioViewModel | null = null;

}
