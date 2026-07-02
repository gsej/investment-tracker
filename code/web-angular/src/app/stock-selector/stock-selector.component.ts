import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-stock-selector',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './stock-selector.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StockSelectorComponent {

  @Input() availableStocks: { symbol: string; description: string }[] = [];

  @Output() stocksChanged = new EventEmitter<string[]>();

  public selectedSymbols: string[] = [];
  public pendingSymbol: string = '';

  addStock(symbol: string): void {
    if (symbol && !this.selectedSymbols.includes(symbol)) {
      this.selectedSymbols = [...this.selectedSymbols, symbol];
      this.stocksChanged.emit(this.selectedSymbols);
    }
    this.pendingSymbol = '';
  }

  removeStock(symbol: string): void {
    this.selectedSymbols = this.selectedSymbols.filter(s => s !== symbol);
    this.stocksChanged.emit(this.selectedSymbols);
  }

  get unselectedStocks(): { symbol: string; description: string }[] {
    return this.availableStocks.filter(s => !this.selectedSymbols.includes(s.symbol));
  }
}
