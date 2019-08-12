import SearchBase from './SearchBase';

export class ListByName extends SearchBase {
  buildQuery() {
    return {
      sorting: [
        3, // original name
        4, // romanized name
        5  // english name
      ]
    };
  }
}
