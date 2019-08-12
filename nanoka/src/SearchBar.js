import React from 'react';
import { withRouter } from "react-router-dom";
import { Button, Icon, Input } from 'semantic-ui-react';
import * as qs from 'qs';

class SearchBar extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      query: qs.parse(window.location.search, {
        ignoreQueryPrefix: true
      }).q
    };
  }

  render() {
    return (
      <Input className="action left icon">
        <Icon name="search" />
        <input
          type="text"
          value={this.state.query}
          style={{ minWidth: "30rem" }}
          onChange={e => this.setState({ query: e.target.value })}
          onKeyUp={() => {
            if (this.state.query)
              this.props.history.push(`/search?q=${this.state.query}`);
            else
              this.props.history.push('/');
          }}
        />
        <Button className="primary">Search</Button>
      </Input>
    );
  }
}

export default withRouter(SearchBar);
