import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BaseChartDirective } from 'ng2-charts';
import { DashboardService } from '../../services/dashboard.service';
import { DashboardChart } from '../../models/dashboard.model';
import { ExerciseService } from '../../services/exercise.service';
import { Exercise } from '../../models/exercise.model';

@Component({
  selector: 'app-chart-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, BaseChartDirective],
  templateUrl: './chart-editor.html',
  styleUrl: './chart-editor.css'
})
export class ChartEditor implements OnInit, OnChanges {
  @Input() chart: DashboardChart | null = null;
  @Input() isOpen = false;
  @Output() save = new EventEmitter<Partial<DashboardChart>>();
  @Output() cancel = new EventEmitter<void>();

  label = '';
  metric = 'weight';
  exerciseId: number | null = null;
  period = '30d';
  chartType = 'line';
  lastAutoLabel = '';

  exercises: Exercise[] = [];
  previewData: any = { labels: [], datasets: [] };
  previewSummary: any = null;
  previewLoading = false;
  previewTimer: any = null;

  metrics = [
    { value: 'weight', label: 'Weight', needsExercise: true, allOption: false },
    { value: 'volume', label: 'Volume', needsExercise: false, allOption: true },
    { value: 'duration', label: 'Duration', needsExercise: true, allOption: false },
    { value: 'bodyWeight', label: 'Body Weight', needsExercise: false, allOption: false },
    { value: 'frequency', label: 'Frequency', needsExercise: false, allOption: false }
  ];

  periods = [
    { value: '7d', label: '7 days' },
    { value: '30d', label: '30 days' },
    { value: '90d', label: '90 days' },
    { value: '180d', label: '6 months' },
    { value: '365d', label: '1 year' },
    { value: 'all', label: 'All time' }
  ];

  previewChartOptions: any = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false } },
    scales: {
      x: { grid: { color: '#665c54' }, ticks: { color: '#a89984', font: { family: 'JetBrains Mono', size: 10 } } },
      y: { grid: { color: '#665c54' }, ticks: { color: '#a89984', font: { family: 'JetBrains Mono', size: 10 } } }
    }
  };

  constructor(
    private dashboardService: DashboardService,
    private exerciseService: ExerciseService,
    private cdr: ChangeDetectorRef
  ) {}

  getChartType(): 'line' | 'bar' {
    return this.chartType as 'line' | 'bar';
  }

  ngOnInit(): void {
    this.exerciseService.getAll().subscribe(exercises => {
      this.exercises = exercises;
      this.cdr.markForCheck();
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen'] && this.isOpen) {
      if (this.chart) {
        this.label = this.chart.label;
        this.metric = this.chart.metric;
        this.exerciseId = this.chart.exerciseId;
        this.period = this.chart.period;
        this.chartType = this.chart.chartType;
        this.lastAutoLabel = this.chart.label;
      } else {
        this.resetForm();
      }
      this.schedulePreview();
    }
  }

  resetForm(): void {
    this.label = '';
    this.metric = 'weight';
    this.exerciseId = null;
    this.period = '30d';
    this.chartType = 'line';
    this.lastAutoLabel = '';
  }

  get currentMetric() {
    return this.metrics.find(m => m.value === this.metric);
  }

  get showExercise(): boolean {
    return this.currentMetric?.needsExercise || false;
  }

  get showAllOption(): boolean {
    return this.currentMetric?.allOption || false;
  }

  onConfigChange(): void {
    this.autoLabel();
    this.schedulePreview();
  }

  onLabelInput(): void {
    this.lastAutoLabel = '';
  }

  private autoLabel(): void {
    if (this.label && this.label !== this.lastAutoLabel) return;
    let newLabel = '';
    if (this.showExercise && this.exerciseId) {
      const ex = this.exercises.find(e => e.id === this.exerciseId);
      newLabel = ex?.name || '';
    } else if (this.showAllOption && !this.exerciseId) {
      newLabel = 'Total Volume';
    } else if (this.metric === 'bodyWeight') {
      newLabel = 'Body Weight';
    } else if (this.metric === 'frequency') {
      newLabel = 'Workouts / week';
    }
    if (newLabel) {
      this.label = newLabel;
      this.lastAutoLabel = newLabel;
    }
  }

  private schedulePreview(): void {
    if (this.previewTimer) clearTimeout(this.previewTimer);
    this.previewTimer = setTimeout(() => this.loadPreview(), 400);
  }

  private loadPreview(): void {
    if (!this.metric || (this.showExercise && !this.exerciseId)) {
      this.previewData = { labels: [], datasets: [] };
      this.previewSummary = null;
      this.cdr.markForCheck();
      return;
    }

    this.previewLoading = true;
    this.cdr.markForCheck();

    const exerciseId = this.showAllOption ? null : this.exerciseId;

    this.dashboardService.getChartData(this.metric, exerciseId, this.period).subscribe(data => {
      this.previewSummary = data.summary;
      this.previewData = {
        labels: data.points.map(p => p.date.slice(5)),
        datasets: [{
          data: data.points.map(p => p.value),
          borderColor: '#b8bb26',
          backgroundColor: this.chartType === 'bar' ? 'rgba(184, 187, 38, 0.8)' : 'transparent',
          pointBackgroundColor: '#b8bb26',
          borderWidth: 2,
          tension: 0.3,
          fill: false
        }]
      };
      this.previewLoading = false;
      this.cdr.markForCheck();
    });
  }

  onSave(): void {
    if (!this.label.trim()) return;
    this.save.emit({
      label: this.label.trim(),
      metric: this.metric,
      exerciseId: this.showExercise ? this.exerciseId : null,
      period: this.period,
      chartType: this.chartType
    });
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
