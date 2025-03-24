//Story Object
export interface Story {
  id: number;
  title: string;
  url: string;
}

//Main Response Object
export interface Stories {
  stories: Story[];
  totalCount: number;
}
