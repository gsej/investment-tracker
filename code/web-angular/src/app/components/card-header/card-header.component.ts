import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-card-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './card-header.component.html',
})
export class CardHeaderComponent {

  @Input()
  class: string = '';

}
