import React, { Component } from 'react';
import './counter.css';

var randomNum;
var imagesList;
class Counter extends Component {
    constructor(props) {
        super(props);
        this.state = {
            min: 1000000000,
            max: 9999999999
        }
        this.generateNumber(this.state.min, this.state.max);
    }
    generateNumber = (min, max) => {
        randomNum = Math.floor(Math.random() * (max - min + 1) + min);
        let arr = randomNum.toString(10).split('').map(Number);
        imagesList = arr.map((imgNum, index) => {
            let imageSource = "./assets/counters/" + imgNum + ".png";
            return <li id={index}><img src={imageSource} className="counterImage"></img></li >;
        });
    }

    render() {
        return (
            <ul>
                {imagesList}
            </ul >
        )
    }
}


export default Counter;