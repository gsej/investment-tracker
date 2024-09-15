import { StockPrice } from "./StockPrice";

export class Holding {
  stockSymbol!: string;
  stockDescription!: string;
  allocation!: string;
  quantity!: number;
  stockPrice!: StockPrice;
  valueInGbp!: number;
  comment!: string;
}
