import { AllocationViewModel } from "./AllocationViewModel";
import { HoldingViewModel } from "./HoldingViewModel";

export class PortfolioViewModel {
  accountCodes: string[] = [];
  holdings!: HoldingViewModel[];
  cashBalanceInGbp!: number;
  totalInvestmentsInGbp!: number;
  totalValueInGbp!: number;
  totalPriceAgeInDays!: number;
  allocations: AllocationViewModel[] = []
}
