import { DOCUMENT } from '@angular/common';
import {
  ApplicationRef,
  ComponentRef,
  EnvironmentInjector,
  Injectable,
  Type,
  createComponent,
  inject
} from '@angular/core';

export interface OverlayHandle<TComponent> {
  readonly componentRef: ComponentRef<TComponent>;
  close(): void;
}

@Injectable({ providedIn: 'root' })
export class OverlayService {
  private readonly appRef = inject(ApplicationRef);
  private readonly documento = inject(DOCUMENT);
  private readonly environmentInjector = inject(EnvironmentInjector);

  private overlaysAbertos = 0;
  private appRoot?: HTMLElement;
  private estadoAppRoot?: { ariaHidden: string | null; inert: boolean };

  open<TComponent>(
    component: Type<TComponent>,
    inputs: Record<string, unknown> = {}
  ): OverlayHandle<TComponent> {
    const hostElement = this.documento.createElement('div');
    hostElement.className = 'app-overlay-host';
    hostElement.setAttribute('data-app-overlay-host', '');
    this.documento.body.appendChild(hostElement);

    const componentRef = createComponent(component, {
      environmentInjector: this.environmentInjector,
      hostElement
    });

    this.appRef.attachView(componentRef.hostView);

    for (const [input, value] of Object.entries(inputs)) {
      componentRef.setInput(input, value);
    }

    this.travarPagina();
    componentRef.changeDetectorRef.detectChanges();

    let fechado = false;

    return {
      componentRef,
      close: () => {
        if (fechado) {
          return;
        }

        fechado = true;
        this.appRef.detachView(componentRef.hostView);
        componentRef.destroy();
        hostElement.remove();
        this.liberarPagina();
      }
    };
  }

  private travarPagina(): void {
    if (this.overlaysAbertos === 0) {
      this.documento.body.classList.add('pedido-modal-aberto');
      this.documento.body.classList.add('app-overlay-aberto');

      const appRoot = this.documento.querySelector('app-root');
      if (appRoot instanceof HTMLElement) {
        this.appRoot = appRoot;
        this.estadoAppRoot = {
          ariaHidden: appRoot.getAttribute('aria-hidden'),
          inert: appRoot.inert
        };
        appRoot.inert = true;
        appRoot.setAttribute('aria-hidden', 'true');
      }
    }

    this.overlaysAbertos += 1;
  }

  private liberarPagina(): void {
    this.overlaysAbertos = Math.max(0, this.overlaysAbertos - 1);

    if (this.overlaysAbertos > 0) {
      return;
    }

    this.documento.body.classList.remove('pedido-modal-aberto');
    this.documento.body.classList.remove('app-overlay-aberto');

    if (this.appRoot && this.estadoAppRoot) {
      this.appRoot.inert = this.estadoAppRoot.inert;

      if (this.estadoAppRoot.ariaHidden === null) {
        this.appRoot.removeAttribute('aria-hidden');
      } else {
        this.appRoot.setAttribute('aria-hidden', this.estadoAppRoot.ariaHidden);
      }
    }

    this.appRoot = undefined;
    this.estadoAppRoot = undefined;
  }
}
