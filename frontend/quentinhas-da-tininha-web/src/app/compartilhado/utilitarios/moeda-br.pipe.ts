import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'moedaBr',
  standalone: true
})
export class MoedaBrPipe implements PipeTransform {
  private readonly formatador = new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL'
  });

  transform(valor: number): string {
    return this.formatador.format(valor);
  }
}
