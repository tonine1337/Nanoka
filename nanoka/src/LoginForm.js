import React from 'react';
import { Grid, Header, Form, Segment, Button, Message, Icon } from 'semantic-ui-react';
import './LoginForm.css';
import * as api from './Api';

export class LoginForm extends React.Component {
  state = {
    secret: '',
    error: null,
    fetching: false
  };

  render() {
    return (
      <div>
        <Grid textAlign="center" style={{ height: '100vh' }} verticalAlign="middle">
          <Grid.Column style={{ maxWidth: '26rem' }}>
            <Header style={{ fontSize: '4rem', marginBottom: '2rem' }} color="blue" textAlign="center">Nanoka</Header>
            <Form>
              <Segment textAlign="left">
                <Form.Field>
                  <label>User secret</label>
                  <Form.Input
                    fluid
                    placeholder="Secret"
                    value={this.state.secret}
                    onChange={e => {
                      if (this.state.fetching)
                        return;

                      this.setState({ secret: e.target.value });
                    }} />
                </Form.Field>

                <Button fluid icon labelPosition="left" loading={this.state.fetching} onClick={async () => {
                  if (this.state.fetching)
                    return;

                  if (!this.state.secret) {
                    this.setState({
                      error: null
                    });
                    return;
                  }

                  // input is basically two UUID's joined
                  if (this.state.secret.length !== 64) {
                    this.setState({
                      error: 'Invalid secret format.'
                    });
                    return;
                  }

                  this.setState({
                    fetching: true
                  });

                  const id = this.state.secret.substr(0, 32);
                  const secret = this.state.secret.substr(32, 32);

                  try {
                    await api.authenticateAsync(id, secret);

                    api.setCredentials(id, secret);
                  }
                  catch (error) {
                    this.setState({
                      fetching: false,
                      error: error
                    });
                  }
                }}>
                  <Icon name="sign-in" />Sign in
                </Button>
              </Segment>
            </Form>
            {this.state.error
              ? <Message error>
                {this.state.error}
              </Message>
              : <Message info>
                <span>New to Nanoka? <a href='/join'>Create an account.</a></span>
              </Message>}
          </Grid.Column>
        </Grid>

        <div id="links" style={{ position: 'absolute', left: 0, bottom: '1rem', right: 0, textAlign: 'center' }}>
          <a href="/about">About</a>
          <a href="https://github.com/chiyadev/Nanoka" target="_blank" rel="noopener noreferrer">Open source</a>
          <a href="/recover_secret">Recover secret</a>
        </div>
      </div>
    );
  }
}

export default LoginForm;
