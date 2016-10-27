import React, { Component } from 'react';
import { Container, Header, Icon } from '../semantic-ui';
import './App.css';
import InstanceTable from './InstanceTable.js';
import RefreshBar from './RefreshBar.js';
import WorldMenu from './WorldMenu.js';
import PlayerChart from './PlayerChart.js';

class App extends Component {
  render() {
    return (
      <Container>
        <Header as='h1'>
          <Icon name='heartbeat' />
          <Header.Content>APB Watcher</Header.Content>
        </Header>
        <PlayerChart />
        <WorldMenu />
        <InstanceTable />
        <RefreshBar />
      </Container>
    );
  }
}

export default App;
