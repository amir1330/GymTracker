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
