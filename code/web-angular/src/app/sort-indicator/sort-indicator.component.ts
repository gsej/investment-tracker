import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-sort-indicator',
  templateUrl: './sort-indicator.component.html',
  styleUrls: ['./sort-indicator.component.scss']
})
export class SortIndicatorComponent {


  @Input()
  public columnName: string | null = null;

  @Input()
  public sortColumn: string | null = null;

  @Input()
  public sortAscending: boolean = true;
}
