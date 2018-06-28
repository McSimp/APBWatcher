import React, { Component, PropTypes } from 'react';
import { connect } from 'react-redux';
import { Menu } from '../semantic-ui';
import { setActiveWorld } from '../actions';

class WorldMenu extends Component {
  static propTypes = {
    activeWorld: PropTypes.string.isRequired,
    setActiveWorld: PropTypes.func.isRequired
  }

  render() {
    const { activeWorld, setActiveWorld } = this.props;
    return (
      <Menu pointing secondary>
        <Menu.Item name='jericho' active={activeWorld === 'jericho'} onClick={ () => { setActiveWorld('jericho'); } }>Jericho (NA)</Menu.Item>
        <Menu.Item name='citadel' active={activeWorld === 'citadel'} onClick={ () => { setActiveWorld('citadel'); } }>Citadel (EU)</Menu.Item>
        <Menu.Item name='han' active={activeWorld === 'han'} onClick={ () => { setActiveWorld('han'); } }>Han (Hong Kong)</Menu.Item>
        <Menu.Item name='nekrova' active={activeWorld === 'nekrova'} onClick={ () => { setActiveWorld('nekrova'); } }>Nekrova (RU)</Menu.Item>
      </Menu>
    );
  }
}

const mapStateToProps = (state) => ({
  activeWorld: state.activeWorld
});

export default connect(mapStateToProps, {
  setActiveWorld
})(WorldMenu);
