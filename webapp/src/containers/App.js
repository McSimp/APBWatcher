import React, { Component } from 'react';
import { Container, Header, Icon } from '../semantic-ui';
import './App.css';
import InstanceTable from './InstanceTable.js';
import RefreshBar from './RefreshBar.js';
import WorldMenu from './WorldMenu.js';

class App extends Component {
  render() {
    return (
      <Container>
        <Header as='h1'>
          <Icon name='heartbeat' />
          <Header.Content>APB Watcher</Header.Content>
        </Header>
        <WorldMenu />
        <InstanceTable />
        <RefreshBar />
      </Container>
    );
  }
}

export default App;
