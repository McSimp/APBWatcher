import * as ActionTypes from '../actions';

const rootReducer = (state={}, action) => {
  switch (action.type) {
    case ActionTypes.REFRESH_INSTANCES:
      return {
        ...state,
        isFetchingInstances: true
      };
    case ActionTypes.RECEIVE_INSTANCES:
      return {
        ...state,
        lastUpdated: action.receivedAt,
        instances: action.instances,
        isFetchingInstances: false
      };
    case ActionTypes.SET_ACTIVE_WORLD:
      return {
        ...state,
        activeWorld: action.world
      };
    default:
      return state;
  }
};

export default rootReducer;
