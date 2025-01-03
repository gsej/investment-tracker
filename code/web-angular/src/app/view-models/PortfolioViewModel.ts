import { AllocationViewModel } from "./AllocationViewModel";
import { HoldingViewModel } from "./HoldingViewModel";

export class PortfolioViewModel {
  accountCode!: string;
  holdings!: HoldingViewModel[];
  cashBalanceInGbp!: number;
  totalInvestmentsInGbp!: number;
  totalValueInGbp!: number;
  totalPriceAgeInDays!: number;
  allocations: AllocationViewModel[] = []
}
