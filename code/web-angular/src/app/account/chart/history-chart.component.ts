import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import Chart from 'chart.js/auto';
import annotationPlugin from 'chartjs-plugin-annotation';
import { HistoryViewModels } from 'src/app/view-models/HistoryViewModels';


Chart.register(annotationPlugin);

@Component({
  selector: 'app-history-chart',
  standalone: true,
  imports: [],
  templateUrl: './history-chart.component.html',
  styleUrl: './history-chart.component.scss'
})
export class HistoryChartComponent implements OnChanges {

  public chart: any;

  private dates: string[] = [];
  private values: number[] = [];
  //private previousPrediction: string | null = null;

  @Input()
  public history: HistoryViewModels | null = null;

  ngOnInit(): void {
   // this.previousPrediction = JSON.stringify(this.prediction);
    this.setData();
    this.createChart();
  }

  ngOnChanges(changes: SimpleChanges): void {
    //const serializedPrediction = JSON.stringify(this.prediction);//
    //if (this.previousPrediction !== serializedPrediction) {
//      this.previousPrediction = serializedPrediction;
      this.setData();
      this.createChart();
    //}
  }

  setData() {
  if (this.history) {
      this.dates = this.history.items.map(y => y.date.toString());
      this.values = this.history.items.map(y => y.valueInGbp);
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
            annotations: {
              // targetAgeLine: {
              //   type: 'line',
              //   scaleID: 'x',
              //   value: this.prediction?.targetAge.toString(),
              //   borderColor: 'green',
              //   borderWidth: 1,
              //   label: {
              //     content: 'Target Age',
              //     enabled: true,
              //     position: 'top'
              //   }
              // },
              // targetLabel: {
              //   type: 'label',
              //   content: this.prediction ? `Amount at ${this.prediction.targetAge}: ${formatCurrency(this.prediction.amountAtTargetAge)}` : '',
              //   position: 'top',
              //   xAdjust: 0,
              //   yAdjust: 0,
              //   //backgroundColor: 'rgba(255,255,255,0.8)',
              //   font: {
              //     size: 12,
              //     style: 'normal',
              //     color: 'red'
              //   }
              // }
            }
          }
        }
      },
    });
  }
}
