import React, { PropTypes } from 'react';
import { Table } from 'semantic-ui-react';

const TableHeader = ({id, sortKey, sortDesc, children, onClick}) => {
  let classes = null;
  if (sortKey === id) {
    if (sortDesc) {
      classes = "sorted descending";
    } else {
      classes = "sorted ascending";
    }
  }
  return (<Table.HeaderCell className={ classes } onClick={ onClick }>{ children }</Table.HeaderCell>);
};

TableHeader.propTypes = {
  id: PropTypes.string.isRequired,
  sortKey: PropTypes.string.isRequired,
  sortDesc: PropTypes.bool.isRequired,
  onClick: PropTypes.func,
};

export default TableHeader;
