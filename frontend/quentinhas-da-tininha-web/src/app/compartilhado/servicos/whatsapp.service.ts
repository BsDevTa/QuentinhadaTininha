import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Acompanhamento, FormaPagamento, Prato, TamanhoRefeicao, TipoEntrega } from '../modelos/cardapio.model';

export interface DetalhesPedidoWhatsapp {
  precisaTroco?: boolean;
  valorTroco?: number | null;
  tipoEntrega?: TipoEntrega;
  enderecoEntrega?: string | null;
  bairro?: string | null;
  referencia?: string | null;
}

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
    numero?: string,
    detalhes?: DetalhesPedidoWhatsapp
  ): string {
    const numeroRestaurante = numero ?? environment.whatsappRestaurante;
    const acompanhamentosTexto = acompanhamentos.length > 0
      ? acompanhamentos.map((acompanhamento) => `- ${acompanhamento.nome}`).join('\n')
      : 'sem acompanhamentos selecionados';
    const pagamentoTexto = this.rotuloPagamento(formaPagamento);
    const trocoTexto = this.criarTextoTroco(formaPagamento, detalhes);
    const entregaTexto = this.criarTextoEntrega(detalhes);

    const mensagem = `Olá, gostaria de fazer um pedido:\n\nPrato: ${prato.nome}\nTamanho: ${tamanho}\nForma de pagamento: ${pagamentoTexto}${trocoTexto}\n${entregaTexto}\nAcompanhamentos:\n${acompanhamentosTexto}\n\nValor: ${this.formatadorMoeda.format(valor)}`;

    return `https://wa.me/${numeroRestaurante.replace(/\D/g, '')}?text=${encodeURIComponent(mensagem)}`;
  }

  private rotuloPagamento(formaPagamento: FormaPagamento): string {
    switch (formaPagamento) {
      case 'dinheiro':
        return 'Dinheiro';
      case 'pix':
        return 'PIX';
      case 'cartao':
        return 'Cartão';
    }
  }

  private criarTextoTroco(
    formaPagamento: FormaPagamento,
    detalhes?: DetalhesPedidoWhatsapp
  ): string {
    if (formaPagamento !== 'dinheiro') {
      return '';
    }

    if (!detalhes?.precisaTroco) {
      return '\nPrecisa de troco: Não';
    }

    return `\nPrecisa de troco: Sim\nTroco para: ${this.formatadorMoeda.format(detalhes.valorTroco ?? 0)}`;
  }

  private criarTextoEntrega(detalhes?: DetalhesPedidoWhatsapp): string {
    if (detalhes?.tipoEntrega !== 'entrega') {
      return '\nTipo de entrega: Retirada\n';
    }

    return `\nTipo de entrega: Entrega\nEndereço: ${detalhes.enderecoEntrega}\nBairro: ${detalhes.bairro}\nReferência: ${detalhes.referencia}\n`;
  }
}
