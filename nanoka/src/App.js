import React from 'react';
import { BrowserRouter as Router, Route, NavLink } from "react-router-dom";
import Index from './Index/Index';
import { Button, Icon, Menu, Header, Divider, Container } from 'semantic-ui-react';
import * as api from './Api';
import SearchBar from './SearchBar';

export default class App extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      fetched: false,
      client: null,
      error: null
    };
  }

  componentDidMount() {
    api.getClientInfo()
      .then(r => {
        this.setState({
          client: r.json(),
          fetched: true
        });
      })
      .catch(err => {
        this.setState({
          error: err,
          fetched: true
        })
      });
  }

  render() {
    if (!this.state.fetched)
      return null;

    if (this.state.error) {
      return (
        <Container as="main">
          <h1 style={{ paddingTop: "3rem" }}>Nanoka</h1>
          <span>Could not connect to Nanoka client :(</span>
          <br />
          <br />

          <Button as="a" href="https://github.com/chiyadev/Nanoka/releases" className="labeled icon primary"><Icon name="download" /> Install</Button>
          <Button icon labelPosition='left' onClick={() => window.location.reload()}><Icon name='redo' /> Retry</Button>
          <br />

          <a href="https://github.com/chiyadev/Nanoka"><small>What is this?</small></a>
        </Container>
      );
    }

    return (
      <Router>
        <Menu>
          <Menu.Item as="a" href="/" className="header">Nanoka</Menu.Item>
          <Menu.Item className="ui simple dropdown">
            <Icon name="book" /> Doujinshi
            <Menu style={{ minWidth: "15em" }}>
              <Header>Doujinshi</Header>
              <NavLink to="/doujinshi/all/name" className="item"><Icon name="sort alphabet down" /> Name</NavLink>
              <NavLink to="/doujinshi/all/upload" className="item"><Icon name="clock" /> Newest</NavLink>
              <NavLink to="/doujinshi/all/trending" className="item"><Icon name="chart line" /> Trending</NavLink>
              <NavLink to="/doujinshi/all/viewed" className="item"><Icon name="eye" /> Most Viewed</NavLink>
              <NavLink to="/doujinshi/random" className="item"><Icon name="random" /> Random</NavLink>
              <Divider />
              <Header>Meta</Header>
              <NavLink to="/doujinshi/meta/artist" className="item"><Icon name="paint brush" /> Artist</NavLink>
              <NavLink to="/doujinshi/meta/group" className="item"><Icon name="users" /> Group</NavLink>
              <NavLink to="/doujinshi/meta/parody" className="item"><Icon name="linkify" /> Parody</NavLink>
              <NavLink to="/doujinshi/meta/character" className="item"><Icon name="odnoklassniki" /> Character</NavLink>
              <NavLink to="/doujinshi/meta/category" className="item"><Icon name="th large" /> Category</NavLink>
              <NavLink to="/doujinshi/meta/language" className="item"><Icon name="globe" /> Language</NavLink>
              <NavLink to="/doujinshi/meta/tag" className="item"><Icon name="tag" /> Tag</NavLink>
              <NavLink to="/doujinshi/meta/convention" className="item"><Icon name="calendar" /> Convention</NavLink>
            </Menu>
          </Menu.Item>
          <Menu.Item className="ui simple dropdown">
            <Icon name="image" /> Booru
            <Menu style={{ minWidth: "15em" }}>
              <Header>Booru</Header>
              <NavLink to="/booru/all/upload" className="item"><Icon name="clock" /> Newest</NavLink>
              <NavLink to="/booru/all/trending" className="item"><Icon name="chart line" /> Trending</NavLink>
              <NavLink to="/booru/all/viewed" className="item"><Icon name="eye" /> Most Viewed</NavLink>
              <NavLink to="/booru/random" className="item"><Icon name="random" /> Random</NavLink>
              <Divider />
              <Header>Tag</Header>
              <NavLink to="/booru/tag/general" className="item"><Icon name="tag" /> General</NavLink>
              <NavLink to="/booru/tag/artist" className="item"><Icon name="paint brush" /> Artist</NavLink>
              <NavLink to="/booru/tag/character" className="item"><Icon name="odnoklassniki" /> Character</NavLink>
              <NavLink to="/booru/tag/copyright" className="item"><Icon name="copyright" /> Copyright</NavLink>
              <NavLink to="/booru/tag/metadata" className="item"><Icon name="info" /> Metadata</NavLink>
            </Menu>
          </Menu.Item>
          <Menu.Item className="ui simple dropdown">
            <Icon name="users" /> Community
            <Menu style={{ minWidth: "15em" }}>
              <Header>Activity</Header>
              <NavLink to="/activity/edit" className="item"><Icon name="edit" /> Edits</NavLink>
              <NavLink to="/activity/comment" className="item"><Icon name="comments" /> Comments</NavLink>
              <NavLink to="/activity/note" className="item"><Icon name="sticky note" /> Notes</NavLink>
              <NavLink to="/activity/moderation" className="item"><Icon name="user secret" /> Moderation</NavLink>
              <Divider />
              <Header>Wiki</Header>
              <NavLink to="/wiki" className="item"><Icon name="book" /> Wiki</NavLink>
              <Divider />
              <Header>Forum</Header>
              <Menu.Item as="a" href="https://reddit.com" target="_blank" rel="noopener noreferrer" className="item"><Icon name="reddit" /> Reddit</Menu.Item>
              <Menu.Item as="a" href="https://discord.gg/Edx7Rbc" target="_blank" rel="noopener noreferrer" className="item"><Icon name="discord" /> Discord</Menu.Item>
            </Menu>
          </Menu.Item>
          <div className="right menu">
            <Menu.Item>
              <SearchBar />
            </Menu.Item>
            <Menu.Item className="ui simple dropdown">
              <Icon name="cog" />
              <Menu style={{ minWidth: "15em", marginRight: "2px" }}>
                <Header>Nanoka</Header>
                <NavLink exact to="/account" className="item"><Icon name="user" /> Account</NavLink>
                <NavLink to="/account/dashboard" className="item"><Icon name="dashboard" /> Dashboard</NavLink>
                <NavLink to="/account/uploads" className="item"><Icon name="upload" /> My uploads</NavLink>
                <NavLink to="/settings/language" className="item"><Icon name="globe" /> Language</NavLink>
                <NavLink to="/settings/theme" className="item"><Icon name="theme" /> Theme</NavLink>
                <Divider />
                <Header>Misc.</Header>
                <Menu.Item as="a" href="https://github.com/chiyadev/Nanoka" target="_blank" rel="noopener noreferrer" className="item"><Icon name="github" /> Open Source</Menu.Item>
                <Menu.Item href="https://github.com/chiyadev/Nanoka" target="_blank" rel="noopener noreferrer" className="item"><Icon name="heart" /> Support Nanoka</Menu.Item>
                <Divider />
                <Header>Management</Header>
                <NavLink to="/management" className="item"><Icon name="database" /> Database</NavLink>
                <Menu.Item href="/client" className="item"><Icon name="terminal" /> Open client</Menu.Item>
                <Menu.Item href="/signout" className="item"><Icon name="sign-out" /> Sign out</Menu.Item>
              </Menu>
            </Menu.Item>
          </div>
        </Menu>

        <Container as="main" style={{ marginTop: "3rem", marginBottom: "3rem" }}>
          <Route path="/" exact component={Index} />
        </Container>
      </Router>
    );
  }
}
