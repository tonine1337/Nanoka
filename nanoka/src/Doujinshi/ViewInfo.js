import React from 'react';
import { Grid, Label, Icon } from 'semantic-ui-react';
import * as api from '../Api';
import { DoujinshiImage } from './DoujinshiImage';

export class ViewInfo extends React.Component {
  state = {
    doujinshi: null,
    currentVariant: null,
    error: null,
    cover: null
  };

  async componentDidMount() {
    try {
      const doujinshi = await api.getDoujinshiAsync(this.props.id);

      // select the first variant by default
      //todo: smart selection based on device language
      const currentVariant = doujinshi.variants[0];

      this.setState({ doujinshi, currentVariant });

      // load cover image after everything else
      this.setState({
        cover: await api.downloadImageAsync(doujinshi.id, doujinshi.variants[0].id, 0)
      });
    }
    catch (error) {
      this.setState({ error });
    }
  }

  getCategoryIcon(doujinshi) {
    switch (doujinshi.category) {
      case 0:
      case 1: return <Icon name="book" />;
      case 2:
      case 3: return <Icon name="tint" />;
      case 4: return <Icon name="images" />;

      default: return null;
    }
  }

  getCategoryName(doujinshi) {
    switch (doujinshi.category) {
      case 0: return 'Doujinshi';
      case 1: return 'Manga';
      case 2: return 'Artist CG';
      case 3: return 'Game CG';
      case 4: return 'Image Set';

      default: return null;
    }
  }

  render() {
    if (this.state.error) {
      return (
        <span>{this.state.error}</span>
      );
    }

    const doujinshi = this.state.doujinshi;
    const variant = this.state.currentVariant;

    if (!doujinshi || !variant)
      return null;

    return (
      <div>
        <Grid stackable divided='vertically'>
          <Grid.Row columns={3}>
            <Grid.Column>
              <DoujinshiImage
                doujinshi={this.state.doujinshi}
                variant={this.state.doujinshi.variants[0]}
                index={0}
                style={{
                  width: '100%',
                  borderRadius: '1rem'
                }} />
            </Grid.Column>
            <Grid.Column>
              <h1>{doujinshi.name_original}</h1>
              <h4 style={{ marginTop: 0 }}>{doujinshi.name_english}</h4>
              {Object.entries(this.state.metaOptions).map(([meta, values]) => {
                const items = Object.entries(values).map(([value, variantIds]) => {
                  return <span>{value}</span>
                });

                return <p>{meta}: {items}</p>
              })}
              <Label as="a">
                {this.getCategoryIcon(doujinshi)}
                {this.getCategoryName(doujinshi)}
              </Label>
            </Grid.Column>
          </Grid.Row>
        </Grid>
      </div>
    );
  }
}
