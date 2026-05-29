import { TrailingReturnViewModel } from './TrailingReturnViewModel';

export interface StockPriceViewModel {
  date: string;
  price: number;
}

export interface StockHistoryViewModel {
  stockSymbol: string;
  prices: StockPriceViewModel[];
  trailingReturns: TrailingReturnViewModel[];
}
