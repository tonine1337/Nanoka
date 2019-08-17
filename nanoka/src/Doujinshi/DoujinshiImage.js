import ApiImage from '../ApiImage';
import * as api from '../Api';

export class DoujinshiImage extends ApiImage {
  async getImageAsync() {
    return await api.downloadImageAsync(this.props.doujinshi.id, this.props.variant.id, this.props.index);
  }
}
