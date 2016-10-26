import { Table, Message, Icon } from 'semantic-ui-react';
import React, { Component, PropTypes } from 'react';
import InstanceRow from '../components/InstanceRow.js';
import SortedTableHeader from '../components/SortedTableHeader.js';
import { connect } from 'react-redux';
import { requestInstanceRefresh } from '../actions';

const columns = {
  'threat': { name: 'Threat',  width: 1 },
  'fullName': { name: 'Name', width: 6 },
  'ruleSet': { name: 'Rule Set', width: 3 },
  'population': { name: 'Population', width: 2 },
  'enforcers': { name: 'Enforcers', width: 2 },
  'criminals': { name: 'Criminals', width: 2 }
};

const numericCols = ['population', 'enforcers', 'criminals', 'threat'];

const compare = (a,b,key,desc) => {
  if (a[key] < b[key]) {
    return desc ? 1 : -1;
  }

  if (a[key] > b[key]) {
    return desc ? -1 : 1;
  }

  return 0;
};

const filterInstances = (instances, world) => {
  return instances.filter(instance => (instance.worldName === world));
};

class InstanceTable extends Component {
  static propTypes = {
    instances: PropTypes.array.isRequired,
    isFetchingInstances: PropTypes.bool.isRequired,
    requestInstanceRefresh: PropTypes.func.isRequired
  }

  constructor(props) {
    super(props);
    this.state = {
      sortKey: 'population',
      sortDesc: true
    };
  }

  componentDidMount() {
    this.props.requestInstanceRefresh();
  }

  sortInstances() {
    return this.props.instances.sort((a,b) => {
      const key = this.state.sortKey;
      const desc = this.state.sortDesc;

      let res = compare(a, b, key, desc);
      if (res !== 0) {
        return res;
      }

      // Things are equal, sort by next columns
      // Sort numeric columns in descending order if we are sorting by a numeric column in descending order, or if we're not sorting by a numeric column.
      const numericDesc = (numericCols.indexOf(key) !== -1) ? desc : true;

      // Foreach numeric column, if it's not the sortKey, compare
      for (const col of numericCols) {
        if (col !== key) {
          res = compare(a, b, col, numericDesc);
          if (res !== 0) {
            return res;
          }
        }
      }

      // Finally sort by name ascending
      if (key !== 'fullName') {
        return compare(a, b, 'fullName', false);
      }

      return 0;
    });
  }

  headerClicked(key) {
    this.setState({
      sortKey: key,
      sortDesc: (key === this.state.sortKey) ? !this.state.sortDesc : false
    });
  }

  render() {
    if (this.props.instances.length === 0) {
      if (!this.props.isFetchingInstances) {
        return (
          <Message icon warning>
            <Icon name='warning circle' />
            <Message.Content>
              <Message.Header>No instances found</Message.Header>
              This could mean that the APB servers are offline, or there was a problem retrieving player data from this server.
            </Message.Content>
          </Message>
        );
      } else {
        return (
          <Message info>
            <Icon name='circle notched' loading /> Loading instances...
          </Message>
        );
      }
    }

    return (
      <Table celled className='sortable unstackable'>
        <Table.Header>
          <Table.Row>
            { Object.keys(columns).map(key => {
              return <SortedTableHeader 
                key={ key }
                id={ key }
                sortKey={ this.state.sortKey }
                sortDesc={ this.state.sortDesc }
                onClick={ () => this.headerClicked(key) }
                width={ columns[key].width }
              >
                { columns[key].name }
              </SortedTableHeader>
            }) }
          </Table.Row>
        </Table.Header>
        <Table.Body>
          { this.sortInstances().map((instance, i) => {
            return (<InstanceRow instance={instance} key={i} />);
          }) }
        </Table.Body>
      </Table>
    );
  }
}

const mapStateToProps = (state) => ({
  instances: filterInstances(state.instances, state.activeWorld),
  isFetchingInstances: state.isFetchingInstances
});

export default connect(mapStateToProps, {
  requestInstanceRefresh
})(InstanceTable);
