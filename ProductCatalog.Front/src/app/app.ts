import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FooterComponent } from './layout/components/footer/footer.component';
import { NavbarComponent } from './layout/components/navbar/navbar.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent, FooterComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {}
