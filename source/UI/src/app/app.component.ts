import { Component } from '@angular/core';
import { StoryService } from './services/story.service';
import { Story } from './story.model';
import { Subject, debounceTime } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {
  title = 'TopStories';
  stories: Story[] = [];
  isLoading = false;
  errorMessage: string = '';
  limit: number = 20;
  currentPage: number = 1;
  totalStories: number = 0;
  totalPages: number = 0;
  searchQuery: string = '';
  private searchSubject = new Subject<string>();

  constructor(private storyService: StoryService) {}

  ngOnInit(): void {
    this.isLoading = true;
    this.fetchStories();
    this.searchSubject.pipe(debounceTime(500)).subscribe((query) => {
      this.fetchStories();
    });
  }

  //Method calling API service to fetch stories and subscribing to the response
  fetchStories(): void {
    this.isLoading = true;
    this.storyService
      .getTopStories(this.currentPage, this.limit, this.searchQuery)
      .subscribe({
        next: (response) => {
          this.stories = response.stories;
          this.isLoading = false;
          this.totalStories = response.totalCount;
          this.totalPages = Math.ceil(this.totalStories / this.limit);
        },
        error: (err) => {
          this.errorMessage = 'Error fetching data';
          this.isLoading = false;
        },
      });
  }

  // Method on search by title
  onSearch(query: string): void {
    this.currentPage = 1;
    this.searchQuery = query;
    this.searchSubject.next(query);
  }

  // Method to go to the previouPage
  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.fetchStories();
    }
  }

  // Method to go to the next currentPage
  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.fetchStories();
    }
  }

  // Method to go to a specific currentPage
  goToPage(currentPage: number): void {
    if (currentPage >= 1 && currentPage <= this.totalPages) {
      this.currentPage = currentPage;
      this.fetchStories();
    }
  }
}
