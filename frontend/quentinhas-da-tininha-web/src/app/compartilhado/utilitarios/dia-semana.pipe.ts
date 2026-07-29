import { Pipe, PipeTransform } from '@angular/core';
import { DiaSemana } from '../modelos/cardapio.model';

@Pipe({
  name: 'diaSemana',
  standalone: true
})
export class DiaSemanaPipe implements PipeTransform {
  transform(dia: DiaSemana): string {
    return ['Domingo', 'Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado'][dia] ?? 'Hoje';
  }
}
