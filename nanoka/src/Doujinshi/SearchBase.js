import React from 'react';
import { Link } from 'react-router-dom';
import { Grid, Card, List, Popup } from 'semantic-ui-react';
import * as api from '../Api';
import { DoujinshiImage } from './DoujinshiImage';
import { fixMetaType } from './ViewInfo';

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

  async componentDidMount() {
    const r = await api.searchDoujinshiAsync(this.state.query);

    this.setState({
      result: r.result
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
      <div>
        <Grid doubling columns={5}>
          {this.state.result.map(doujinshi => {
            //todo: smart variant selection based on device language
            const variant = doujinshi.variants[0];

            return (
              <Grid.Column verticalAlign="middle">
                <Popup
                  trigger={
                    <Link to={`/doujinshi/${doujinshi.id}`}>
                      <Card link fluid>
                        <DoujinshiImage doujinshi={doujinshi} variant={variant} index={0} />
                        <Card.Content>
                          <Card.Header>{variant.name}</Card.Header>
                          <Card.Meta>
                            <span>{variant.name_romanized}</span>
                          </Card.Meta>
                        </Card.Content>
                      </Card>
                    </Link>}
                  position="bottom center"
                  wide="very"
                  style={{ opacity: 0.9, boxShadow: 'none' }}>
                  <List relaxed>
                    {Object.entries(doujinshi.metas).map(([meta, values]) => {
                      if (!values || values.length === 0)
                        return null;

                      return (
                        <List.Item>
                          <List.Header>{fixMetaType(meta)}</List.Header>
                          <ul className="label-list">
                            {values.map((value, i) =>
                              <li>
                                {value}
                                {i === values.length - 1 ? null : ","}
                              </li>)}
                          </ul>
                        </List.Item>
                      );
                    })}
                  </List>
                </Popup>
              </Grid.Column>
            );
          })}
        </Grid>
      </div>
    );
  }
}
