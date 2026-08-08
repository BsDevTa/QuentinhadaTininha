import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Acompanhamento, FormaPagamento, Prato, TamanhoRefeicao, TipoEntrega } from '../modelos/cardapio.model';

export interface DetalhesPedidoWhatsapp {
  pedidoId?: string | null;
  nomeCliente?: string | null;
  precisaTroco?: boolean;
  valorTroco?: number | null;
  tipoEntrega?: TipoEntrega;
  observacaoItem?: string | null;
  subtotal?: number | null;
  valorFrete?: number | null;
  logradouro?: string | null;
  numero?: string | null;
  complemento?: string | null;
  enderecoEntrega?: string | null;
  bairro?: string | null;
  cidade?: string | null;
  estado?: string | null;
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
    const nomeCliente = this.normalizarTexto(detalhes?.nomeCliente) ?? 'Cliente';
    const pedidoId = this.normalizarTexto(detalhes?.pedidoId);
    const pagamentoTexto = this.rotuloPagamento(formaPagamento);
    const subtotal = detalhes?.subtotal ?? valor;
    const blocos = [
      '*🍽️ Pedido — Quentinhas da Tininha*',
      pedidoId ? `*Pedido:*\n#${pedidoId.slice(-6).toUpperCase()}` : null,
      `*Cliente:*\n${nomeCliente}`,
      '━━━━━━━━━━━━━━━━━━',
      `*🍛 Prato:*\n${prato.nome}`,
      `*📏 Tamanho:*\n${this.rotuloTamanho(tamanho)}`,
      `*🥗 Acompanhamentos:*\n${this.criarTextoAcompanhamentos(acompanhamentos)}`,
      this.criarTextoObservacao(detalhes?.observacaoItem),
      '━━━━━━━━━━━━━━━━━━',
      `*🚚 Tipo do pedido:*\n${detalhes?.tipoEntrega === 'entrega' ? 'Entrega' : 'Retirada no local'}`,
      this.criarTextoEntrega(detalhes),
      `*💳 Forma de pagamento:*\n${pagamentoTexto}`,
      this.criarTextoTroco(formaPagamento, detalhes),
      `*💵 Subtotal:*\n${this.formatadorMoeda.format(subtotal)}`,
      this.criarTextoFrete(detalhes),
      `*💰 Total:*\n${this.formatadorMoeda.format(valor)}`,
      '━━━━━━━━━━━━━━━━━━',
      'Obrigado pela preferência.',
      'Em breve seu pedido será preparado.'
    ].filter((bloco): bloco is string => Boolean(bloco));

    const mensagem = blocos.join('\n\n');

    return `https://wa.me/${numeroRestaurante.replace(/\D/g, '')}?text=${encodeURIComponent(mensagem)}`;
  }

  private criarTextoAcompanhamentos(acompanhamentos: Acompanhamento[]): string {
    if (acompanhamentos.length === 0) {
      return 'Sem acompanhamentos selecionados';
    }

    return acompanhamentos.map((acompanhamento) => `- ${acompanhamento.nome}`).join('\n');
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
      return '*💵 Troco:*\nNão precisa';
    }

    return `*💵 Troco:*\nPara ${this.formatadorMoeda.format(detalhes.valorTroco ?? 0)}`;
  }

  private criarTextoEntrega(detalhes?: DetalhesPedidoWhatsapp): string {
    if (detalhes?.tipoEntrega !== 'entrega') {
      return '';
    }

    const endereco =
      this.normalizarTexto(detalhes.enderecoEntrega) ??
      this.montarEndereco(detalhes);

    const blocos = [
      endereco ? `*📍 Endereço:*\n${endereco}` : null,
      this.normalizarTexto(detalhes.bairro)
        ? `*🏘️ Bairro:*\n${this.normalizarTexto(detalhes.bairro)}`
        : null,
      this.normalizarTexto(detalhes.cidade) && this.normalizarTexto(detalhes.estado)
        ? `*🏙️ Cidade/UF:*\n${this.normalizarTexto(detalhes.cidade)} - ${this.normalizarTexto(detalhes.estado)}`
        : null,
      this.normalizarTexto(detalhes.referencia)
        ? `*📌 Referência:*\n${this.normalizarTexto(detalhes.referencia)}`
        : null
    ].filter((bloco): bloco is string => Boolean(bloco));

    return blocos.join('\n\n');
  }

  private criarTextoObservacao(observacao?: string | null): string {
    const texto = this.normalizarTexto(observacao);
    return texto ? `*📝 Observação:*\n${texto}` : '';
  }

  private criarTextoFrete(detalhes?: DetalhesPedidoWhatsapp): string {
    if (detalhes?.tipoEntrega !== 'entrega' || detalhes.valorFrete === null || detalhes.valorFrete === undefined) {
      return '';
    }

    return `*🛵 Frete:*\n${this.formatadorMoeda.format(detalhes.valorFrete)}`;
  }

  private montarEndereco(detalhes?: DetalhesPedidoWhatsapp): string | null {
    const logradouro = this.normalizarTexto(detalhes?.logradouro);
    const numero = this.normalizarTexto(detalhes?.numero);
    const complemento = this.normalizarTexto(detalhes?.complemento);

    if (!logradouro || !numero) {
      return null;
    }

    return complemento ? `${logradouro}, ${numero} - ${complemento}` : `${logradouro}, ${numero}`;
  }

  private normalizarTexto(texto?: string | null): string | null {
    const valor = texto?.trim();
    return valor ? valor : null;
  }
}
