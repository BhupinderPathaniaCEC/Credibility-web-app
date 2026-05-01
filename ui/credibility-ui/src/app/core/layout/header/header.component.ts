import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent implements OnInit {
  private readonly auth = inject(AuthService);

  isLoggedIn = false;
  isAdminUser = false;

  ngOnInit() {
    this.auth.isLoggedIn$.subscribe(status => {
      this.isLoggedIn = status;
      this.isAdminUser = status && this.auth.isAdmin();
    });
  }

  login() {
    this.auth.login();
  }

  logout() {
    this.auth.logout();
  }
}