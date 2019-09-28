import React, { Component } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faSearch } from '@fortawesome/free-solid-svg-icons';
import './home.css';
import Counter from '../../components/counter/counter';

var userName = "Phosphene";
var hostName = "Nanoka v1";
var serverName = "nhitomi";

/**
 * 
 */
class Home extends Component {
    render() {
        return (
            <div className="landing ">
                <p className="heading">
                    NONAKA
                </p>

                <ul>
                    <li>Books</li>
                    <li>Images</li>
                    <li>Music</li>
                    <li>Forum</li>
                    <li>Wiki</li>
                    <li>Account</li>
                </ul>
                <div className="search">
                    <input type="text" placeholder="Search.." name="search" />
                    <button type="submit"><FontAwesomeIcon icon={faSearch} color="#FFFFFF" /></button>
                </div>
                <p className="userInfo">
                    Signed in as {userName}.
                </p>
                <p className="userInfo">
                    Running {hostName}.
                </p>
                <p className="userInfo">
                    Connected to {serverName}.
                </p>
                <Counter />
            </div>
        );
    }
}



export default Home;
