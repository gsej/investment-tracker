import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-benchmark-selector',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './benchmark-selector.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BenchmarkSelectorComponent {

  @Input() availableStocks: { symbol: string; description: string }[] = [];

  @Output() benchmarkChanged = new EventEmitter<string>();

  public selectedSymbol: string = '';

  onSelectionChange(): void {
    this.benchmarkChanged.emit(this.selectedSymbol);
  }
}
