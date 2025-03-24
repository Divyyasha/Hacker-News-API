# Top Stories API

## Overview
This API fetches the top 200 stories from the Hacker News API, and provides a search functionality for stories by title.

## Endpoints

- `GET /api/stories`: Fetches the top 200 stories from Hacker News.
- `GET /api/stories/search?query={searchTerm}`: Searches for stories by title.

## Caching
- The API caches the top stories for 30 minutes to avoid making frequent API calls to Hacker News.

## External APIs
- This API calls the Hacker News API to fetch the top 200 stories.

## Swagger
- The API has Swagger enabled at `/swagger` for interactive documentation.

## Error Handling
- The API returns a `500` error for any internal server issues with the error message.