import React from 'react';
import * as api from '../Api';

export class ViewInfo extends React.Component {
  state = {
    doujinshi: null,
    error: null
  };

  async componentDidMount() {
    const doujinshi = await api.getDoujinshiAsync(this.props.id);

    if (doujinshi.error) {
      this.setState({
        error: doujinshi.message
      });
      return;
    }

    this.setState({ doujinshi });
  }

  render() {
    if (this.state.error)
      return (
        <span>{this.state.error}</span>
      );

    const doujinshi = this.state.doujinshi;

    if (!doujinshi)
      return null;

    return (
      <div>
        <div style={{
          height: '70vh',
          width: '100%',
          overflow: 'hidden',
          position: "relative",
          backgroundImage: `url(${api.getEndpoint(`doujinshi/${doujinshi.id}/variants/${doujinshi.variants[0].id}/images/0`)})`,
          backgroundSize: 'cover',
          backgroundPosition: 'center'
        }} />
        <h1>{doujinshi.name_original}</h1>
        <h4 style={{ marginTop: 0 }}>{doujinshi.name_english || 'anata dake no asashio desu'}</h4>
      </div>
    );
  }
}
