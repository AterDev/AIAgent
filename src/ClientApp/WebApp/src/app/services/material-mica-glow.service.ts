import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class MaterialMicaGlowService {
  private activeElement: HTMLElement | null = null;
  private initialized = false;

  private readonly targetSelector = [
    'button.mat-mdc-button-base:not(:disabled)',
    'a.mat-mdc-button-base',
    '.mat-mdc-menu-item',
    '.mat-mdc-list-item',
    '.mat-mdc-tab-link',
    '.mat-mdc-tab'
  ].join(',');

  init(): void {
    if (this.initialized || typeof document === 'undefined') {
      return;
    }

    this.initialized = true;
    document.addEventListener('pointermove', this.onPointerMove, { passive: true });
    document.addEventListener('pointerdown', this.onPointerMove, { passive: true });
    document.addEventListener('pointerleave', this.resetActive, { passive: true });
    window.addEventListener('blur', this.resetActive, { passive: true });
  }

  private onPointerMove = (event: PointerEvent): void => {
    const source = event.target as Element | null;
    const target = source?.closest(this.targetSelector) as HTMLElement | null;

    if (!target) {
      this.resetActive();
      return;
    }

    if (this.activeElement !== target) {
      this.resetActive();
      this.activeElement = target;
      this.activeElement.classList.add('mica-glow-active');
    }

    const rect = target.getBoundingClientRect();
    target.style.setProperty('--mica-x', `${event.clientX - rect.left}px`);
    target.style.setProperty('--mica-y', `${event.clientY - rect.top}px`);
  };

  private resetActive = (): void => {
    if (!this.activeElement) {
      return;
    }

    this.activeElement.classList.remove('mica-glow-active');
    this.activeElement.style.removeProperty('--mica-x');
    this.activeElement.style.removeProperty('--mica-y');
    this.activeElement = null;
  };
}
