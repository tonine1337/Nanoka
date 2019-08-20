import React from 'react';
import { BrowserRouter, Link } from 'react-router-dom';
import { Grid, Label, Icon, Popup, Table, Dropdown, Button } from 'semantic-ui-react';
import moment from 'moment';
import * as api from '../Api';
import { DoujinshiImage } from './DoujinshiImage';
import './ViewInfo.css';

export class ViewInfo extends React.Component {
  state = {
    doujinshi: null,
    currentVariant: null,
    error: null
  };

  async componentDidMount() {
    try {
      const doujinshi = await api.getDoujinshiAsync(this.props.id);

      // select the first variant by default
      //todo: smart selection based on device language
      const currentVariant = doujinshi.variants[0].id;

      this.setState({ doujinshi, currentVariant });
    }
    catch (error) {
      this.setState({ error });
    }
  }

  getCategoryIcon(category) {
    switch (category) {
      case 0:
      case 1: return <Icon name="book" />;
      case 2:
      case 3: return <Icon name="tint" />;
      case 4: return <Icon name="images" />;

      default: return null;
    }
  }

  getCategoryName(category) {
    switch (category) {
      case 0: return 'Doujinshi';
      case 1: return 'Manga';
      case 2: return 'Artist CG';
      case 3: return 'Game CG';
      case 4: return 'Image Set';

      default: return null;
    }
  }

  getLanguageName(language) {
    switch (language) {
      case 0: return 'Japanese';
      case 1: return 'English';

      default: return null;
    }
  }

  fixMetaType(meta) {
    switch (meta.toLowerCase()) {
      case 'artist': return 'Artist';
      case 'group': return 'Group';
      case 'parody': return 'Parody';
      case 'character': return 'Characters';
      case 'tag': return 'Tags';
      case 'convention': return 'Convention';

      default: return meta;
    }
  }

