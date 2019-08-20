import SearchBase from './SearchBase';

export class ListByViewed extends SearchBase {
  buildQuery() {
    return {
      //todo: view count is not implemented
      sorting: [
        ~1 // upload time (desc)
      ]
    };
  }
}
