export interface UserProfile {
  id: number;
  userName: string;
  email: string;
  weight?: number;
  settings?: {
    theme: string;
  };
}
