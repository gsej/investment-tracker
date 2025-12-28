import { Allocation } from "./Allocation";
import { TotalValue } from "./TotalValue";
import { Holding } from "./holding";

export class Portfolio {
  accountCodes!: string[];
  holdings!: Holding[];
  cashBalanceInGbp!: number;
  totalValue!: TotalValue;
  allocations!: Allocation[];
}
