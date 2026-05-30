import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent, CardContentComponent, CardHeaderComponent, CardTitleComponent, SeparatorComponent } from '@gsej/tailwind-components';
import { TrailingReturnViewModel } from 'src/app/view-models/TrailingReturnViewModel';

@Component({
  selector: 'app-trailing-returns',
  standalone: true,
  imports: [CommonModule, CardComponent, CardContentComponent, CardHeaderComponent, CardTitleComponent, SeparatorComponent],
  templateUrl: './trailing-returns.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TrailingReturnsComponent {

  @Input() trailingReturns: TrailingReturnViewModel[] | null = null;
  @Input() benchmarkTrailingReturns: TrailingReturnViewModel[] | null = null;
  @Input() benchmarkName: string | null = null;

  formatReturn(value: number | null): string {
    if (value === null) return '—';
    const pct = value * 100;
    const sign = pct >= 0 ? '+' : '';
    return `${sign}${pct.toFixed(2)}%`;
  }

  benchmarkReturnFor(periodLabel: string): number | null {
    return this.benchmarkTrailingReturns?.find(r => r.periodLabel === periodLabel)?.returnPercentage ?? null;
  }
}
