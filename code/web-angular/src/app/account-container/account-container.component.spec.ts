import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AccountContainerComponent } from './account-container.component';

describe('AccountSelectorComponent', () => {
  let component: AccountContainerComponent;
  let fixture: ComponentFixture<AccountContainerComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AccountContainerComponent]
    });
    fixture = TestBed.createComponent(AccountContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
