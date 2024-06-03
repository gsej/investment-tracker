import { Component, Input } from '@angular/core';
import { RecordedTotalValues } from '../models/recordedTotalValues';

@Component({
  selector: 'app-recorded-total-values',
  templateUrl: './recorded-total-values.component.html',
  styleUrls: ['./recorded-total-values.component.scss']
})
export class RecordedTotalValuesComponent {

  public displayedColumns = ['date', 'accountCode', 'totalValueInGbp'];

  @Input()
  public recordedTotalValues: RecordedTotalValues | null = null;
}
