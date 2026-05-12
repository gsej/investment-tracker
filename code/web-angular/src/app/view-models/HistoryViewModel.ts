export class HistoryViewModel {
  date!: string;
  valueInGbp!: number;
  netInflows!: number;
  totalPriceAgeInDays!: number;
  recordedTotalValueInGbp!: number;
  recordedTotalValueSource!: string;
  discrepancyRatio!: number;
  differenceToPreviousDay!: number;
  differenceRatio!: number;
  comment!: string;
  units!: UnitViewModel;
}

export class UnitViewModel {
  date!: string;
  numberOfUnits!: number;
  valueInGbpPerUnit!: number;
}
