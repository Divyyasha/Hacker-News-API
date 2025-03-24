import { TestBed } from '@angular/core/testing';
import { StoryService } from './story.service';
import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';
import { environment } from '../../environments/environment';

describe('StoryService', () => {
  let service: StoryService;
  let httpMock: HttpTestingController;
  const apiUrl = environment.apiUrl;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [StoryService],
    });

    service = TestBed.inject(StoryService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should make a GET request with correct parameters when getTopStories is called', () => {
    //Arrange
    const mockResponse = { stories: [], totalCount: 100 };
    const page = 1;
    const limit = 20;
    const searchString = 'Test';

    //Act
    service.getTopStories(page, limit, searchString).subscribe((response) => {
      expect(response).toEqual(mockResponse);
    });

    //Assert
    const req = httpMock.expectOne(
      (req) => req.url === apiUrl && req.method === 'GET'
    );
    expect(req.request.params.has('page')).toBeTrue();
    expect(req.request.params.get('page')).toBe(page.toString());
    expect(req.request.params.has('pageSize')).toBeTrue();
    expect(req.request.params.get('pageSize')).toBe(limit.toString());
    expect(req.request.params.has('searchTitle')).toBeTrue();
    expect(req.request.params.get('searchTitle')).toBe(searchString);
    req.flush(mockResponse);
  });

  it('should make a GET request with custom parameters when getTopStories is called', () => {
    //Arrange
    const mockResponse = { stories: [], totalCount: 100 };
    const page = 2;
    const limit = 10;
    const searchString = 'Test';

    // Act
    service.getTopStories(page, limit, searchString).subscribe((response) => {
      expect(response).toEqual(mockResponse);
    });

    //Assert
    const req = httpMock.expectOne(
      (req) => req.url === apiUrl && req.method === 'GET'
    );
    expect(req.request.params.get('page')).toBe(page.toString());
    expect(req.request.params.get('pageSize')).toBe(limit.toString());
    expect(req.request.params.get('searchTitle')).toBe(searchString);
    req.flush(mockResponse);
  });

  it('should handle HTTP errors gracefully', () => {
    //Arrange
    const errorMessage = 'Error fetching data';

    //Act
    service.getTopStories(1, 20).subscribe(
      () => {},
      (error) => {
        expect(error.status).toBe(500);
        expect(error.statusText).toBe(errorMessage);
      }
    );

    //Assert
    const req = httpMock.expectOne(
      (req) => req.url === apiUrl && req.method === 'GET'
    );
    req.flush(errorMessage, { status: 500, statusText: errorMessage });
  });

  it('should use default values for page and limit when no parameters are passed', () => {
    //Arrange
    const mockResponse = { stories: [], totalCount: 100 };

    //Act
    service.getTopStories().subscribe((response) => {
      expect(response).toEqual(mockResponse);
    });

    //Assert
    const req = httpMock.expectOne(
      (req) => req.url === apiUrl && req.method === 'GET'
    );
    expect(req.request.params.get('page')).toBe('1');
    expect(req.request.params.get('pageSize')).toBe('20');
    req.flush(mockResponse);
  });

  it('should call the correct URL for the GET request', () => {
    //Arrange
    const mockResponse = { stories: [], totalCount: 100 };

    //Act
    service.getTopStories(1, 20).subscribe((response) => {
      expect(response).toEqual(mockResponse);
    });

    //Assert
    const req = httpMock.expectOne((request) => {
      return (
        request.url === apiUrl &&
        request.params.has('page') &&
        request.params.get('page') === '1' &&
        request.params.has('pageSize') &&
        request.params.get('pageSize') === '20'
      );
    });
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  afterEach(() => {
    httpMock.verify();
  });
});
