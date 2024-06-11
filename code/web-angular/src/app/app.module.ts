import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AccountSelectorComponent } from './account-selector/account-selector.component';
import { AccountSummaryComponent } from './account-summary/account-summary.component';
import { AccountContainerComponent } from './account-container/account-container.component';
import { AccountValueHistoryComponent } from './account-value-history/account-value-history.component';
import { SortIndicatorComponent } from './sort-indicator/sort-indicator.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    AppComponent,
    AccountContainerComponent,
    AccountSelectorComponent,
    AccountSummaryComponent,
    AccountValueHistoryComponent,
    SortIndicatorComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    BrowserAnimationsModule,
    ReactiveFormsModule,
    MatSelectModule,
    MatTableModule,
    MatIconModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
