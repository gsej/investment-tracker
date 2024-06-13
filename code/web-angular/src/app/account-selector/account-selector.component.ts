import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Account } from '../models/account';
import { FormControl, FormGroup } from '@angular/forms';

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

  accountSelectorForm: FormGroup;

  constructor() {
    this.accountSelectorForm = new FormGroup({
      account: new FormControl(null)
    });
  }

  onAccountChanged() {
    let selectedAccountCode = this.accountSelectorForm.controls["account"].value.accountCode;
    this.accountChanged.next(selectedAccountCode);
  }
}
