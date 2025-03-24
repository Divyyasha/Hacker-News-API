import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Stories } from '../story.model';

@Injectable({
  providedIn: 'root',
})
export class StoryService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getTopStories(
    page: number = 1,
    limit: number = 20,
    searchString: string = ''
  ): Observable<Stories> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', limit.toString());

    if (searchString) {
      params = params.set('searchTitle', searchString);
    }

    return this.http.get<Stories>(this.apiUrl, { params });
  }
}
