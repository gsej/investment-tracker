import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-card-title',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './card-title.component.html',
})
export class CardTitleComponent {


  @Input()
  class: string = '';

}
