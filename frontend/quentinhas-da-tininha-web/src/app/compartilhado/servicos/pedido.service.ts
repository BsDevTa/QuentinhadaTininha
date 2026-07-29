import { Injectable } from '@angular/core';
import {
  Acompanhamento,
  FormaPagamento,
  GrupoAcompanhamento,
  PersonalizacaoPedido,
  Prato,
  TamanhoRefeicao
} from '../modelos/cardapio.model';
import { acompanhamentosMock } from '../dados/cardapio.mock';

@Injectable({ providedIn: 'root' })
export class PedidoService {
  private readonly acompanhamentosPorId = new Map(
    acompanhamentosMock.map((acompanhamento) => [acompanhamento.id, acompanhamento])
  );

  obterGrupo(prato: Prato): GrupoAcompanhamento {
    if (prato.grupoAcompanhamento) {
      const acompanhamentos = [...prato.grupoAcompanhamento.acompanhamentos]
        .sort((a, b) => (a.ordemExibicao ?? 0) - (b.ordemExibicao ?? 0));

      return {
        titulo: prato.grupoAcompanhamento.nome,
        tipoFeijao: acompanhamentos.filter((acompanhamento) =>
          acompanhamento.tipoSelecao === 'EXCLUSIVA' &&
          acompanhamento.grupoExclusivo === 'TIPO_FEIJAO'
        ),
        itens: acompanhamentos.filter((acompanhamento) =>
          acompanhamento.tipoSelecao !== 'EXCLUSIVA' ||
          acompanhamento.grupoExclusivo !== 'TIPO_FEIJAO'
        )
      };
    }

    const item = (id: string): Acompanhamento => this.acompanhamentosPorId.get(id)!;

    const grupos: Record<string, GrupoAcompanhamento> = {
      padrao: {
        tipo: 'padrao',
        titulo: 'Acompanhamentos',
        tipoFeijao: [item('feijao-caldo'), item('feijao-tropeiro')],
        itens: [item('arroz'), item('macarrao'), item('salada')]
      },
      comida_baiana: {
        tipo: 'comida_baiana',
        titulo: 'Comida baiana',
        tipoFeijao: [],
        itens: [item('arroz'), item('feijao-fradinho'), item('caruru'), item('vatapa'), item('farofa')]
      },
      cozido: {
        tipo: 'cozido',
        titulo: 'Cozido',
        tipoFeijao: [],
        itens: [item('arroz'), item('pirao')]
      },
      sarapatel_xinxim: {
        tipo: 'sarapatel_xinxim',
        titulo: 'Sarapatel e xinxim',
        tipoFeijao: [item('feijao-caldo'), item('feijao-tropeiro')],
        itens: [item('arroz')]
      },
      arrumadinho: {
        tipo: 'arrumadinho',
        titulo: 'Arrumadinho',
        tipoFeijao: [],
        itens: [item('arroz'), item('farofa'), item('feijao-fradinho'), item('salada-vinagrete')]
      }
    };

    return grupos[prato.tipoGrupoAcompanhamento ?? 'padrao'];
  }

  calcularPreco(prato: Prato, tamanho: TamanhoRefeicao, formaPagamento: FormaPagamento): number {
    if (tamanho === 'P' && formaPagamento === 'dinheiro_pix') {
      return prato.precos.pequenaDinheiroPix;
    }

    if (tamanho === 'P' && formaPagamento === 'cartao') {
      return prato.precos.pequenaCartao;
    }

    if (tamanho === 'G' && formaPagamento === 'dinheiro_pix') {
      return prato.precos.grandeDinheiroPix;
    }

    return prato.precos.grandeCartao;
  }

  listarAcompanhamentosSelecionados(
    personalizacao: PersonalizacaoPedido,
    grupo?: GrupoAcompanhamento
  ): Acompanhamento[] {
    if (grupo) {
      const acompanhamentos = [...grupo.tipoFeijao, ...grupo.itens];
      const selecionados = acompanhamentos.filter((acompanhamento) =>
        acompanhamento.estaDisponivel &&
        personalizacao.acompanhamentoIds.includes(acompanhamento.id)
      );

      if (personalizacao.tipoFeijaoId) {
        const feijao = grupo.tipoFeijao.find((item) =>
          item.id === personalizacao.tipoFeijaoId &&
          item.estaDisponivel
        );
        return feijao ? [feijao, ...selecionados] : selecionados;
      }

      return selecionados;
    }

    const selecionados = personalizacao.acompanhamentoIds
      .map((id) => this.acompanhamentosPorId.get(id))
      .filter((acompanhamento): acompanhamento is Acompanhamento => Boolean(acompanhamento));

    if (personalizacao.tipoFeijaoId) {
      const feijao = this.acompanhamentosPorId.get(personalizacao.tipoFeijaoId);
      if (feijao) {
        return [feijao, ...selecionados];
      }
    }

    return selecionados;
  }

  rotuloTamanho(tamanho: TamanhoRefeicao): string {
    return tamanho === 'P' ? 'P - Pequena' : 'G - Grande';
  }

  rotuloPagamento(formaPagamento: FormaPagamento): string {
    return formaPagamento === 'dinheiro_pix' ? 'Dinheiro ou Pix' : 'Cartão';
  }
}
