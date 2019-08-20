import SearchBase from './SearchBase';

export class ListByName extends SearchBase {
  buildQuery() {
    return {
      sorting: [
        1 // upload time
      ]
    };
  }
}
