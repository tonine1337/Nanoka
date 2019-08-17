import React from 'react';
import * as api from '../Api';

export class ViewInfo extends React.Component {
  state = {
    doujinshi: null,
    error: null
  };

  componentDidMount() {
    api.getDoujinshi(this.props.id, {
      success: r => this.setState({ doujinshi: r }),
      error: e => this.setState({ error: e })
    });
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
          backgroundImage: `url(${api.getEndpoint(`doujinshi/${doujinshi.id}/variants/${doujinshi.variants[0].id}/0`)})`,
          backgroundSize: 'cover',
          backgroundPosition: 'center'
        }} />
        <h1>{doujinshi.name_original}</h1>
        <h4 style={{ marginTop: 0 }}>{doujinshi.name_english || 'anata dake no asashio desu'}</h4>
      </div>
    );
  }
}
