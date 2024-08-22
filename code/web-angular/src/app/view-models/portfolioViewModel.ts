import { HoldingViewModel } from "./holdingViewModel";

export class PortfolioViewModel {
  accountCode!: string;
  holdings!: HoldingViewModel[];
  cashBalanceInGbp!: number;
  totalValueInGbp!: number;
  totalPriceAgeInDays!: number;
}
