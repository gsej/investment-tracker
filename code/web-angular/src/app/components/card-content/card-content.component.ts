import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-card-content',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './card-content.component.html',
})
export class CardContentComponent {

  @Input()
  class: string = '';
}
