import React from 'react';
import { Form, Button, Dropdown } from 'semantic-ui-react';

export class Uploader extends React.Component {
  render() {
    return (
      <div>
        <h1>Upload a Doujinshi</h1>
        <Form>
          <Form.Group className="three">
            <Form.Field required>
              <label>Original Name</label>
              <input type="text" placeholder="Original Name" />
              <small>
                Name of this doujinshi in its source language.
                This would usually be Japanese.
              </small>
            </Form.Field>
            <Form.Field>
              <label>Romanized Name</label>
              <input type="text" placeholder="Romanized Name" />
              <small>
                Romanized version of the original name if applicable.
                <a href="https://en.wikipedia.org/wiki/Hepburn_romanization" rel="noopener noreferrer" target="_blank"> Hepburn romanization</a> is preferred.
              </small>
            </Form.Field>
            <Form.Field>
              <label>English Name</label>
              <input type="text" placeholder="English Name" />
              <small>
                English translation of the original name if applicable.
              </small>
            </Form.Field>
          </Form.Group>
          <Form.Group grouped className="two">
            <Form.Field required>
              <label>Artist</label>
              <Dropdown search selection multiple allowAdditions placeholder="Artist" />
              <small>
                Artist of this doujinshi.
              </small>
            </Form.Field>
            <Form.Field>
              <label>Group</label>
              <Dropdown search selection multiple allowAdditions placeholder="Group" />
              <small>
                Doujin circle that published this doujinshi.
              </small>
            </Form.Field>
            <Form.Field>
              <label>Parody</label>
              <Dropdown search selection multiple allowAdditions placeholder="Parody" />
              <small>
                Name of the anime series, manga or other copyrighted work that this doujinshi is the parody of.
              </small>
            </Form.Field>
            <Form.Field required>
              <label>Characters</label>
              <Dropdown search selection multiple allowAdditions placeholder="Character" />
              <small>
                Characters that appear in this doujinshi.
              </small>
            </Form.Field>
            <Form.Field required>
              <label>Language</label>
              <Dropdown search selection multiple allowAdditions placeholder="Language" />
              <small>
                Language in which this doujinshi is written, or the target language if translated.
              </small>
            </Form.Field>
            <Form.Field required>
              <label>Tags</label>
              <Dropdown search selection multiple allowAdditions placeholder="Tag" />
              <small>
                Tags that describe this doujinshi appropriately.
              </small>
            </Form.Field>
            <Form.Field>
              <label>Convention</label>
              <Dropdown search selection multiple allowAdditions placeholder="Convention" />
              <small>
                Convention where this doujinshi was available.
              </small>
            </Form.Field>
          </Form.Group>
          <Button type="submit">Submit</Button>
        </Form>
      </div>
    );
  }
}
