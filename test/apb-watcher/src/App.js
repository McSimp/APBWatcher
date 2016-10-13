import React, { Component } from 'react';
import { Button, Container, Header, Icon, Table, Menu } from 'semantic-ui-react'
import './App.css';
import InstanceRow from './InstanceRow.js'
import TableHeader from './TableHeader.js'
import { createStore } from 'redux'
import { Provider } from 'react-redux'
import ReactDOM from 'react-dom';

const data = [
  ["2016-10-10T09:21:35.431Z",8,"684354270",0,"26",5,"5",0,"2","3002"],
  ["2016-10-10T09:21:35.325Z",0,"3675975819",0,"4",0,"1",0,"0","3002"],
  ["2016-10-10T09:21:35.205Z",1,"4008467211",0,"1",0,"4",0,"1","3002"],
  ["2016-10-10T09:21:34.731Z",28,"4008467211",0,"1",16,"3",0,"2","3002"],
  ["2016-10-10T09:21:34.624Z",3,"3695193608",0,"29",0,"1",0,"0","3002"],
  ["2016-10-10T09:21:34.515Z",1,"684354270",0,"26",0,"4",0,"1","3002"],
  ["2016-10-10T09:21:34.399Z",0,"1094540976",0,"14",0,"1",0,"0","3002"],
  ["2016-10-10T09:21:34.284Z",14,"4008467211",0,"1",16,"2",0,"3","3002"],
  ["2016-10-10T09:21:34.182Z",0,"4008467211",0,"1",0,"1",0,"4","3002"],
  ["2016-10-10T09:21:34.066Z",0,"684354270",0,"26",0,"1",0,"4","3002"],
  ["2016-10-10T09:21:33.955Z",0,"684354270",0,"26",0,"2",0,"3","3002"],
  ["2016-10-10T09:21:33.855Z",2,"3616844488",0,"10",3,"1",0,"0","3002"],
  ["2016-10-10T09:21:33.729Z",23,"444197787",0,"16",19,"1",0,"0","3002"]
];

const columns = ["time","criminals","district_instance_type_sdd","district_status","district_uid","enforcers","instance_num","queue_size","threat","world_uid"];

const state = {
  instances: [],
  sortDesc: true,
  sortKey: 'population',
  activeWorld: 'jericho'
};

for (var entry of data) {
  var instanceData = {};
  for (var i = 0; i < entry.length; i++) {
    instanceData[columns[i]] = entry[i];
  }
  state.instances.push(instanceData);
}

console.log(state);

function reducer(state = {}, action) {
  console.log("REDUCER ACTION: ", action);
  if (action.type === 'SORT_TABLE') {
    return Object.assign({}, state, { sortKey: action.key, sortDesc: (action.key === state.sortKey) ? !state.sortDesc : false });
  }

  return Object.assign({}, state);
}

const store = createStore(reducer, state, window.__REDUX_DEVTOOLS_EXTENSION__ && window.__REDUX_DEVTOOLS_EXTENSION__());

class App extends Component {
  //state = { activeItem: 'jericho' }
  columns = {
    'threat': 'Threat',
    'name': 'Name',
    'rule_set': 'Rule Set',
    'population': 'Population',
    'enforcers': 'Enforcers',
    'criminals': 'Criminals'
  }

  handleItemClick = (e, { name }) => this.setState({ activeItem: name })

  render() {
    //const { activeItem } = this.state;
    const state = store.getState();

    return (
      <Provider store={store}>
        <Container>
          <Header as='h1'>
            <Icon name='heartbeat' />
            <Header.Content>APB Watcher</Header.Content>
          </Header>
          <Menu pointing secondary>
            <Menu.Item name='jericho' active={state.activeWorld === 'jericho'} onClick={this.handleItemClick}>Jericho (NA)</Menu.Item>
            {/*<Menu.Item name='citadel' active={activeItem === 'citadel'} onClick={this.handleItemClick}>Citadel (EU)</Menu.Item>
            <Menu.Item name='han' active={activeItem === 'han'} onClick={this.handleItemClick}>Han (Hong Kong)</Menu.Item>*/}
          </Menu>
          <Table celled className="sortable">
            <Table.Header>
              <Table.Row>
                { Object.keys(this.columns).map(key => {
                  return <TableHeader 
                    key={ key } 
                    id={ key } 
                    sortKey={ state.sortKey } 
                    sortDesc={ state.sortDesc }
                    onClick={() => {
                      store.dispatch({
                        type: 'SORT_TABLE',
                        key: key
                      });
                    }}
                  >
                    { this.columns[key] }
                  </TableHeader>;
                }) }
              </Table.Row>
            </Table.Header>
            <Table.Body>
              { state.instances.map((instance, i) => {
                return (<InstanceRow instance={instance} key={i} />);
              }) }
            </Table.Body>
          </Table>
          <div>
            Last updated: 2 minutes ago
            <Button floated='right' icon='refresh' content='Refresh' />
          </div>
        </Container>
      </Provider>
    );
  }
}

const render = () => {
  console.log('Rendering');
  ReactDOM.render(
    <App />,
    document.getElementById('root')
  );  
}

store.subscribe(render);

export default render;
