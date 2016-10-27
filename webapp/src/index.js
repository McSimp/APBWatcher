import React from 'react';
import { render } from 'react-dom';
import Root from './containers/Root';
import configureStore from './store/configureStore';
import 'core-js/fn/string/starts-with';
import 'core-js/es6/object';
import 'core-js/es6/symbol';
import 'core-js/es6/map';

const store = configureStore({
  instances: [],
  isFetchingInstances: false,
  activeWorld: 'jericho',
  lastUpdated: null
});

render(
  <Root store={store} />,
  document.getElementById('root')
);
