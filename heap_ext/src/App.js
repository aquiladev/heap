import React, { Component } from 'react';
import './App.css';

const chrome = window.chrome;

class App extends Component {
  constructor(props) {
    super(props)

    this.state = {
      questionId: 0,
      payload: {}
    };

    this.getQuestion();
  }

  getQuestion() {
    const t = this;
    chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
      chrome.tabs.sendMessage(tabs[0].id, { request: "question" }, function (response) {
        t.setState({
          questionId: response.question
        });
      });
    });
  }

  process() {
    console.log("process")
  }

  refresh() {
    console.log("refresh")
  }

  render() {
    return (
      <div>
        <div className="actions">
          <button onClick={this.process} className="btn-process" alt="Process"></button>
          <button onClick={this.refresh} className="btn-refresh" alt="Refresh"></button>
        </div>
        <h2>Question: {this.state.questionId}</h2>
      </div>
    );
  }
}

export default App;
