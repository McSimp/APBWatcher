import React, { Component, PropTypes } from 'react';
import { Button } from 'semantic-ui-react';
import TimeAgo from 'react-timeago';
import { connect } from 'react-redux';
import { requestInstanceRefresh } from '../actions';

class RefreshBar extends Component {
  static propTypes = {
    lastUpdated: PropTypes.number,
    isFetchingInstances: PropTypes.bool.isRequired,
    requestInstanceRefresh: PropTypes.func.isRequired,
  }

  canRefresh() {
    if (this.props.isFetchingInstances) {
      return false;
    }

    return true;
  }

  render() {
    const { lastUpdated, isFetchingInstances, requestInstanceRefresh } = this.props;
    return (
      <div>
        Last updated: { lastUpdated ? <TimeAgo date={lastUpdated} /> : 'never' }
        <Button floated='right' icon='refresh' content='Refresh' onClick={ () => { requestInstanceRefresh(); } } loading={isFetchingInstances} disabled={!this.canRefresh()} />
      </div>
    );
  }
}

const mapStateToProps = (state) => ({
  lastUpdated: state.lastUpdated,
  isFetchingInstances: state.isFetchingInstances
});

export default connect(mapStateToProps, {
  requestInstanceRefresh
})(RefreshBar);
