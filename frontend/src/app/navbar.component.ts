import { Component } from '@angular/core';
import { MaterialModule } from './material.module';
import { AuthService } from './auth/auth.service';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [MaterialModule, FormsModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent {
  navbarSearch: string = '';
  constructor(public auth: AuthService, private router: Router) {}

  get username(): string {
    const user = this.auth.getUser();
    return user ? user.fullName.split(' ')[0] : '';
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  onNavbarSearch() {
    if (!this.navbarSearch.trim()) return;
    // Navigate to product list with search param
    this.router.navigate(['/product/list'], { queryParams: { search: this.navbarSearch.trim() } });
  }

  clearNavbarSearch() {
    this.navbarSearch = '';
    this.router.navigate(['/product/list']);
  }
}
