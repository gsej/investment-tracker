import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { Account } from '../models/account';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-account-selector',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './account-selector.component.html',
  styleUrls: ['./account-selector.component.scss']
})
export class AccountSelectorComponent implements OnChanges {

  @Input()
  public accounts: Account[] = []

  @Output()
  public accountChanged: EventEmitter<string[]> = new EventEmitter<string[]>();

  accountSelectorForm: FormGroup;
  selectedAccounts: Set<string> = new Set<string>();
  pendingSelectedAccounts: Set<string> = new Set<string>();

  constructor() {
    this.accountSelectorForm = new FormGroup({});
  }

  get hasChanges(): boolean {
    if (this.selectedAccounts.size !== this.pendingSelectedAccounts.size) {
      return true;
    }
    for (const account of this.selectedAccounts) {
      if (!this.pendingSelectedAccounts.has(account)) {
        return true;
      }
    }
    return false;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.accounts.length > 0) {
      // Initialize form controls for each account
      this.accounts.forEach(account => {
        if (!this.accountSelectorForm.controls[account.accountCode]) {
          this.accountSelectorForm.addControl(account.accountCode, new FormControl(false));
        }
      });

      // If no accounts are selected and we have accounts, select the first one by default
      if (this.selectedAccounts.size === 0) {
        this.selectedAccounts.add(this.accounts[0].accountCode);
        this.pendingSelectedAccounts.add(this.accounts[0].accountCode);
        this.accountSelectorForm.controls[this.accounts[0].accountCode].setValue(true);
        this.accountChanged.next(Array.from(this.selectedAccounts));
      }
    }
  }

  onAccountChanged(accountCode: string, event: Event) {
    const checkbox = event.target as HTMLInputElement;

    if (checkbox.checked) {
      this.pendingSelectedAccounts.add(accountCode);
    } else {
      this.pendingSelectedAccounts.delete(accountCode);
    }
  }

  applyChanges() {
    this.selectedAccounts = new Set(this.pendingSelectedAccounts);
    this.accountChanged.next(Array.from(this.selectedAccounts));
  }
}
