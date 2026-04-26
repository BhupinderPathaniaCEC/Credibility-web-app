
import { Component, signal, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TokenService } from './core/auth/token.service';
import { HeaderComponent } from './core/layout/header/header.component';


@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly title = signal('credibility-ui');

  constructor(private tokenService: TokenService) {}

  ngOnInit(): void {
    this.tokenService.fetchAndStoreToken();
  }
}
