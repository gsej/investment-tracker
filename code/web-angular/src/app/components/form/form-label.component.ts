import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-formlabel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './form-label.component.html',
})
export class FormLabelComponent {

  @Input()
  class: string = '';

  @Input()
  for: string = '';
}
