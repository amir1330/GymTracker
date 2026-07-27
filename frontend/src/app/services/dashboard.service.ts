import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DashboardChart, ChartDataResponse } from '../models/dashboard.model';

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
