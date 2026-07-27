import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { DashboardService } from '../../services/dashboard.service';
import { DashboardChart } from '../../models/dashboard.model';
import { ChartTile } from '../../components/chart-tile/chart-tile';
import { ChartEditor } from '../../components/chart-editor/chart-editor';

@Component({
  selector: 'app-progress-page',
  standalone: true,
  imports: [ChartTile, ChartEditor],
  templateUrl: './progress-page.html',
  styleUrl: './progress-page.css'
})
export class ProgressPage implements OnInit {
  charts: DashboardChart[] = [];
  loading = true;

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
    this.dashboardService.getAll().subscribe(charts => {
      this.charts = charts;
      this.loading = false;
      this.cdr.markForCheck();
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
      this.dashboardService.update(this.editingChart.id, config).subscribe(() => {
        this.closeEditor();
        this.loadDashboard();
      });
    } else {
      this.dashboardService.create(config).subscribe(() => {
        this.closeEditor();
        this.loadDashboard();
      });
    }
  }

  deleteChart(chart: DashboardChart): void {
    if (confirm(`Delete "${chart.label}"?`)) {
      this.dashboardService.delete(chart.id).subscribe(() => this.loadDashboard());
    }
  }

  moveUp(chart: DashboardChart, index: number): void {
    if (index === 0) return;
    const items = [
      { id: this.charts[index - 1].id, position: index },
      { id: chart.id, position: index - 1 }
    ];
    this.dashboardService.reorder(items).subscribe(() => this.loadDashboard());
  }

  moveDown(chart: DashboardChart, index: number): void {
    if (index === this.charts.length - 1) return;
    const items = [
      { id: chart.id, position: index + 1 },
      { id: this.charts[index + 1].id, position: index }
    ];
    this.dashboardService.reorder(items).subscribe(() => this.loadDashboard());
  }
}
