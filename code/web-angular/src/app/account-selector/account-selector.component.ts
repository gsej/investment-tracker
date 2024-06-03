import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Account } from '../models/account';

@Component({
  selector: 'app-account-selector',
  templateUrl: './account-selector.component.html',
  styleUrls: ['./account-selector.component.scss']
})
export class AccountSelectorComponent {

  @Input()
  public accounts: Account[] = []

  @Output()
  public accountsChanged: EventEmitter<string[]> = new EventEmitter<string[]>();


  accountsSelected(selection: any) {

    const accountCodes = selection.map((item: any) => item.value.accountCode);
    this.accountsChanged.next(accountCodes);
  }

}
