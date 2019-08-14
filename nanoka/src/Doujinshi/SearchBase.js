import React from 'react';
import * as api from '../Api';

export default class SearchBase extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      query: this.buildQuery(),
      result: null,
      error: null
    };
  }

  buildQuery() {
    return {
      limit: 100,
      sorting: []
    };
  }

  componentDidMount() {
    api.searchDoujinshi(this.state.query, {
      success: r => {
        this.setState({
          result: r.result
        });
      },
      error: e => {
        this.setState({
          error: e
        });
      }
    });
  }

  render() {
    if (this.state.error) {
      return (
        <span>Error: {this.state.error}</span>
      );
    }

    if (!this.state.result)
      return null;

    return (
      <span>Sorting: {this.state.result}</span>
    );
  }
}
