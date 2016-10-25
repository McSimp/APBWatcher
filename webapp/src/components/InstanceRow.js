import React, { Component, PropTypes } from 'react';
import { Table } from 'semantic-ui-react';
import ThreatIndicator from './ThreatIndicator.js'

class InstanceRow extends Component {
  render() {
    const { instance } = this.props;

    return (
      <Table.Row>
        <Table.Cell textAlign='center'><ThreatIndicator threat={ instance.threat } /></Table.Cell>
        <Table.Cell>{ instance.fullName }</Table.Cell>
        <Table.Cell>{ instance.ruleSet }</Table.Cell>
        <Table.Cell>{ (instance.population).toString() }</Table.Cell>
        <Table.Cell>{ instance.enforcers.toString() }</Table.Cell> 
        <Table.Cell>{ instance.criminals.toString() }</Table.Cell>
      </Table.Row>
    );
  }
}

InstanceRow.propTypes = {
  instance: PropTypes.object.isRequired
};

export default InstanceRow;