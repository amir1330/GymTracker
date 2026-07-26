import { Component, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { DashboardChart } from './dashboard.service';

@Component({
  selector: 'app-chart-tile',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  templateUrl: './chart-tile.html',
  styleUrl: './chart-tile.css'
})
export class ChartTile implements OnChanges {
  @Input() chart!: DashboardChart;
  @ViewChild(BaseChartDirective) chartDirective?: BaseChartDirective;

  chartData: any = { labels: [], datasets: [] };

  chartOptions: any = {
    responsive: true,
    maintainAspectRatio: false,
    animation: false,
    plugins: { legend: { display: false } },
    scales: {
      x: { grid: { color: '#665c54' }, ticks: { color: '#a89984', font: { family: 'JetBrains Mono', size: 10 } } },
      y: { grid: { color: '#665c54' }, ticks: { color: '#a89984', font: { family: 'JetBrains Mono', size: 10 } } }
    }
  };

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['chart'] && this.chart?.data?.points) {
      this.updateChart();
    }
  }

  getChartType(): 'line' | 'bar' {
    return this.chart.chartType as 'line' | 'bar';
  }

  private updateChart(): void {
    const labels = this.chart.data.points.map((p: any) => p.date.slice(5));
    const values = this.chart.data.points.map((p: any) => p.value);

    this.chartData = {
      labels,
      datasets: [{
        data: values,
        borderColor: '#b8bb26',
        backgroundColor: this.chart.chartType === 'bar' ? 'rgba(184, 187, 38, 0.8)' : 'transparent',
        pointBackgroundColor: '#b8bb26',
        pointBorderColor: '#b8bb26',
        borderWidth: 2,
        tension: 0.3,
        fill: false
      }]
    };

    setTimeout(() => this.chartDirective?.update());
  }

  get trendIcon(): string {
    if (!this.chart?.data?.summary) return '';
    switch (this.chart.data.summary.trend) {
      case 'up': return '↑';
      case 'down': return '↓';
      default: return '→';
    }
  }

  get trendClass(): string {
    return this.chart?.data?.summary?.trend || 'flat';
  }
}
