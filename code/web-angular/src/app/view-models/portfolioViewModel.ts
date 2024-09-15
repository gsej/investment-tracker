import { HoldingViewModel } from "./holdingViewModel";



export class Allocation {
  name!: string;
  value!: number;
  percentage!: number;
}


export class PortfolioViewModel {
  accountCode!: string;
  holdings!: HoldingViewModel[];
  cashBalanceInGbp!: number;
  totalInvestmentsInGbp!: number;
  totalValueInGbp!: number;
  totalPriceAgeInDays!: number;
  allocations: Allocation[] = []
}
