import { Component, Input } from '@angular/core';
import { CommonModule, formatPercent } from '@angular/common';
import { PortfolioViewModel } from 'src/app/view-models/PortfolioViewModel';
import { formatQuantity, formatCurrency, formatPercentage } from 'src/app/utils/formatters';
import { CardComponent, CardContentComponent, CardHeaderComponent, CardTitleComponent, SeparatorComponent } from '@gsej/tailwind-components';

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
