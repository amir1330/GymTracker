import { Component, OnInit } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-onboarding-guide',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './onboarding-guide.html',
  styleUrl: './onboarding-guide.css'
})
export class OnboardingGuide implements OnInit {
  visible = false;

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    const dismissed = localStorage.getItem('onboardingDismissed');
    if (!dismissed && this.authService.isLoggedIn()) {
      this.visible = true;
    }
  }

  dismiss(): void {
    localStorage.setItem('onboardingDismissed', 'true');
    this.visible = false;
  }
}
