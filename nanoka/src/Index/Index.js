import React from 'react';
import { Link } from "react-router-dom";
import './Index.css';
import * as api from '../Api';

function Index() {
  return (
    <div>
      <h1 style={{
        fontSize: "4rem",
        marginBottom: "0.2rem"
      }} className="ui blue header center aligned">Nanoka</h1>

      <div className="links" style={{
        textAlign: "center"
      }}>
        <Link to="/doujinshi/all/upload">Doujinshi</Link>
        <Link to="/booru/all/upload">Booru</Link>
        <Link to="/activity/comment">Comments</Link>
        <a href="#" className="ui simple dropdown">
          Forum
          <div className="menu">
            <a href="https://reddit.com" target="_blank" rel="noopener noreferrer" className="item"><i className="icon reddit"></i> Reddit</a>
            <a href="https://discord.gg/Edx7Rbc" target="_blank" rel="noopener noreferrer" className="item"><i className="icon discord"></i> Discord</a>
          </div>
        </a>
        <Link to="/wiki">Wiki</Link>
        <Link to="/account">Account</Link>
      </div>
      <br />

      <div className="info" style={{
        textAlign: "center"
      }}>
        <small>Signed in as {api.getCurrentUser().name}</small>
        <small>Running Nanoka v{api.version}</small>
        <br />
        <small><code>{api.getCurrentUser().id}</code></small>
      </div>
      <br />
      <br />

      <div style={{
        display: "flex",
        justifyContent: "center"
      }}>
        <img alt="counter" src="assets/counters/1.png" />
        <img alt="counter" src="assets/counters/2.png" />
        <img alt="counter" src="assets/counters/3.png" />
        <img alt="counter" src="assets/counters/4.png" />
        <img alt="counter" src="assets/counters/5.png" />
        <img alt="counter" src="assets/counters/6.png" />
        <img alt="counter" src="assets/counters/7.png" />
      </div>
    </div>
  );
}

export default Index;
