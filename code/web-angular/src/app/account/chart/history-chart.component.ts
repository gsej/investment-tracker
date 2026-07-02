import { Component, Input, Output, EventEmitter, OnChanges, OnDestroy, SimpleChanges, ChangeDetectionStrategy, ChangeDetectorRef, NgZone, HostListener } from '@angular/core';
import Chart from 'chart.js/auto';
import annotationPlugin from 'chartjs-plugin-annotation';
import { HistoryViewModels } from 'src/app/view-models/HistoryViewModels';
import { CommentViewModel } from 'src/app/view-models/CommentViewModel';
import { StockHistoryViewModel, StockPriceViewModel } from 'src/app/view-models/StockHistoryViewModel';
import { FormsModule } from '@angular/forms';
import { FormLabelComponent } from '@gsej/tailwind-components';

Chart.register(annotationPlugin);

@Component({
  selector: 'app-history-chart',
  standalone: true,
  imports: [FormsModule, FormLabelComponent],
  templateUrl: './history-chart.component.html',
  styleUrl: './history-chart.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HistoryChartComponent implements OnChanges, OnDestroy {

  public chart: any;

  private dates: string[] = [];
  private values: number[] = [];
  private stockDatasets: { symbol: string; values: number[] }[] = [];

  private readonly stockColors = [
    'hsl(200, 70%, 55%)',
    'hsl(340, 70%, 55%)',
    'hsl(120, 50%, 45%)',
    'hsl(30, 80%, 55%)',
    'hsl(270, 60%, 60%)',
  ];

  public chartType = 'valueInGbp';
  private label = '';

  @Output() rangeSelected = new EventEmitter<{ start: string; end: string }>();

  private dragStartX: number | null = null;
  private dragCurrentX: number | null = null;
  private isDragging = false;
  private canvasMouseDownHandler: ((e: MouseEvent) => void) | null = null;

  public activeCommentIndex: number | null = null;
  public visibleComments: CommentViewModel[] = [];
  private visibleCommentAnnotationKeys: string[] = [];

  get activeComment(): CommentViewModel | null {
    if (this.activeCommentIndex === null) return null;
    return this.visibleComments[this.activeCommentIndex] ?? null;
  }

  get hasPreviousComment(): boolean {
    return this.activeCommentIndex !== null && this.activeCommentIndex > 0;
  }

  get hasNextComment(): boolean {
    return this.activeCommentIndex !== null && this.activeCommentIndex < this.visibleComments.length - 1;
  }

  @Input()
  public history: HistoryViewModels | null = null;

  @Input()
  public stockHistories: StockHistoryViewModel[] = [];

  @Input()
  public dateRange: { start: string; end: string } = { start: '', end: '' };

  constructor(private cdr: ChangeDetectorRef, private zone: NgZone) {}

  ngOnInit(): void {
    this.setData(this.chartType);
    this.createChart();
  }

  ngOnChanges(changes: SimpleChanges): void {
      this.setData(this.chartType);
      this.createChart();
  }

  onChartTypeChange(value: string) {
    this.chartType = value;
    this.setData(this.chartType);
    this.createChart();
  }

  @HostListener('document:keydown', ['$event'])
  onKeyDown(event: KeyboardEvent) {
    if (this.activeCommentIndex === null) return;
    if (event.key === 'ArrowLeft') {
      this.previousComment();
      this.cdr.markForCheck();
    } else if (event.key === 'ArrowRight') {
      this.nextComment();
      this.cdr.markForCheck();
    }
  }

  closeComment() {
    this.activeCommentIndex = null;
    this.updateAnnotationStyles();
  }

  nextComment() {
    if (this.hasNextComment) {
      this.activeCommentIndex!++;
      this.updateAnnotationStyles();
    }
  }

  previousComment() {
    if (this.hasPreviousComment) {
      this.activeCommentIndex!--;
      this.updateAnnotationStyles();
    }
  }

  private updateAnnotationStyles() {
    if (!this.chart) return;
    const annotations = this.chart.options.plugins.annotation.annotations;
    this.visibleCommentAnnotationKeys.forEach((key, visibleIndex) => {
      const annotation = annotations[key];
      if (!annotation) return;
      const isActive = visibleIndex === this.activeCommentIndex;
      annotation.borderColor = isActive ? 'rgb(180, 83, 9)' : 'rgb(217, 119, 6)';
      annotation.borderWidth = isActive ? 3 : 1.5;
      annotation.borderDash = isActive ? [] : [6, 3];
      annotation.label.backgroundColor = isActive ? 'rgb(180, 83, 9)' : 'rgb(217, 119, 6)';
    });
    this.chart.update('none');
  }

  setData(chartType: string) {
    if (this.history) {

      const start = this.dateRange.start || this.history.items[0]?.date?.toString() || '';
      const end = this.dateRange.end || this.history.items[this.history.items.length - 1]?.date?.toString() || '';
      this.dates = this.generateDateRange(start, end);

      if (chartType === 'valueInGbp') {
        this.label = 'Value in Gbp';
        this.values = this.history.items.map(y => y.valueInGbp);
      }
      else if (chartType === 'unitValue') {
        this.label = 'unit value';
        this.values = this.history.items.map(y => y.units.valueInGbpPerUnit);
      }
      if (chartType === 'numberOfUnits') {
        this.values = this.history.items.map(y => y.units.numberOfUnits);
      }

      if (chartType === 'unitValue') {
        this.stockDatasets = this.stockHistories
          .filter(h => h.prices?.length)
          .map(h => ({
            symbol: h.stockSymbol,
            values: this.scaleStockToAccount(this.dates, h.prices, this.values)
          }));
      } else {
        this.stockDatasets = [];
      }

    }
    else if (this.stockHistories.some(h => h.prices?.length)) {
      this.chartType = 'unitValue';
      const withPrices = this.stockHistories.filter(h => h.prices?.length);

      const starts = withPrices.map(h => h.prices[0]?.date?.toString() ?? '').filter(d => d).sort();
      const ends = withPrices.map(h => h.prices[h.prices.length - 1]?.date?.toString() ?? '').filter(d => d).sort();
      const start = this.dateRange.start || starts[0] || '';
      const end = this.dateRange.end || ends[ends.length - 1] || '';
      this.dates = this.generateDateRange(start, end);
      this.values = [];

      this.stockDatasets = withPrices.map(stock => {
        let lastPrice: number | null = null;
        let bIdx = 0;
        const raw = this.dates.map(date => {
          while (bIdx < stock.prices.length && stock.prices[bIdx].date <= date) {
            lastPrice = stock.prices[bIdx++].price;
          }
          return lastPrice !== null ? lastPrice : NaN;
        });

        const firstIdx = raw.findIndex(v => !isNaN(v));
        if (firstIdx === -1) return { symbol: stock.stockSymbol, values: raw };
        const base = raw[firstIdx];
        return { symbol: stock.stockSymbol, values: raw.map(v => isNaN(v) ? NaN : (v / base) * 100) };
      });
    }
    else {
      this.dates = [];
      this.values = [];
      this.stockDatasets = [];
    }
  }

  private generateDateRange(start: string, end: string): string[] {
    const dates: string[] = [];
    const endDate = new Date(end);
    for (let d = new Date(start); d <= endDate; d.setDate(d.getDate() + 1)) {
      dates.push(d.toISOString().substring(0, 10));
    }
    return dates;
  }

  private scaleStockToAccount(accountDates: string[], prices: StockPriceViewModel[], accountValues: number[]): number[] {
    // Forward-fill: for each account date, use the latest benchmark price on or before that date
    const filled: (number | null)[] = [];
    let lastPrice: number | null = null;
    let bIdx = 0;

    for (const date of accountDates) {
      while (bIdx < prices.length && prices[bIdx].date <= date) {
        lastPrice = prices[bIdx].price;
        bIdx++;
      }
      filled.push(lastPrice);
    }

    // Normalise so the benchmark starts at the same value as the account at the first shared data point
    const firstIdx = filled.findIndex(v => v !== null);
    if (firstIdx === -1 || firstIdx >= accountValues.length) return filled.map(() => NaN);

    const firstBPrice = filled[firstIdx]!;
    const firstAValue = accountValues[firstIdx];
    if (firstBPrice === 0 || !firstAValue) return filled.map(() => NaN);

    return filled.map(v => v !== null ? (v / firstBPrice) * firstAValue : NaN);
  }

  private getDragSelectionPlugin() {
    return {
      id: 'dragSelection',
      afterDraw: (chart: any) => {
        if (!this.isDragging || this.dragStartX === null || this.dragCurrentX === null) return;
        const { ctx, chartArea } = chart;
        const x1 = Math.min(this.dragStartX, this.dragCurrentX);
        const x2 = Math.max(this.dragStartX, this.dragCurrentX);
        ctx.save();
        ctx.fillStyle = 'rgba(99, 102, 241, 0.15)';
        ctx.strokeStyle = 'rgba(99, 102, 241, 0.5)';
        ctx.lineWidth = 1;
        ctx.fillRect(x1, chartArea.top, x2 - x1, chartArea.bottom - chartArea.top);
        ctx.strokeRect(x1, chartArea.top, x2 - x1, chartArea.bottom - chartArea.top);
        ctx.restore();
      }
    };
  }

  private attachDragHandlers(canvas: HTMLCanvasElement): void {
    const mouseDownHandler = (e: MouseEvent) => {
      const rect = canvas.getBoundingClientRect();
      const x = e.clientX - rect.left;
      const chartArea = this.chart?.chartArea;
      if (!chartArea || x < chartArea.left || x > chartArea.right) return;

      this.dragStartX = x;
      this.dragCurrentX = x;
      this.isDragging = true;
      canvas.style.cursor = 'crosshair';

      let localMoveHandler: (me: MouseEvent) => void;
      let localUpHandler: (me: MouseEvent) => void;

      localMoveHandler = (me: MouseEvent) => {
        const r = canvas.getBoundingClientRect();
        const mx = me.clientX - r.left;
        const ca = this.chart?.chartArea;
        if (!ca) return;
        this.dragCurrentX = Math.max(ca.left, Math.min(ca.right, mx));
        this.chart?.render();
      };

      localUpHandler = (me: MouseEvent) => {
        window.removeEventListener('mousemove', localMoveHandler);
        window.removeEventListener('mouseup', localUpHandler);

        const dragDistance = Math.abs((this.dragCurrentX ?? 0) - (this.dragStartX ?? 0));
        if (dragDistance > 5 && this.chart) {
          const labels = this.chart.data.labels as string[];
          const scale = this.chart.scales['x'];
          const clamp = (v: number) => Math.max(0, Math.min(labels.length - 1, v));
          const startIdx = clamp(Math.round(scale.getValueForPixel(Math.min(this.dragStartX!, this.dragCurrentX!))));
          const endIdx = clamp(Math.round(scale.getValueForPixel(Math.max(this.dragStartX!, this.dragCurrentX!))));
          const startDate = labels[startIdx];
          const endDate = labels[endIdx];
          this.zone.run(() => {
            this.rangeSelected.emit({ start: startDate, end: endDate });
          });
        }

        this.isDragging = false;
        this.dragStartX = null;
        this.dragCurrentX = null;
        canvas.style.cursor = 'default';
        this.chart?.render();
      };

      window.addEventListener('mousemove', localMoveHandler);
      window.addEventListener('mouseup', localUpHandler);
    };

    canvas.addEventListener('mousedown', mouseDownHandler);
    this.canvasMouseDownHandler = mouseDownHandler;
  }

  private buildCommentAnnotations(): Record<string, any> {
    const annotations: Record<string, any> = {};
    this.visibleComments = [];
    this.visibleCommentAnnotationKeys = [];
    this.activeCommentIndex = null;

    if (!this.history?.comments?.length) {
      return annotations;
    }

    const dateSet = new Set(this.dates);

    this.history.comments.forEach((comment, index) => {
      if (!dateSet.has(comment.date)) {
        return;
      }

      const visibleIndex = this.visibleComments.length;
      const annotationKey = `comment-${index}`;
      this.visibleComments.push(comment);
      this.visibleCommentAnnotationKeys.push(annotationKey);

      annotations[annotationKey] = {
        type: 'line',
        scaleID: 'x',
        value: comment.date,
        borderColor: 'rgb(217, 119, 6)',
        borderWidth: 1.5,
        borderDash: [6, 3],
        label: {
          display: true,
          content: 'i',
          position: 'start',
          backgroundColor: 'rgb(217, 119, 6)',
          color: 'white',
          font: { weight: 'bold', size: 10 },
          padding: { top: 2, bottom: 2, left: 6, right: 6 },
          borderRadius: 10,
        },
        enter: (ctx: any) => { ctx.chart.canvas.style.cursor = 'pointer'; },
        leave: (ctx: any) => { ctx.chart.canvas.style.cursor = 'default'; },
        click: () => {
          this.zone.run(() => {
            this.activeCommentIndex = visibleIndex;
            this.updateAnnotationStyles();
            this.cdr.markForCheck();
          });
        },
      };
    });

    return annotations;
  }

  ngOnDestroy(): void {
    if (this.chart) {
      if (this.canvasMouseDownHandler) {
        this.chart.canvas.removeEventListener('mousedown', this.canvasMouseDownHandler);
      }
      this.chart.destroy();
    }
  }

  createChart() {

    if (this.chart) {
      if (this.canvasMouseDownHandler) {
        this.chart.canvas.removeEventListener('mousedown', this.canvasMouseDownHandler);
        this.canvasMouseDownHandler = null;
      }
      this.chart.destroy();
      this.chart = null;
    }

    const datasets: any[] = [];

    if (this.values.length > 0) {
      datasets.push({
        label: "Total £",
        data: this.values,
        backgroundColor: 'hsl(60, 9.1%, 97.8%)',
        borderColor: 'hsl(60, 9.1%, 70%)',
        fill: false
      });
    }

    this.stockDatasets.forEach((bd, i) => {
      datasets.push({
        label: bd.symbol,
        data: bd.values,
        backgroundColor: 'transparent',
        borderColor: this.stockColors[i % this.stockColors.length],
        fill: false
      });
    });

    this.chart = new Chart("MyChart", <any>{
      type: 'line',

      data: {
        labels: this.dates,
        datasets
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        elements: {
          point: {
            pointStyle: false
          }
        },
        plugins: {
          legend: {
            position: "chartArea",
          },
          annotation: {
            annotations: this.buildCommentAnnotations()
          }
        }
      },
      plugins: [this.getDragSelectionPlugin()]
    });

    this.attachDragHandlers(this.chart.canvas);
  }
}
