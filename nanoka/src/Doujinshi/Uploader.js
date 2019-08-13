import React from 'react';
import { Form, Button, Dropdown, Icon } from 'semantic-ui-react';
import { createDropzone } from '../DropzoneStyle';
import * as api from '../Api';

export class Uploader extends React.Component {
  state = {
    file: null
  };

  render() {
    const getNameProps = (name, type) => ({
      type: type || 'text',
      onChange: e => this.setState({ [name]: e.target.value }),
      value: this.state[name] || ''
    });

    const getMetaDropProps = name => ({
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

    return (
      <div>
        <h1>Upload a Doujinshi</h1>
        <Form>
          <Form.Group grouped className="two">
            <Form.Field required>
              <label>Original Name</label>
              <input placeholder="Original Name" {...getNameProps('originalName')} />
              <small>
                Name of this doujinshi in its source language.
                This would usually be Japanese.
              </small>
            </Form.Field>
            <Form.Field>
              <label>Romanized Name</label>
              <input placeholder="Romanized Name" {...getNameProps('romanizedName')} />
              <small>
                Romanized version of the original name if applicable.
                <a href="https://en.wikipedia.org/wiki/Hepburn_romanization" rel="noopener noreferrer" target="_blank"> Hepburn romanization</a> is preferred for Japanese.
              </small>
            </Form.Field>
            <Form.Field>
              <label>English Name</label>
              <input placeholder="English Name" {...getNameProps('englishName')} />
              <small>
                English translation of the original name if applicable.
              </small>
            </Form.Field>
          </Form.Group>
          <Form.Field required className="four wide">
            <label>Category</label>
            <Dropdown selection defaultValue={0} options={[{
              text: 'Doujinshi',
              value: 0
            }, {
              text: 'Manga',
              value: 1
            }, {
              text: 'Artist CG',
              value: 2
            }, {
              text: 'Game CG',
              value: 3
            }, {
              text: 'Image Set',
              value: 4
            }
            ]} onChange={(_, { value }) => this.state.category = value} />
            <small>
              Special category that this doujinshi belongs to.
            </small>
          </Form.Field>
          <Form.Group className="three">
            <Form.Field required>
              <label>Artist</label>
              <Dropdown placeholder="Artist" {...getMetaDropProps('artist')} />
              <small>
                Artist of this doujinshi.
              </small>
            </Form.Field>
            <Form.Field>
              <label>Group</label>
              <Dropdown placeholder="Group" {...getMetaDropProps('group')} />
              <small>
                Doujin circle that published this doujinshi.
              </small>
            </Form.Field>
            <Form.Field required>
              <label>Language</label>
              <Dropdown placeholder="Language" {...getMetaDropProps('language')} />
              <small>
                Language in which this doujinshi is written, or the target language if translated.
              </small>
            </Form.Field>
          </Form.Group>
          <Form.Group className="three">
            <Form.Field>
              <label>Parody</label>
              <Dropdown placeholder="Parody" {...getMetaDropProps('parody')} />
              <small>
                Name of the anime series, manga or other copyrighted work that this doujinshi is the parody of.
              </small>
            </Form.Field>
            <Form.Field>
              <label>Convention</label>
              <Dropdown placeholder="Convention" {...getMetaDropProps('convention')} />
              <small>
                Convention where this doujinshi was available.
              </small>
            </Form.Field>
            <Form.Field required>
              <label>Characters</label>
              <Dropdown placeholder="Character" {...getMetaDropProps('character')} />
              <small>
                Characters that appear in this doujinshi.
              </small>
            </Form.Field>
          </Form.Group>
          <Form.Field required>
            <label>Tags</label>
            <Dropdown placeholder="Tag" {...getMetaDropProps('tag')} />
            <small>
              Tags that describe this doujinshi appropriately.
              </small>
          </Form.Field>
          <Form.Field>
            <label>Source</label>
            <input placeholder="URL" {...getNameProps('source', 'url')} />
            <small>
              URL of the web page where this doujinshi was transferred from.
              </small>
          </Form.Field>
          <Form.Field required>
            <label>File</label>
            {createDropzone.call(this)}
          </Form.Field>
          <Button type="submit" className="labeled icon primary" onClick={() => this.handleSubmit()}><Icon name="upload" /> Upload</Button>
        </Form>
      </div>
    );
  }

  handleSubmit() {
    const state = this.state;

    api.uploadDoujinshiAsync(
      {
        doujinshi: {
          name_original: state.originalName,
          name_romanized: state.romanizedName,
          name_english: state.englishname,
          category: state.category
        },
        variant: {
          metas: {
            artist: state.artist,
            group: state.group,
            language: state.language,
            parody: state.parody,
            convention: state.convention,
            character: state.character,
            tag: state.tag
          }
        },
        file: state.file
      },
      {
        success: r => {
          console.log("yay");
          console.log(r)
        }
      }
    );
  }
}
