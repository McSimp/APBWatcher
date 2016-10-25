const types = {
  '4008467211': {
    ruleSet: 'Missions',
    area: 'Financial',
    language: 'EN',
  },
  '3616844488': {
    ruleSet: 'Fight Club',
    area: 'Abington Towers',
    language: 'EN',
  },
  '2858091178': {
    ruleSet: 'Dynamic Event',
    area: 'Waterfront - Anarchy',
    language: 'EN',
  },
  '684354270': {
    ruleSet: 'Missions',
    area: 'Waterfront',
    language: 'EN',
  },
  '444197787': {
    ruleSet: 'Social',
    area: 'Breakwater Marina',
    language: 'EN',
  },
  '1094540976': {
    ruleSet: 'Fight Club',
    area: 'Baylan Shipping Storage',
    language: 'EN',
  },
  '3695193608': {
    ruleSet: 'Open Conflict',
    area: 'Waterfront',
    language: 'EN',
  },
  '3675975819': {
    ruleSet: 'Open Conflict',
    area: 'Financial',
    language: 'EN',
  },
  '416275946': {
    ruleSet: 'Dynamic Event',
    area: 'Financial',
    language: 'EN',
  },
  '1850553705': {
    ruleSet: 'Dynamic Event',
    area: 'Waterfront',
    language: 'EN',
  }
};

// TODO: What are the other worlds?
const worlds = {
  '3002': 'jericho',
  '3103': 'citadel'
};

export default class Instance {
  constructor(data) {
    this.data = data;
  }

  get criminals() {
    return this.data.criminals;
  }

  get enforcers() {
    return this.data.enforcers;
  }

  get threat() {
    return this.data.threat;
  }

  get districtInstanceTypeSdd() {
    return this.data.district_instance_type_sdd;
  }

  get instanceNum() {
    return this.data.instance_num;
  }

  get districtStatus() {
    return this.data.district_status;
  }

  get districtUid() {
    return this.data.district_uid;
  }

  get worldUid() {
    return this.data.world_uid;
  }

  get worldName() {
    if (this.worldUid in worlds) {
      return worlds[this.worldUid];
    }

    return 'Unknown';
  }

  // Computed properties
  get population() {
    return this.criminals + this.enforcers;
  }

  get fullName() {
    const sdd = this.districtInstanceTypeSdd;
    if (sdd in types) {
      return types[sdd].ruleSet + '-' + types[sdd].area + '-' + types[sdd].language + '-' + this.instanceNum;
    } else {
      return 'Unknown';
    }
  }

  get ruleSet() {
    const sdd = this.districtInstanceTypeSdd;
    if (sdd in types) {
      return types[sdd].ruleSet;
    } else {
      return 'Unknown';
    }
  }
}
