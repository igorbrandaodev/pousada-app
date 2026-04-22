export interface Quarto {
  id: number;
  numero: string;
  tipo: 'standard' | 'luxo' | 'suite';
  capacidade: number;
  precoDiaria: number;
  status: 'disponivel' | 'ocupado' | 'manutencao' | 'limpeza';
  andar: number;
  amenidades: string[];
}

export interface Hospede {
  id: number;
  nome: string;
  documento: string;
  telefone: string;
  email: string;
  cidade: string;
  observacoes?: string;
}

export interface Reserva {
  id: number;
  idHospede: number;
  hospede?: Hospede;
  idQuarto: number;
  quarto?: Quarto;
  dataCheckin: Date;
  dataCheckout: Date;
  status: 'pendente' | 'confirmada' | 'checkin' | 'checkout' | 'cancelada';
  valorTotal: number;
  observacoes?: string;
  comandas?: Comanda[];
}

export interface CategoriaCardapio {
  id: number;
  nome: string;
  icone: string;
}

export interface Produto {
  id: number;
  nome: string;
  descricao: string;
  preco: number;
  categoria: string;
  idCategoria: number;
  disponivel: boolean;
  imagem?: string;
}

export interface ItemComanda {
  id: number;
  idProduto: number;
  produto?: Produto;
  quantidade: number;
  precoUnitario: number;
  observacao?: string;
  status: 'pendente' | 'preparando' | 'entregue';
}

export interface Comanda {
  id: number;
  idReserva: number;
  reserva?: Reserva;
  itens: ItemComanda[];
  dataAbertura: Date;
  dataFechamento?: Date;
  status: 'aberta' | 'fechada';
  total: number;
}
