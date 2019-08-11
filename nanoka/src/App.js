import React from 'react';
import { BrowserRouter as Router, Route, Link } from "react-router-dom";
import Index from './Index/Index';

function App() {
  return (
    <Router>
      <div>
        <nav>
          <ul>
            <li>
              <Link to="/">Home</Link>
            </li>
            <li>
              <Link to="/about/">About</Link>
            </li>
            <li>
              <Link to="/users/">Users</Link>
            </li>
          </ul>
        </nav>

        <Route path="/" exact component={Index} />
      </div>
    </Router>
  );
}

export default App;
