import { Component, Input, OnChanges, SimpleChanges, ChangeDetectionStrategy, ChangeDetectorRef, NgZone } from '@angular/core';
import Chart from 'chart.js/auto';
import annotationPlugin from 'chartjs-plugin-annotation';
import { HistoryViewModels } from 'src/app/view-models/HistoryViewModels';
import { CommentViewModel } from 'src/app/view-models/CommentViewModel';
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
export class HistoryChartComponent implements OnChanges {

  public chart: any;

  private dates: string[] = [];
  private values: number[] = [];

  public chartType = 'valueInGbp';
  private label = '';

  public activeComment: CommentViewModel | null = null;

  @Input()
  public history: HistoryViewModels | null = null;

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

  closeComment() {
    this.activeComment = null;
  }

  setData(chartType: string) {
    if (this.history) {

      this.dates = this.history.items.map(y => y.date.toString());

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

    }
    else {
      this.dates = [];
      this.values = [];
    }
  }

  private buildCommentAnnotations(): Record<string, any> {
    const annotations: Record<string, any> = {};

    if (!this.history?.comments?.length) {
      return annotations;
    }

    const dateSet = new Set(this.dates);

    this.history.comments.forEach((comment, index) => {
      if (!dateSet.has(comment.date)) {
        return;
      }

      annotations[`comment-${index}`] = {
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
            this.activeComment = comment;
            this.cdr.markForCheck();
          });
        },
      };
    });

    return annotations;
  }

  createChart() {

    if (this.chart) {
      this.chart.destroy();
      this.chart = null;
    }

    this.chart = new Chart("MyChart", <any>{
      type: 'line',

      data: {
        labels: this.dates,
        datasets: [
          {
            label: "Total £",
            data: this.values,
            backgroundColor: 'hsl(60, 9.1%, 97.8%)',
            borderColor: 'hsl(60, 9.1%, 70%)',
            fill: false
          }
        ]
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
    });
  }
}
