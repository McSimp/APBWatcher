import React, { Component, PropTypes } from 'react';
import { Table } from 'semantic-ui-react';
import ThreatIndicator from './ThreatIndicator.js'

const types = {
  '4008467211': {
    ruleSet: 'Missions',
    area: 'Financial',
    language: 'EN',
  },
  '3616844488': {
    ruleSet: 'Fight Club',
    area: 'Abington Towers',
    language: 'EN',
  },
  '2858091178': {
    ruleSet: 'Dynamic Event',
    area: 'Waterfront - Anarchy',
    language: 'EN',
  },
  '684354270': {
    ruleSet: 'Missions',
    area: 'Waterfront',
    language: 'EN',
  },
  '444197787': {
    ruleSet: 'Social',
    area: 'Breakwater Marina',
    language: 'EN',
  },
  '1094540976': {
    ruleSet: 'Fight Club',
    area: 'Baylan Shipping Storage',
    language: 'EN',
  },
  '3695193608': {
    ruleSet: 'Open Conflict',
    area: 'Waterfront',
    language: 'EN',
  },
  '3675975819': {
    ruleSet: 'Open Conflict',
    area: 'Financial',
    language: 'EN',
  }
};

class InstanceRow extends Component {
  getFullName() {
    const sdd = this.props.instance.district_instance_type_sdd;
    if (sdd in types) {
      return types[sdd].ruleSet + '-' + types[sdd].area + '-' + types[sdd].language + '-' + this.props.instance.instance_num;
    } else {
      return 'Unknown';
    }
  }

  getRuleSet() {
    const sdd = this.props.instance.district_instance_type_sdd;
    if (sdd in types) {
      return types[sdd].ruleSet;
    } else {
      return 'Unknown';
    }
  }

  render() {
    return (
      <Table.Row>
        <Table.Cell textAlign='center'><ThreatIndicator threat={this.props.instance.threat} /></Table.Cell>
        <Table.Cell>{ this.getFullName() }</Table.Cell>
        <Table.Cell>{ this.getRuleSet() }</Table.Cell>
        <Table.Cell>{ (this.props.instance.enforcers + this.props.instance.criminals).toString() }</Table.Cell>
        <Table.Cell>{ this.props.instance.enforcers.toString() }</Table.Cell> 
        <Table.Cell>{ this.props.instance.criminals.toString() }</Table.Cell>
      </Table.Row>
    );
  }
}

InstanceRow.propTypes = {
  instance: PropTypes.shape({
    criminals: PropTypes.number.isRequired,
    enforcers: PropTypes.number.isRequired,
    threat: PropTypes.string.isRequired,
    district_instance_type_sdd: PropTypes.string.isRequired,
    instance_num: PropTypes.string.isRequired,
  }).isRequired
};

export default InstanceRow;