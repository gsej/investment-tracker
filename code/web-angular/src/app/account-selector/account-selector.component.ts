import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, ChangeDetectionStrategy, OnInit, OnDestroy, HostListener } from '@angular/core';
import { Account } from '../models/account';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-account-selector',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './account-selector.component.html',
  styleUrls: ['./account-selector.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AccountSelectorComponent implements OnChanges, OnInit, OnDestroy {

  @Input()
  public accounts: Account[] = []

  @Output()
  public accountChanged: EventEmitter<string[]> = new EventEmitter<string[]>();

  accountSelectorForm: FormGroup;
  selectedAccounts: Set<string> = new Set<string>();
  pendingSelectedAccounts: Set<string> = new Set<string>();
  isShiftPressed: boolean = false;

  constructor() {
    this.accountSelectorForm = new FormGroup({
      rad_selectedAccount: new FormControl('')
    });
  }

onLabelClick(event: MouseEvent, accountCode: string) {
  if (event.shiftKey) {
    event.preventDefault();
    const checkbox = document.getElementById('chk_' + accountCode) as HTMLInputElement;
    if (checkbox) {
      checkbox.click();
    }
  }
}

  get hasChanges(): boolean {
    if (!this.multiSelectMode) {
      return false;
    }

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

  get multiSelectMode(): boolean {
    return this.isShiftPressed || this.pendingSelectedAccounts.size > 1;
  }

  @HostListener('window:keydown', ['$event'])
  onKeyDown(event: KeyboardEvent) {
    if (event.key === 'Shift') {
      this.isShiftPressed = true;
    }
  }

  @HostListener('window:keyup', ['$event'])
  onKeyUp(event: KeyboardEvent) {
    if (event.key === 'Shift') {
      this.isShiftPressed = false;
    }
  }

  ngOnInit(): void {

  }

  ngOnDestroy(): void {
    // Cleanup if needed
  }

  ngOnChanges(changes: SimpleChanges): void {

    if (!changes["accounts"]) {
      return;
    }

    if (this.accounts.length > 0) {
      this.accounts.forEach(account => {
        const controlName = "chk_" + account.accountCode;
        if (!this.accountSelectorForm.controls[controlName]) {
          this.accountSelectorForm.addControl(controlName, new FormControl(false));
        }
      });

      if (this.selectedAccounts.size === 0) {
        this.selectedAccounts.add(this.accounts[0].accountCode);
        this.pendingSelectedAccounts.add(this.accounts[0].accountCode);
        this.updateFormControls();
        this.accountChanged.next(Array.from(this.selectedAccounts));
      }
    }

  }

  private updateFormControls(): void {

    this.accounts.forEach(account => {
      const isSelected = this.pendingSelectedAccounts.has(account.accountCode);
      this.accountSelectorForm.controls["chk_" + account.accountCode].setValue(isSelected);
    });

    const selectedAccount = Array.from(this.pendingSelectedAccounts)[0] || '';
    this.accountSelectorForm.controls['rad_selectedAccount'].setValue(selectedAccount);
  }

  onAccountChanged(accountCode: string, event: Event) {
    const input = event.target as HTMLInputElement;

    if (this.multiSelectMode) {
      if (input.checked) {
        this.pendingSelectedAccounts.add(accountCode);
      } else {
        this.pendingSelectedAccounts.delete(accountCode);
      }
    } else {

      if (input.checked) {
        this.pendingSelectedAccounts.clear();
        this.pendingSelectedAccounts.add(accountCode);
      }

      this.updateFormControls();
    }

    if (this.pendingSelectedAccounts.size === 1) {
      this.selectedAccounts = new Set(this.pendingSelectedAccounts);
      this.accountChanged.next(Array.from(this.selectedAccounts));
    }
  }

  applyChanges() {
    this.selectedAccounts = new Set(this.pendingSelectedAccounts);
  }
}
