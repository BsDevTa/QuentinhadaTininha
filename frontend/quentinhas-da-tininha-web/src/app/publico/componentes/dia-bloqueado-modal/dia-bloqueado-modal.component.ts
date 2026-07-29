import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  Output,
  ViewChild
} from '@angular/core';

@Component({
  selector: 'app-dia-bloqueado-modal',
  standalone: true,
  template: `
    <div class="modal-dia-bloqueado" role="presentation">
      <button class="modal-dia-bloqueado__fundo" type="button" aria-label="Fechar aviso" (click)="fechar.emit()"></button>
      <section
        #painel
        id="modal-dia-bloqueado"
        class="modal-dia-bloqueado__conteudo"
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-dia-titulo"
        tabindex="-1"
      >
        <button class="modal-dia-bloqueado__fechar" type="button" aria-label="Fechar aviso" (click)="fechar.emit()">×</button>
        <span class="modal-dia-bloqueado__icone" aria-hidden="true">🔒</span>
        <h3 id="modal-dia-titulo">{{ nome }} bloqueado</h3>
        <p>Essa data ainda não foi liberada para pedidos.</p>
        <div class="modal-dia-bloqueado__motivo">
          <strong>Motivo:</strong>
          <span>{{ motivo }}</span>
        </div>
        @if (data) {
          <small>{{ data }}</small>
        }
        <button class="botao" type="button" (click)="fechar.emit()">Entendi</button>
      </section>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DiaBloqueadoModalComponent implements AfterViewInit {
  @ViewChild('painel') private painel?: ElementRef<HTMLElement>;

  @Input({ required: true }) nome = '';
  @Input({ required: true }) motivo = '';
  @Input({ required: true }) data = '';
  @Output() readonly fechar = new EventEmitter<void>();

  ngAfterViewInit(): void {
    setTimeout(() => this.painel?.nativeElement.focus());
  }

  @HostListener('document:keydown.escape')
  protected fecharPorEsc(): void {
    this.fechar.emit();
  }
}
