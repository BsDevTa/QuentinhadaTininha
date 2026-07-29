import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Acompanhamento, FormaPagamento, Prato, TamanhoRefeicao } from '../modelos/cardapio.model';

@Injectable({ providedIn: 'root' })
export class WhatsappService {
  private readonly formatadorMoeda = new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL'
  });

  criarLinkPedido(
    prato: Prato,
    tamanho: TamanhoRefeicao,
    formaPagamento: FormaPagamento,
    acompanhamentos: Acompanhamento[],
    valor: number,
    numero?: string
  ): string {
    const numeroRestaurante = numero ?? environment.whatsappRestaurante;
    const acompanhamentosTexto = acompanhamentos.length > 0
      ? acompanhamentos.map((acompanhamento) => `- ${acompanhamento.nome}`).join('\n')
      : 'sem acompanhamentos selecionados';
    const pagamentoTexto = formaPagamento === 'dinheiro_pix' ? 'Dinheiro ou Pix' : 'Cartão';

    const mensagem = `Olá, gostaria de fazer um pedido:\n\nPrato: ${prato.nome}\nTamanho: ${tamanho}\nForma de pagamento: ${pagamentoTexto}\n\nAcompanhamentos:\n${acompanhamentosTexto}\n\nValor: ${this.formatadorMoeda.format(valor)}`;

    return `https://wa.me/${numeroRestaurante.replace(/\D/g, '')}?text=${encodeURIComponent(mensagem)}`;
  }
}
