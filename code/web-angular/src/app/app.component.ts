import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AccountContainerComponent } from './account/account-container/account-container.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [AccountContainerComponent ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {

}
