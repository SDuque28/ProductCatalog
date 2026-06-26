import { Component, input } from '@angular/core';

@Component({
  selector: 'app-error-state',
  standalone: true,
  templateUrl: './error-state.component.html',
  styleUrl: './error-state.component.scss'
})
export class ErrorStateComponent {
  public readonly title = input('Something went wrong');
  public readonly message = input('Unable to complete the request right now.');
}
