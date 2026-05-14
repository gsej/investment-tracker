import { Component, Input, OnChanges, SimpleChanges, ChangeDetectionStrategy, ChangeDetectorRef, NgZone, HostListener } from '@angular/core';
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
