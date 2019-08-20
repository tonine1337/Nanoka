import React from 'react';
import { Form, Button, Dropdown, Icon, Message, Progress } from 'semantic-ui-react';
import { createDropzone } from '../DropzoneStyle';
import * as api from '../Api';
import * as JSZip from 'jszip';

export class Uploader extends React.Component {
  state = {
    file: null,
    isSubmitting: false,
    uploadProgress: 0,
    errors: []
  };

  render() {
    const getTextProps = (name, type) => ({
      type: type || 'text',
      onChange: e => this.setState({ [name]: e.target.value }),
      value: this.state[name] || ''
    });

    const getSelectProps = (name, options) => ({
      selection: true,
      onChange: (_, { value }) => this.setState({ [name]: value }),
      options: options.map(([text, value]) => ({ text, value })),
      value: this.state[name] || 0
    });

    const getTagInputProps = name => ({
      search: true,
      selection: true,
      multiple: true,
      allowAdditions: true,
      noResultsMessage: null,
      onChange: (_, { value }) => this.setState({ [name]: value }),
      onAddItem: (_, { value }) => this.setState(p => {
        if (p[name + 'options'])
          return { [name + 'options']: [{ text: value, value }, ...p[name + 'options']] };
        return { [name + 'options']: [{ text: value, value }] };
      }),
      options: this.state[name + 'options'] || [],
      value: this.state[name] || []
    });
    console.log(this.state)
    return (
      <div>
        <h1>Upload a Doujinshi</h1>
        <Form>
          <Form.Group grouped className="two">
            <Form.Field required>
              <label>Name</label>
              <input placeholder="Doujinshi name" {...getTextProps('name')} />
              <small>
                Name of this doujinshi in the written language.
              </small>
            </Form.Field>
            <Form.Field>
              <label>Romanized Name</label>
              <input placeholder="Romanized name" {...getTextProps('name_romanized')} />
              <small>
                Romanized version of the original name, if applicable.
                <a href="https://en.wikipedia.org/wiki/Hepburn_romanization" rel="noopener noreferrer" target="_blank"> Hepburn romanization</a> is preferred for Japanese.
              </small>
            </Form.Field>
            <Form.Field>
              <label>Source</label>
              <input placeholder="URL" {...getTextProps('source', 'url')} />
              <small>
                URL of the web page where this doujinshi is being transferred from.
              </small>
            </Form.Field>
          </Form.Group>

          <Form.Group grouped className="four wide">
            <Form.Field>
              <label>Language</label>
              <Dropdown {...getSelectProps('language', [
                ['Japanese', 0],
                ['English', 1]
              ])} />
              <small>
                Language in which this doujinshi is written.
              </small>
            </Form.Field>
            <Form.Field>
              <label>Category</label>
              <Dropdown {...getSelectProps('category', [
                ['Doujinshi', 0],
                ['Manga', 1],
                ['Artist CG', 2],
                ['Game CG', 3],
                ['Image Set', 4]
              ])} />
              <small>
                Special category that this doujinshi belongs to.
              </small>
            </Form.Field>
          </Form.Group>

          <Form.Group className="three">
            <Form.Field required>
              <label>Artist</label>
              <Dropdown placeholder="Artist" {...getTagInputProps('artist')} />
              <small>
                Artist of this doujinshi.
              </small>
            </Form.Field>
            <Form.Field>
              <label>Group</label>
              <Dropdown placeholder="Group" {...getTagInputProps('group')} />
              <small>
                Doujin circle that published this doujinshi.
              </small>
            </Form.Field>
            <Form.Field>
              <label>Convention</label>
              <Dropdown placeholder="Convention" {...getTagInputProps('convention')} />
              <small>
                Convention where this doujinshi was available.
              </small>
            </Form.Field>
          </Form.Group>

          <Form.Group className="three">
            <Form.Field>
              <label>Parody</label>
              <Dropdown placeholder="Parody" {...getTagInputProps('parody')} />
              <small>
                Name of the anime series, manga or other copyrighted work that this doujinshi is the parody of.
              </small>
            </Form.Field>
            <Form.Field required>
              <label>Characters</label>
              <Dropdown placeholder="Character" {...getTagInputProps('character')} />
              <small>
                Characters that appear in this doujinshi.
              </small>
            </Form.Field>
            <Form.Field required>
              <label>Tags</label>
              <Dropdown placeholder="Tag" {...getTagInputProps('tag')} />
              <small>
                Tags that describe this doujinshi appropriately.
              </small>
            </Form.Field>
          </Form.Group>

          <Form.Field required>
            <label>File</label>
            {createDropzone.call(this)}
          </Form.Field>

          <Button type="submit" className="labeled icon primary" loading={this.state.isSubmitting} onClick={() => this.handleSubmit()}>
            <Icon name="upload" disabled={this.state.isSubmitting} />Upload
          </Button>
        </Form>

        <Message negative hidden={this.state.errors.length === 0}>
          <Message.Header content="There were some errors with your submission." />
          <Message.List items={this.state.errors} />
        </Message>
        <Message success hidden={!this.state.isSubmitting}>
          <Message.Header content="Your submission is being uploaded." />
          <Message.Content content="This may take a while depending on your internet speed." />
        </Message>

        {this.state.uploadProgress !== 0 ?
          <Progress progress indicating
            percent={this.state.uploadProgress * 100}
            active={this.state.uploadProgress !== 1} /> : <span />}
      </div>
    );
  }

  async handleSubmit() {
    const state = this.state;

    if (state.isSubmitting)
      return;

    // validate input first
    const errors = [];

    if (!state.name)
      errors.push('Name is not specified.');

    if (!state.artist || state.artist.length === 0)
      errors.push('Artist is not specified.');

    if (!state.character || state.character.length === 0)
      errors.push('Doujinshi must have at least one character.');

    if (!state.tag || state.tag.length === 0)
      errors.push('Doujinshi must have at least one tag.');

    if (!state.file)
      errors.push('Please select a file to upload.');

    if (errors.length !== 0) {
      this.setState({ isSubmitting: false, errors });
      return;
    }

    this.setState({ isSubmitting: true, errors, uploadProgress: 0 });

    try {
      const zip = await JSZip.loadAsync(this.state.file);
      const fileCount = Object.keys(zip.files).length;

      let upload = await api.createDoujinshiAsync(
        {
          category: state.category,
          metas: {
            artist: state.artist,
            group: state.group,
            parody: state.parody,
            convention: state.convention,
            character: state.character,
            tag: state.tag
          }
        },
        {
          name: state.name,
          name_romanized: state.name_romanized,
          language: state.language,
          source: state.source
        }
      );

      let i = 0;

      for (const entry in zip.files) {
        const file = zip.files[entry];
        const blob = await file.async('blob');

        upload = await api.uploadFileAsync(upload.id, blob, ++i === fileCount);

        this.setState({
          uploadProgress: (i / fileCount).toFixed(2)
        });
      }

      this.props.history.push(`/doujinshi/${upload.id}`);
    }
    catch (error) {
      this.setState({
        isSubmitting: false,
        errors: [error]
      });
    }
  }
}
