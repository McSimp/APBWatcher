import Instance from '../models/instance.js';

export const REFRESH_INSTANCES = 'REFRESH_INSTANCES';
export const RECEIVE_INSTANCES = 'RECEIVE_INSTANCES';
export const SET_ACTIVE_WORLD = 'SET_ACTIVE_WORLD';

const refreshInstances = () => ({
  type: REFRESH_INSTANCES
});

const receiveInstances = instances => ({
  type: RECEIVE_INSTANCES,
  instances,
  receivedAt: Date.now()
});

const fetchInstances = dispatch => {
  dispatch(refreshInstances());
  return fetch('https://will.io/apb/instances.php')
    .then(response => response.json())
    .then(json => {
      let instances = [];
      for (const instanceData of json) {
        instances.push(new Instance(instanceData));
      }

      dispatch(receiveInstances(instances));
    })
};

export const requestInstanceRefresh = () => (dispatch, getState) => {
  const state = getState();
  if (!state.isFetchingInstances) {
    return dispatch(fetchInstances);
  }
};

export const setActiveWorld = (world) => ({
  type: SET_ACTIVE_WORLD,
  world
});
