import React, { Component, PropTypes } from 'react';
import { connect } from 'react-redux';
import ReactHighcharts from 'react-highcharts';
import { Loader } from '../semantic-ui';
import { requestPlayerStatsRefresh } from '../actions';

// TODO: Move this to class
var chartConfig = {
  chart: {
    height: 250
  },
  title: {
    text: null
  },
  xAxis: {
    type: 'datetime'
  },
  yAxis: {
    title: {
      text: 'Players'
    },
  },
  series: [
    { 
      type: 'area',
      name: 'Jericho (NA)',
      data: [],
      color: '#FF6384'
    },
    { 
      type: 'area',
      name: 'Citadel (EU)',
      data: [],
      color: '#36A2EB'
    },
    { 
      type: 'area',
      name: 'Han (Hong Kong)',
      data: [],
      color: '#FFCE56'
    },
  ],
  credits: {
    enabled: false
  },
  exporting: {
    enabled: false
  },
  rangeSelector: {
    enabled: false
  },
  scrollbar: {
    enabled: false
  },
  navigator: {
    enabled: false
  },
  tooltip: {
    shared: true,
    crosshairs: true,
    valueSuffix: ' players'
  },
  plotOptions: {
    area: {
      fillOpacity: 0.1,
      lineWidth: 2,
      states: {
        hover: {
          lineWidthPlus: 0
        }
      }
    }
  },
  legend: {
    align: 'center',
    verticalAlign: 'top',
    borderWidth: 0
  },
};

function getSeriesData(stats, worldUid) {
  let data = [];
  let points = stats[worldUid]
  for (let i = 0; i < points.length; i++) {
    data.push([stats.times[i], points[i]]);
  }
  return data;
}

class PlayerChart extends Component {
  static propTypes = {
    playerStats: PropTypes.object
  }

  componentDidMount() {
    ReactHighcharts.Highcharts.setOptions({
      global: {
        useUTC: false
      }
    });

    this.props.requestPlayerStatsRefresh();
  }

  render() {
    const stats = this.props.playerStats;
    if (stats === null) {
      return <Loader active inline='centered'>Loading player statistics</Loader>;
    }

    chartConfig.series[0].data = getSeriesData(stats, '3002');
    chartConfig.series[1].data = getSeriesData(stats, '3103');
    chartConfig.series[2].data = getSeriesData(stats, '3151');

    return <ReactHighcharts config={chartConfig} />;
  }
}

const mapStateToProps = (state) => ({
  playerStats: state.playerStats
});

export default connect(mapStateToProps, {
  requestPlayerStatsRefresh
})(PlayerChart);
