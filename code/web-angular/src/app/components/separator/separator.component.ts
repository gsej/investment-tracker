import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-separator',
  standalone: true,
  imports: [CommonModule],
  template: `<div [ngClass]="[class, 'shrink-0', 'bg-border', 'h-[1px]', 'w-full']"></div>`
})
export class SeparatorComponent {

  @Input()
  class: string = '';
}
