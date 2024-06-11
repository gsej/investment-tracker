import { HoldingViewModel } from "./holdingViewModel";

export class AccountSummaryViewModel {
  holdings!: HoldingViewModel[];
  cashBalanceInGbp!: number;
  totalValueInGbp!: number;
  totalPriceAgeInDays!: number;
}
