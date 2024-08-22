import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { Account } from '../models/account';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-account-selector',
  standalone: true,
  imports: [ ReactiveFormsModule ],
  templateUrl: './account-selector.component.html',
  styleUrls: ['./account-selector.component.scss']
})
export class AccountSelectorComponent implements OnChanges {

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

  ngOnChanges(changes: SimpleChanges): void {
    if (this.accountSelectorForm.controls["account"].value == null && this.accounts.length > 0) {
      this.accountSelectorForm.controls['account'].setValue(this.accounts[0].accountCode);
      this.accountChanged.next(this.accounts[0].accountCode);
    }
  }

  onAccountChanged() {
    const selectedAccountCode = this.accountSelectorForm.controls["account"].value;
    this.accountChanged.next(selectedAccountCode);
  }
}
