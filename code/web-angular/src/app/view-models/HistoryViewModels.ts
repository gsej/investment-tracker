import { CommentViewModel } from "./CommentViewModel";
import { HistoryViewModel } from "./HistoryViewModel";
import { TrailingReturnViewModel } from "./TrailingReturnViewModel";

export class HistoryViewModels {
  items!: HistoryViewModel[];
  comments!: CommentViewModel[];
  trailingReturns!: TrailingReturnViewModel[];
}
