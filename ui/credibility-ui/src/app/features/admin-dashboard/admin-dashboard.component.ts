import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminCleanupService, Website, Category } from './admin-cleanup.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <h2>🧹 Admin Cleanup</h2>
      
      <table>
        <thead>
          <tr>
            <th>Website</th>
            <th>Category</th>
            <th>Action</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let site of websites">
            <td>{{ site.domain }}</td>
            <td>{{ site.categoryName }}</td>
            <td>
              <select (change)="onMove(site.id, $event)" [disabled]="isBusy(site.id)">
                <option value="" disabled selected>Move to...</option>
                <option *ngFor="let c of categories" [value]="c.id">{{ c.name }}</option>
              </select>
            </td>
            <td>
              <ng-container *ngIf="getState(site.id) as s">
                <span [class]="'badge ' + s.status">{{ s.message }}</span>
                <button *ngIf="s.status === 'error'" (click)="clear(site.id)">×</button>
              </ng-container>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  `,
  styles: [`
    .container { padding: 20px; font-family: sans-serif; }
    table { width: 100%; border-collapse: collapse; }
    td, th { padding: 12px; border-bottom: 1px solid #eee; text-align: left; }
    .badge { padding: 4px 8px; border-radius: 4px; font-size: 12px; }
    .success { background: #d4edda; color: #155724; }
    .error { background: #f8d7da; color: #721c24; }
    .validating { background: #fff3cd; color: #856404; }
  `]
})
export class AdminDashboardComponent implements OnInit {
  websites: Website[] = [];
  categories: Category[] = [];
  states: any = {};

  constructor(private service: AdminCleanupService) {}

  ngOnInit() {
    this.service.getWebsites().subscribe(res => this.websites = res);
    this.service.getCategories().subscribe(res => this.categories = res);
    this.service.states$.subscribe(s => this.states = s);
  }

  onMove(siteId: string, event: any) {
    this.service.processCategoryChange(siteId, event.target.value).subscribe(() => {
      // Refresh local text (optional: find in array and update name)
      this.service.getWebsites().subscribe(res => this.websites = res);
    });
  }

  getState(id: string) { return this.states.get(id); }
  isBusy(id: string) { return this.getState(id)?.status === 'validating'; }
  clear(id: string) { this.service.clearState(id); }
}