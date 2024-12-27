export class HistoryViewModel {
  date!: string;
  accountCode!: string;
  valueInGbp!: number;
  contributions!: number;
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



