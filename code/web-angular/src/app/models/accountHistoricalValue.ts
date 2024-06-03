export class AccountHistoricalValue {
  // represents a total value for an account calculated from prices etc.
  date!: string;
  accountCode!: string;
  valueInGbp!: number;
  totalPriceAgeInDays!: number;
  recordedTotalValueInGbp!: number;
  discrepancyPercentage!: number;
  comment!: string;
}
