import { TotalValue } from "./TotalValue";
import { Holding } from "./holding";

export class Portfolio {
  accountCode!: string;
  holdings!: Holding[];
  cashBalanceInGbp!: number;
  totalValue!: TotalValue;
}
