import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-date-range-selector',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './date-range-selector.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DateRangeSelectorComponent implements OnChanges {

  @Input() overrideRange: { start: string; end: string } | null = null;

  @Output() rangeChanged = new EventEmitter<{ start: string; end: string }>();

  public rangeStart: string = '';
  public rangeEnd: string = '';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['overrideRange'] && this.overrideRange) {
      this.rangeStart = this.overrideRange.start;
      this.rangeEnd = this.overrideRange.end;
    }
  }

  onRangeChange(): void {
    this.emitRange();
  }

  setRange(years: number | null): void {
    if (years === null) {
      this.rangeStart = '';
      this.rangeEnd = '';
    } else {
      this.rangeStart = this.dateMinusYears(years);
      this.rangeEnd = this.today();
    }
    this.emitRange();
  }

  setRangeMonths(months: number): void {
    this.rangeStart = this.dateMinusMonths(months);
    this.rangeEnd = this.today();
    this.emitRange();
  }

  setRangeYtd(): void {
    this.rangeStart = new Date(new Date().getFullYear(), 0, 1).toISOString().substring(0, 10);
    this.rangeEnd = this.today();
    this.emitRange();
  }

  isRangeActive(years: number | null): boolean {
    if (years === null) return this.rangeStart === '' && this.rangeEnd === '';
    return this.rangeStart === this.dateMinusYears(years) && this.rangeEnd === this.today();
  }

  isMonthRangeActive(months: number): boolean {
    return this.rangeStart === this.dateMinusMonths(months) && this.rangeEnd === this.today();
  }

  isYtdActive(): boolean {
    const ytd = new Date(new Date().getFullYear(), 0, 1).toISOString().substring(0, 10);
    return this.rangeStart === ytd && this.rangeEnd === this.today();
  }

  private today(): string {
    return new Date().toISOString().substring(0, 10);
  }

  private dateMinusYears(years: number): string {
    const d = new Date();
    d.setFullYear(d.getFullYear() - years);
    return d.toISOString().substring(0, 10);
  }

  private dateMinusMonths(months: number): string {
    const d = new Date();
    d.setMonth(d.getMonth() - months);
    return d.toISOString().substring(0, 10);
  }

  private emitRange(): void {
    this.rangeChanged.emit({ start: this.rangeStart, end: this.rangeEnd });
  }
}
