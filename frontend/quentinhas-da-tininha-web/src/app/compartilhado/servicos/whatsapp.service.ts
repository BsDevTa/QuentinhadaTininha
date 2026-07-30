import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Acompanhamento, FormaPagamento, Prato, TamanhoRefeicao, TipoEntrega } from '../modelos/cardapio.model';

export interface DetalhesPedidoWhatsapp {
  nomeCliente?: string | null;
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
    const nomeCliente = this.normalizarTexto(detalhes?.nomeCliente) ?? 'cliente';
    const pagamentoTexto = this.rotuloPagamento(formaPagamento);
    const blocos = [
      '🍽️ *Quentinhas da Tininha*',
      `*Olá, ${nomeCliente}!*`,
      'Seu pedido foi registrado com sucesso.',
      '━━━━━━━━━━━━━━━━━━',
      `*🍛 Prato:*\n*${prato.nome}*`,
      `*📏 Tamanho:*\n*${this.rotuloTamanho(tamanho)}*`,
      `*🥗 Acompanhamentos:*\n${this.criarTextoAcompanhamentos(acompanhamentos)}`,
      `*💳 Forma de pagamento:*\n*${pagamentoTexto}*`,
      this.criarTextoTroco(formaPagamento, detalhes),
      this.criarTextoEntrega(detalhes),
      `*💰 Total:*\n*${this.formatadorMoeda.format(valor)}*`,
      '━━━━━━━━━━━━━━━━━━',
      'Obrigado pela preferência ❤️',
      'Em breve seu pedido será preparado.'
    ].filter((bloco): bloco is string => Boolean(bloco));

    const mensagem = blocos.join('\n\n');

    return `https://wa.me/${numeroRestaurante.replace(/\D/g, '')}?text=${encodeURIComponent(mensagem)}`;
  }

  private criarTextoAcompanhamentos(acompanhamentos: Acompanhamento[]): string {
    if (acompanhamentos.length === 0) {
      return '*Sem acompanhamentos selecionados*';
    }

    return acompanhamentos.map((acompanhamento) => `- *${acompanhamento.nome}*`).join('\n');
  }

  private rotuloTamanho(tamanho: TamanhoRefeicao): string {
    return tamanho === 'P' ? 'P - Pequena' : 'G - Grande';
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
      return '*💵 Troco:*\n*Não precisa*';
    }

    return `*💵 Troco:*\n*Para ${this.formatadorMoeda.format(detalhes.valorTroco ?? 0)}*`;
  }

  private criarTextoEntrega(detalhes?: DetalhesPedidoWhatsapp): string {
    if (detalhes?.tipoEntrega !== 'entrega') {
      return '*🚚 Entrega:*\n*Retirada no local*';
    }

    const endereco = [
      detalhes.enderecoEntrega,
      detalhes.bairro,
      detalhes.referencia ? `Referência: ${detalhes.referencia}` : null
    ]
      .map((texto) => this.normalizarTexto(texto))
      .filter((texto): texto is string => Boolean(texto))
      .join('\n');

    return `*🚚 Entrega:*\n*Entrega*\n\n*📍 Endereço:*\n*${endereco}*`;
  }

  private normalizarTexto(texto?: string | null): string | null {
    const valor = texto?.trim();
    return valor ? valor : null;
  }
}
