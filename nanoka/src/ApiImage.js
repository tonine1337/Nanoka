import React from 'react';
import { Loader, Image, Placeholder } from 'semantic-ui-react'

export class ApiImage extends React.Component {
  state = {
    url: null,
    error: null
  };

  async getImageAsync() {
    return null;
  }

  async componentDidMount() {
    try {
      const url = await this.getImageAsync();

      this.setState({
        url: url
      });
    }
    catch (error) {
      this.setState({
        error: error
      });
    }
  }

  render() {
    if (this.state.error) {
      return (
        <span>error</span>
      );
    }

    if (!this.state.url) {
      return (
        <Placeholder>
          <Placeholder.Image square />
        </Placeholder>
      );
    }

    return (
      <div>
        <Image src={this.state.url} style={this.props.style} />
      </div>
    );
  }
}

export default ApiImage;
