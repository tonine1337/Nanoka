import React from 'react';
import { BrowserRouter as Router, Route, Link } from "react-router-dom";
import Index from './Index/Index';
import './App.css';

function App() {
  return (
    <Router>
      <div>
        <nav>
          <ul>
            <li>
              <Link to="/" className="fa fa-home"></Link>
            </li>
            <li>
              <Link to="/about/">About</Link>
            </li>
            <li>
              <Link to="/users/">Users</Link>
            </li>
          </ul>
        </nav>

        <main>
          <Route path="/" exact component={Index} />
        </main>
      </div>
    </Router>
  );
}

export default App;
