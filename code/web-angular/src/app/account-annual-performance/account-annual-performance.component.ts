import { Component, Input } from '@angular/core';
import { AccountAnnualPerformances } from '../models/accountAnnualPerformances';

@Component({
  selector: 'app-account-annual-performance',
  templateUrl: './account-annual-performance.component.html',
  styleUrls: ['./account-annual-performance.component.scss']
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
