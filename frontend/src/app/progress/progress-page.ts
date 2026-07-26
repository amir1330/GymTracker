import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService, DashboardChart } from './dashboard.service';
import { ChartTile } from './chart-tile';
import { ChartEditor } from './chart-editor';

@Component({
  selector: 'app-progress-page',
  standalone: true,
  imports: [CommonModule, ChartTile, ChartEditor],
  templateUrl: './progress-page.html',
  styleUrl: './progress-page.css'
})
export class ProgressPage implements OnInit {
  charts: DashboardChart[] = [];
  loading = true;
  error = '';

  editorOpen = false;
  editingChart: DashboardChart | null = null;

  constructor(
    private dashboardService: DashboardService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;
    this.error = '';
    this.dashboardService.getAll().subscribe({
      next: (charts: DashboardChart[]) => {
        this.charts = charts;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        this.error = this.extractError(err);
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  openAddChart(): void {
    this.editingChart = null;
    this.editorOpen = true;
  }

  openEditChart(chart: DashboardChart): void {
    this.editingChart = chart;
    this.editorOpen = true;
  }

  closeEditor(): void {
    this.editorOpen = false;
    this.editingChart = null;
  }

  saveChart(config: Partial<DashboardChart>): void {
    if (this.editingChart) {
      this.dashboardService.update(this.editingChart.id, config).subscribe({
        next: () => {
          this.closeEditor();
          this.loadDashboard();
        },
        error: (err: any) => {
          this.error = this.extractError(err);
          this.cdr.markForCheck();
        }
      });
    } else {
      this.dashboardService.create(config).subscribe({
        next: () => {
          this.closeEditor();
          this.loadDashboard();
        },
        error: (err: any) => {
          this.error = this.extractError(err);
          this.cdr.markForCheck();
        }
      });
    }
  }

  deleteChart(chart: DashboardChart): void {
    if (confirm(`Delete "${chart.label}"?`)) {
      this.dashboardService.delete(chart.id).subscribe({
        next: () => this.loadDashboard(),
        error: (err: any) => {
          this.error = this.extractError(err);
          this.cdr.markForCheck();
        }
      });
    }
  }

  moveUp(chart: DashboardChart, index: number): void {
    if (index === 0) return;
    const items = [
      { id: this.charts[index - 1].id, position: index },
      { id: chart.id, position: index - 1 }
    ];
    this.dashboardService.reorder(items).subscribe({
      next: () => this.loadDashboard(),
      error: (err: any) => {
        this.error = this.extractError(err);
        this.cdr.markForCheck();
      }
    });
  }

  moveDown(chart: DashboardChart, index: number): void {
    if (index === this.charts.length - 1) return;
    const items = [
      { id: chart.id, position: index + 1 },
      { id: this.charts[index + 1].id, position: index }
    ];
    this.dashboardService.reorder(items).subscribe({
      next: () => this.loadDashboard(),
      error: (err: any) => {
        this.error = this.extractError(err);
        this.cdr.markForCheck();
      }
    });
  }

  private extractError(err: any): string {
    const body = err.error;
    if (typeof body === 'string') return body;
    if (body?.message) return body.message;
    return 'Operation failed';
  }
}
