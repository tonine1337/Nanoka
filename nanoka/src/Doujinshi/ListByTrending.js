import SearchBase from './SearchBase';

export class ListByTrending extends SearchBase {
  buildQuery() {
    return {
      sorting: [
        ~3, // score
        ~1 // upload time (desc)
      ]
    };
  }
}
