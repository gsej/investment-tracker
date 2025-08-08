import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { AccountAnnualPerformances } from '../models/accountAnnualPerformances';

// TODO: is this used?
@Component({
  selector: 'app-account-annual-performance',
  standalone: true,
  templateUrl: './account-annual-performance.component.html',
  styleUrls: ['./account-annual-performance.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AccountAnnualPerformanceComponent  {

  public displayedColumns = [
    'year',
    'accountCode',
    'inFlows'];

  @Input()
  public accountAnnualPerformances: AccountAnnualPerformances = {
    years: []
  };

}
