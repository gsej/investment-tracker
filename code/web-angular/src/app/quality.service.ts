import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class QualityService {

  _showQualityData = false;

  private _qualitySubject: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(this._showQualityData);
  public showQualityData$: Observable<boolean> = this._qualitySubject.asObservable();

  toggleShowQualityData() {
    this._showQualityData = !this._showQualityData;
    this._qualitySubject.next(this._showQualityData);
  }
}
