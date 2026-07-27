export interface UserProfile {
  id: number;
  userName: string;
  email: string;
  weight?: number;
  height?: number;
  settings?: {
    theme: string;
  };
}
