import { Component, Input, OnChanges, SimpleChanges, ChangeDetectionStrategy } from '@angular/core';
import Chart from 'chart.js/auto';
import annotationPlugin from 'chartjs-plugin-annotation';
import { HistoryViewModels } from 'src/app/view-models/HistoryViewModels';
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

  @Input()
  public history: HistoryViewModels | null = null;

  ngOnInit(): void {
    this.setData(this.chartType);
    this.createChart();
  }

  ngOnChanges(changes: SimpleChanges): void {
      this.setData(this.chartType);
      this.createChart();
  }

  // setData() {
  // if (this.history) {
  //     this.dates = this.history.items.map(y => y.date.toString());
  //     this.values = this.history.items.map(y => y.valueInGbp);
  //   }
  //   else {
  //   this.dates = [];
  //     this.values = [];
  //   }
  // }

  onChartTypeChange(value: string) {
    this.chartType = value;
    this.setData(this.chartType);
    this.createChart();
  }

  setData(chartType: string) {
    if (this.history) {

      this.dates = this.history.items.map(y => y.date.toString());

      if (chartType === 'valueInGbp') {
        this.label = 'Value in Gbp';
        this.values = this.history.items.map(y => y.valueInGbp);
        //this.percentages = this.prediction.cumulativeBands.map(band => band.percentage * 100);
      }
      else if (chartType === 'unitValue') {
        this.label = 'unit value';
        this.values = this.history.items.map(y => y.units.valueInGbpPerUnit);
                //this.percentages = this.prediction.bands.map(band => band.percentage * 100);
      }
      if (chartType === 'numberOfUnits') {
        // this.label = 'percentage of outcomes having more than this';
        this.values = this.history.items.map(y => y.units.numberOfUnits);
        // this.values = this.prediction.reverseCumulativeBands.map(band => `${format000s(band.lower)}k`);
        // this.percentages = this.prediction.reverseCumulativeBands.map(band => band.percentage * 100);
      }

    }
    else {
      this.dates = [];
      this.values = [];
    }
  }


  createChart() {

    if (this.chart) {
      this.chart.destroy();
      this.chart = null;
    }

    this.chart = new Chart("MyChart", <any>{
      type: 'line', //this denotes tha type of chart

      data: {// values on X-Axis
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
          }
        }
      },
    });
  }
}
