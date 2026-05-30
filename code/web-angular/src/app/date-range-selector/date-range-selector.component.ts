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

  @Input() dataStart: string = '';
  @Input() dataEnd: string = '';
  @Input() overrideRange: { start: string; end: string } | null = null;

  @Output() rangeChanged = new EventEmitter<{ start: string; end: string }>();

  public rangeStart: string = '';
  public rangeEnd: string = '';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['dataStart'] || changes['dataEnd']) {
      this.rangeStart = '';
      this.rangeEnd = '';
      this.emitRange();
    }
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
      this.rangeEnd = this.dataEnd;
    }
    this.emitRange();
  }

  setRangeMonths(months: number): void {
    this.rangeStart = this.dateMinusMonths(months);
    this.rangeEnd = this.dataEnd;
    this.emitRange();
  }

  setRangeYtd(): void {
    this.rangeStart = new Date(new Date().getFullYear(), 0, 1).toISOString().substring(0, 10);
    this.rangeEnd = this.dataEnd;
    this.emitRange();
  }

  isRangeActive(years: number | null): boolean {
    if (years === null) return this.rangeStart === '' && this.rangeEnd === '';
    if (this.rangeEnd !== this.dataEnd) return false;
    return this.rangeStart === this.dateMinusYears(years);
  }

  isMonthRangeActive(months: number): boolean {
    if (this.rangeEnd !== this.dataEnd) return false;
    return this.rangeStart === this.dateMinusMonths(months);
  }

  isYtdActive(): boolean {
    if (this.rangeEnd !== this.dataEnd) return false;
    const ytd = new Date(new Date().getFullYear(), 0, 1).toISOString().substring(0, 10);
    return this.rangeStart === ytd;
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
