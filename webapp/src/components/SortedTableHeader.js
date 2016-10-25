import React, { PropTypes } from 'react';
import { Table } from 'semantic-ui-react';

const SortedTableHeader = ({id, sortKey, sortDesc, children, onClick, ...props}) => {
  let classes = null;
  if (sortKey === id) {
    if (sortDesc) {
      classes = 'sorted descending';
    } else {
      classes = 'sorted ascending';
    }
  }
  return (<Table.HeaderCell className={ classes } onClick={ onClick } {...props}>{ children }</Table.HeaderCell>);
};

SortedTableHeader.propTypes = {
  id: PropTypes.string.isRequired,
  sortKey: PropTypes.string.isRequired,
  sortDesc: PropTypes.bool.isRequired,
  onClick: PropTypes.func.isRequired,
};

export default SortedTableHeader;
