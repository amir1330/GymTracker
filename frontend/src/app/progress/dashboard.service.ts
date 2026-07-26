import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ChartPoint {
  date: string;
  value: number;
}

export interface ChartSummary {
  current: number | null;
  best: number | null;
  change: string;
  trend: 'up' | 'down' | 'flat';
}

export interface ChartDataResponse {
  points: ChartPoint[];
  summary: ChartSummary;
}

export interface DashboardChart {
  id: number;
  label: string;
  metric: string;
  exerciseId: number | null;
  exerciseName: string | null;
  period: string;
  chartType: string;
  position: number;
  data: ChartDataResponse;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = '/api/dashboard';
  private statsUrl = '/api/stats';

  constructor(private http: HttpClient) {}

  getAll(): Observable<DashboardChart[]> {
    return this.http.get<DashboardChart[]>(this.apiUrl);
  }

  create(chart: Partial<DashboardChart>): Observable<DashboardChart> {
    return this.http.post<DashboardChart>(this.apiUrl, {
      label: chart.label,
      metric: chart.metric,
      exerciseId: chart.exerciseId,
      period: chart.period,
      chartType: chart.chartType
    });
  }

  update(id: number, chart: Partial<DashboardChart>): Observable<DashboardChart> {
    return this.http.put<DashboardChart>(`${this.apiUrl}/${id}`, {
      label: chart.label,
      metric: chart.metric,
      exerciseId: chart.exerciseId,
      period: chart.period,
      chartType: chart.chartType
    });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  reorder(items: { id: number; position: number }[]): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/reorder`, items);
  }

  getChartData(metric: string, exerciseId: number | null, period: string): Observable<ChartDataResponse> {
    return this.http.post<ChartDataResponse>(`${this.statsUrl}/chart-data`, {
      metric,
      exerciseId,
      period
    });
  }
}