  render() {
    if (this.state.error) {
      return (
        <span>{this.state.error}</span>
      );
    }

    const doujinshi = this.state.doujinshi;

    if (!doujinshi)
      return null;

    const variant = doujinshi.variants.find(v => v.id === this.state.currentVariant);

    const uploadTime = moment(doujinshi.upload);
    const updateTime = moment(doujinshi.update);

    return (
      <BrowserRouter>
        <Grid stackable divided="vertically">
          <Grid.Row>
            <Grid.Column width="6" textAlign="center" verticalAlign="middle">
              <Link to={`/doujinshi/${doujinshi.id}/${variant.id}/0`}>
                <DoujinshiImage doujinshi={doujinshi} variant={variant} index={0} style={{
                  width: '100%',
                  borderRadius: '1rem'
                }} />
              </Link>
            </Grid.Column>
            <Grid.Column width="10">
              <h4 style={{ marginBottom: 0, opacity: 0.6 }}>{variant.name_romanized}</h4>
              <h1 style={{ marginTop: 0, fontSize: '2.5rem' }}>{variant.name}</h1>

              <ul className="label-list">
                <li>
                  <Label as="a" color="black">
                    {this.getCategoryIcon(doujinshi.category)}
                    {this.getCategoryName(doujinshi.category)}
                  </Label>
                </li>
                <li>
                  <Label basic as="a">
                    <Icon name="globe" />
                    <Dropdown
                      inline
                      value={variant.id}
                      options={doujinshi.variants.map(v => ({
                        text: this.getLanguageName(v.language),
                        value: v.id
                      }))}
                      onChange={(_, { value }) => this.setState({ currentVariant: value })} />
                    <Label.Detail>{doujinshi.variants.length}</Label.Detail>
                  </Label>
                </li>
                <li>
                  {variant.source
                    ? <Popup
                      trigger={<Label basic as="a"><Icon name="linkify" />Source: <em>{new URL(variant.source).hostname}</em></Label>}
                      content={<a href={variant.source} target="_blank" rel="noopener noreferrer">{variant.source}</a>}
                      position="bottom center"
                      wide="very"
                      pinned
                      on="click"
                    />
                    : <span />}
                </li>
              </ul>
              <br />

              <Table basic="very">
                <Table.Body>
                  {Object.entries(doujinshi.metas).map(([meta, values]) => {
                    if (!values || values.length === 0)
                      return null;

                    return (
                      <Table.Row>
                        <Table.Cell collapsing>
                          <strong>{this.fixMetaType(meta)}</strong>
                        </Table.Cell>
                        <Table.Cell>
                          <ul className="label-list">
                            {values.map(value =>
                              <li>
                                <Label basic as="a">{value}</Label>
                              </li>)}
                          </ul>
                        </Table.Cell>
                      </Table.Row>
                    );
                  })}
                </Table.Body>
              </Table>

              <div style={{ opacity: 0.8 }}>
                <span>{variant.pages} pages</span>
                <br />
                <br />
                <span>
                  Uploaded <Popup
                    wide
                    inverted
                    trigger={<span>{uploadTime.calendar()}</span>}
                    content={uploadTime.format("LLLL")} /> by <code>userId</code>
                </span>
                <br />
                <span>
                  Last edited <Popup
                    wide
                    inverted
                    trigger={<strong>{updateTime.calendar()}</strong>}
                    content={updateTime.format("LLLL")} /> by <code>userId</code>
                </span>
              </div>
              <br />

              <ul className="label-list">
                <li>
                  <Button as="div" labelPosition="right">
                    <Button color="red">
                      <Icon name="heart" />Like</Button>
                    <Label as="a" basic color="red" pointing="left">{doujinshi.score}</Label>
                  </Button>
                </li>
                <li>
                  <Button icon labelPosition="left" color="green" basic>
                    <Icon name="plus" />Add
                  </Button>
                </li>
                <li>
                  <Button icon labelPosition="left" color="blue" basic>
                    <Icon name="download" />Download
                  </Button>
                </li>
                <li>
                  <Dropdown floating inline icon="ellipsis horizontal">
                    <Dropdown.Menu>
                      <Dropdown.Item>
                        <Icon name="share" />Share doujinshi
                      </Dropdown.Item>
                      <Dropdown.Item>
                        <Icon name="history" />View history
                      </Dropdown.Item>
                      <Dropdown.Item>
                        <Icon name="edit" />Edit doujinshi
                      </Dropdown.Item>
                      <Dropdown.Item>
                        <Icon name="exchange" />Move variant
                      </Dropdown.Item>
                      <Dropdown.Item>
                        <Icon name="object group" />Merge into another doujinshi
                      </Dropdown.Item>
                      <Dropdown.Item>
                        <Icon name="object ungroup" />Separate variant
                      </Dropdown.Item>
                      <Dropdown.Item>
                        <Icon name="trash alternate" />Delete variant
                      </Dropdown.Item>
                      <Dropdown.Item>
                        <Icon name="flag" />Report
                      </Dropdown.Item>
                    </Dropdown.Menu>
                  </Dropdown>
                </li>
              </ul>
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Grid.Column>
              <Grid doubling columns={6}>
                {[...Array(variant.pages).keys()].map(i => {
                  return (
                    <Grid.Column className="thumb" textAlign="center" verticalAlign="middle">
                      <Link to={`/doujinshi/${doujinshi.id}/${variant.id}/${i}`}>
                        <DoujinshiImage doujinshi={doujinshi} variant={variant} index={i} style={{
                          borderRadius: '0.5rem'
                        }} />
                      </Link>
                    </Grid.Column>
                  );
                })}
              </Grid>
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Grid.Column>
              <span>Doujinshi <code>{doujinshi.id}</code></span>
              <br />
              <span>Variant <code>{variant.id}</code></span>
            </Grid.Column>
          </Grid.Row>
        </Grid>
      </BrowserRouter>
    );
  }
}
