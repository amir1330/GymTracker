import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-onboarding-guide',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './onboarding-guide.html',
  styleUrl: './onboarding-guide.css'
})
export class OnboardingGuide implements OnInit {
  visible = false;

  ngOnInit(): void {
    const dismissed = localStorage.getItem('onboardingDismissed');
    if (!dismissed) {
      this.visible = true;
    }
  }

  dismiss(): void {
    localStorage.setItem('onboardingDismissed', 'true');
    this.visible = false;
  }
}
