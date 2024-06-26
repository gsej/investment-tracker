import { StockPrice } from "./StockPrice";

export class Holding {
  stockSymbol!: string;
  stockDescription!: string;
  quantity!: number;
  stockPrice!: StockPrice;
  valueInGbp!: number;
  comment!: string;
}
