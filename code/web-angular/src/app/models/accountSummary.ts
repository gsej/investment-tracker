import { TotalValue } from "./TotalValue";
import { Holding } from "./holding";

export class AccountSummary {
  holdings!: Holding[];
  cashBalanceInGbp!: number;
  totalValue!: TotalValue;
}
