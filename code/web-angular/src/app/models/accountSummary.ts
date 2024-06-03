import { TotalValue } from "./TotalValue";
import { Holding } from "./holding";


// TODO: why do these all have to be labelled nullable
export class StockPrice {
  price!: number;
  currency!: string;
  ageInDays!: number;
}

export class AccountSummary {
  holdings!: Holding[];
  cashBalanceInGbp!: number;
  totalValue!: TotalValue;
}


