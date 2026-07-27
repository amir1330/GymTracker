export interface UserProfile {
  id: number;
  email: string;
  settings?: {
    theme: string;
  };
}
