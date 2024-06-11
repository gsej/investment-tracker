import { Component, EventEmitter, Input, Output } from '@angular/core';
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
  public accountChanged: EventEmitter<string> = new EventEmitter<string>();

  accountsSelected(event: any) {
    this.accountChanged.next(event.value.accountCode);
  }
}
