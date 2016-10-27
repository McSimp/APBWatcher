import React, { PropTypes } from 'react';
import { Icon, Popup } from '../semantic-ui';

const threats = {
  '1': { 
    color: 'green',
    name: 'Green',
  },
  '2': {
    color: 'brown',
    name: 'Bronze',
  },
  '3': {
    color: 'grey',
    name: 'Silver',
  },
  '4': {
    color: 'yellow',
    name: 'Gold',
  },
};

const ThreatIndicator = ({threat}) => {
  if (threat in threats) {
    return (<Popup
      trigger={<Icon name='circle' color={ threats[threat].color } />}
      content={ threats[threat].name }
      inverted
    />);
  } else {
    return null;
  }
};

ThreatIndicator.propTypes = {
  threat: PropTypes.string.isRequired,
};

export default ThreatIndicator;
