import Instance from '../models/instance.js';

export const REFRESH_INSTANCES = 'REFRESH_INSTANCES';
export const RECEIVE_INSTANCES = 'RECEIVE_INSTANCES';
export const SET_ACTIVE_WORLD = 'SET_ACTIVE_WORLD';
export const REFRESH_PLAYER_STATS = 'REFRESH_PLAYER_STATS';
export const RECEIVE_PLAYER_STATS = 'RECEIVE_PLAYER_STATS';

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

const refreshPlayerStats = () => ({
  type: REFRESH_PLAYER_STATS
});

const receivePlayerStats = stats => ({
  type: RECEIVE_PLAYER_STATS,
  stats,
  receivedAt: Date.now()
});

const fetchPlayerStats = dispatch => {
  dispatch(refreshPlayerStats());
  return fetch('https://will.io/apb/stats.php')
    .then(response => response.json())
    .then(json => {
      dispatch(receivePlayerStats(json));
    })
};

export const requestPlayerStatsRefresh = () => (dispatch, getState) => {
  const state = getState();
  if (!state.isFetchingPlayerStats) {
    return dispatch(fetchPlayerStats);
  }
};
