import {
  ComponentFixture,
  TestBed,
  fakeAsync,
  tick,
} from '@angular/core/testing';
import { AppComponent } from './app.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { StoryService } from './services/story.service';
import { Stories } from './story.model';
import { FormsModule } from '@angular/forms';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('AppComponent', () => {
  let component: AppComponent;
  let fixture: ComponentFixture<AppComponent>;
  let storyService: jasmine.SpyObj<StoryService>;

  beforeEach(async () => {
    let storyServiceSpy = jasmine.createSpyObj('StoryService', [
      'getTopStories',
    ]);
    await TestBed.configureTestingModule({
      imports: [HttpClientTestingModule, FormsModule],
      providers: [{ provide: StoryService, useValue: storyServiceSpy }],
      declarations: [AppComponent],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
    storyService = TestBed.inject(StoryService) as jasmine.SpyObj<StoryService>;
  });

  it('should create the app component', () => {
    expect(component).toBeTruthy();
  });

  it('should call fetchStories on ngOnInit and set isLoading to true', () => {
    //Arrange
    storyService.getTopStories.and.returnValue(
      of({ stories: [], totalCount: 0 })
    );

    //Act
    component.ngOnInit();
    component.isLoading = true;
    fixture.detectChanges();

    //Assert
    expect(storyService.getTopStories).toHaveBeenCalled();
    expect(component.isLoading).toBeFalse();
  });

  it('should update component state when fetchStories is successful', () => {
    //Arrange
    let mockResponse: Stories = {
      stories: [
        { id: 1, title: 'Test Story 1', url: 'http://www.teststory1.com' },
        { id: 2, title: 'Test Story 2', url: 'http://www.teststory1.com' },
      ],
      totalCount: 2,
    };

    storyService.getTopStories.and.returnValue(of(mockResponse)); // Return an observable

    // Act
    component.fetchStories();
    component.isLoading = true;

    // Assert
    fixture.detectChanges();
    expect(component.stories).toEqual(mockResponse.stories);
    expect(component.totalStories).toBe(mockResponse.totalCount);
    expect(component.totalPages).toBe(1);
    expect(component.isLoading).toBeFalse();
    expect(component.errorMessage).toBe('');
  });

  it('should handle error when fetchStories fails', () => {
    //Arrange
    const errorMessage = 'Error fetching data';
    storyService.getTopStories.and.returnValue(throwError(errorMessage));

    //Act
    component.fetchStories();

    //Assert
    expect(component.errorMessage).toBe('Error fetching data');
    expect(component.isLoading).toBeFalse();
  });

  it('should update searchQuery and call fetchStories on search', () => {
    //Arrange
    const searchQuery = 'Test';
    const fetchStoriesSpy = spyOn(component, 'fetchStories').and.callThrough();
    storyService.getTopStories.and.returnValue(
      of({ stories: [], totalCount: 0 })
    );

    //Act
    component.onSearch(searchQuery);
    fixture.detectChanges();

    //Assert
    expect(component.currentPage).toEqual(1);
    expect(component.searchQuery).toEqual(searchQuery);
    fixture.whenStable();
    expect(fetchStoriesSpy).toHaveBeenCalled();
  });

  it('should decrement currentPage when previousPage is called', () => {
    //Arrange
    let mockResponse: Stories = {
      stories: [
        { id: 1, title: 'Test Story 1', url: 'http://www.teststory1.com' },
        { id: 2, title: 'Test Story 2', url: 'http://www.teststory1.com' },
      ],
      totalCount: 2,
    };
    component.currentPage = 2;
    const fetchStoriesSpy = spyOn(component, 'fetchStories').and.callThrough();
    storyService.getTopStories.and.returnValue(of(mockResponse));

    //Act
    component.previousPage();
    fixture.detectChanges();

    //Assert
    expect(component.currentPage).toBe(1);
    fixture.whenStable();
    expect(fetchStoriesSpy).toHaveBeenCalled();
  });

  it('should increment currentPage when nextPage is called', () => {
    //Arrange
    let mockResponse: Stories = {
      stories: [
        { id: 1, title: 'Test Story 1', url: 'http://www.teststory1.com' },
        { id: 2, title: 'Test Story 2', url: 'http://www.teststory1.com' },
      ],
      totalCount: 2,
    };
    component.currentPage = 1;
    component.totalPages = 3;
    const fetchStoriesSpy = spyOn(component, 'fetchStories').and.callThrough();
    storyService.getTopStories.and.returnValue(of(mockResponse));

    //Act
    component.nextPage();
    fixture.detectChanges();

    //Assert
    expect(component.currentPage).toBe(2);
    fixture.whenStable();
    expect(fetchStoriesSpy).toHaveBeenCalled();
  });

  it('should not increment currentPage if it is the last page', () => {
    //Arrange
    component.currentPage = 3;
    component.totalPages = 3;
    const fetchStoriesSpy = spyOn(component, 'fetchStories').and.callThrough();

    //Act
    component.nextPage();

    //Assert
    expect(component.currentPage).toBe(3);
    expect(fetchStoriesSpy).not.toHaveBeenCalled();
  });

  it('should update currentPage when goToPage is called with a valid page number', () => {
    //Arrange
    let mockResponse: Stories = {
      stories: [
        { id: 1, title: 'Test Story 1', url: 'http://www.teststory1.com' },
        { id: 2, title: 'Test Story 2', url: 'http://www.teststory1.com' },
      ],
      totalCount: 2,
    };
    const validPage = 2;
    component.totalPages = 5;
    component.currentPage = 1;
    const fetchStoriesSpy = spyOn(component, 'fetchStories').and.callThrough();
    storyService.getTopStories.and.returnValue(of(mockResponse));

    //Act
    component.goToPage(validPage);
    fixture.detectChanges();

    //Assert
    expect(component.currentPage).toBe(validPage);
    fixture.whenStable();
    expect(fetchStoriesSpy).toHaveBeenCalled();
  });

  it('should not update currentPage when goToPage is called with an invalid page number', () => {
    //Arrange
    const invalidPage = 10;
    component.totalPages = 5;
    const fetchStoriesSpy = spyOn(component, 'fetchStories').and.callThrough();

    //Act
    component.goToPage(invalidPage);

    //Assert
    expect(component.currentPage).toBe(1);
    expect(fetchStoriesSpy).not.toHaveBeenCalled();
  });

  it('should call fetchStories after debounce time when onSearch is called', fakeAsync(() => {
    //Arange
    let mockResponse: Stories = {
      stories: [
        { id: 1, title: 'Test Story 1', url: 'http://www.teststory1.com' },
        { id: 2, title: 'Test Story 2', url: 'http://www.teststory1.com' },
      ],
      totalCount: 2,
    };

    const searchQuery = 'Test';
    const fetchStoriesSpy = spyOn(component, 'fetchStories').and.callThrough();
    storyService.getTopStories.and.returnValue(of(mockResponse));

    //Act
    component.onSearch(searchQuery);
    tick(500);
    fixture.detectChanges();

    //Assert
    fixture.whenStable();
    expect(fetchStoriesSpy).toHaveBeenCalled();
  }));
});
