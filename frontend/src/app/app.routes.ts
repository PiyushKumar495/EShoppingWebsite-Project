import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./auth/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./auth/register/register.component').then(m => m.RegisterComponent) },
  { path: 'product/list', loadComponent: () => import('./product/list/list.component').then(m => m.ListComponent) },
  { path: 'product/detail/:id', loadComponent: () => import('./product/detail/detail.component').then(m => m.DetailComponent) },
  { path: 'cart', loadComponent: () => import('./cart/cart.component').then(m => m.CartComponent) },
  { path: 'order/place', loadComponent: () => import('./order/place/place.component').then(m => m.PlaceComponent) },
  { path: 'order/history', loadComponent: () => import('./order/list/list.component').then(m => m.ListComponent) },
  { path: 'user/profile', loadComponent: () => import('./user/profile/profile.component').then(m => m.ProfileComponent) },
  { path: 'admin-dashboard', loadComponent: () => import('./admin/dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent) },
  { path: 'admin/payments', loadComponent: () => import('./admin/payments/admin-payments.component').then(m => m.AdminPaymentsComponent) },
  { path: 'category/manage', loadComponent: () => import('./category/manage-category.component').then(m => m.ManageCategoryComponent) },
  { path: 'product/manage', loadComponent: () => import('./product/manage-product.component').then(m => m.ManageProductComponent) },
  { path: 'user-dashboard', loadComponent: () => import('./user/profile/profile.component').then(m => m.ProfileComponent) },
  { path: '', redirectTo: 'product/list', pathMatch: 'full' },
  { path: '**', redirectTo: 'product/list' }
];
