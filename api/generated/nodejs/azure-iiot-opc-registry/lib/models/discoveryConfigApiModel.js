/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 *
 * Code generated by Microsoft (R) AutoRest Code Generator 1.0.0.0
 * Changes may cause incorrect behavior and will be lost if the code is
 * regenerated.
 */

'use strict';

/**
 * Discovery configuration
 *
 */
class DiscoveryConfigApiModel {
  /**
   * Create a DiscoveryConfigApiModel.
   * @property {string} [addressRangesToScan] Address ranges to scan (null ==
   * all wired nics)
   * @property {number} [networkProbeTimeoutMs] Network probe timeout
   * @property {number} [maxNetworkProbes] Max network probes that should ever
   * run.
   * @property {string} [portRangesToScan] Port ranges to scan (null == all
   * unassigned)
   * @property {number} [portProbeTimeoutMs] Port probe timeout
   * @property {number} [maxPortProbes] Max port probes that should ever run.
   * @property {number} [minPortProbesPercent] Probes that must always be there
   * as percent of max.
   * @property {number} [idleTimeBetweenScansSec] Delay time between discovery
   * sweeps in seconds
   * @property {array} [discoveryUrls] List of preset discovery urls to use
   * @property {array} [locales] List of locales to filter with during
   * discovery
   * @property {array} [callbacks] Callbacks to invoke once onboarding finishes
   * @property {object} [activationFilter] Activate all twins with this filter
   * during onboarding.
   * @property {array} [activationFilter.trustLists] Certificate trust list
   * identifiers to use for
   * activation, if null, all certificates are
   * trusted.  If empty list, no certificates are
   * trusted which is equal to no filter.
   * @property {array} [activationFilter.securityPolicies] Endpoint security
   * policies to filter against.
   * If set to null, all policies are in scope.
   * @property {string} [activationFilter.securityMode] Security mode level to
   * activate. If null,
   * then Microsoft.Azure.IIoT.OpcUa.Registry.Models.SecurityMode.Best is
   * assumed. Possible values include: 'Best', 'Sign', 'SignAndEncrypt', 'None'
   */
  constructor() {
  }

  /**
   * Defines the metadata of DiscoveryConfigApiModel
   *
   * @returns {object} metadata of DiscoveryConfigApiModel
   *
   */
  mapper() {
    return {
      required: false,
      serializedName: 'DiscoveryConfigApiModel',
      type: {
        name: 'Composite',
        className: 'DiscoveryConfigApiModel',
        modelProperties: {
          addressRangesToScan: {
            required: false,
            serializedName: 'addressRangesToScan',
            type: {
              name: 'String'
            }
          },
          networkProbeTimeoutMs: {
            required: false,
            serializedName: 'networkProbeTimeoutMs',
            type: {
              name: 'Number'
            }
          },
          maxNetworkProbes: {
            required: false,
            serializedName: 'maxNetworkProbes',
            type: {
              name: 'Number'
            }
          },
          portRangesToScan: {
            required: false,
            serializedName: 'portRangesToScan',
            type: {
              name: 'String'
            }
          },
          portProbeTimeoutMs: {
            required: false,
            serializedName: 'portProbeTimeoutMs',
            type: {
              name: 'Number'
            }
          },
          maxPortProbes: {
            required: false,
            serializedName: 'maxPortProbes',
            type: {
              name: 'Number'
            }
          },
          minPortProbesPercent: {
            required: false,
            serializedName: 'minPortProbesPercent',
            type: {
              name: 'Number'
            }
          },
          idleTimeBetweenScansSec: {
            required: false,
            serializedName: 'idleTimeBetweenScansSec',
            type: {
              name: 'Number'
            }
          },
          discoveryUrls: {
            required: false,
            serializedName: 'discoveryUrls',
            type: {
              name: 'Sequence',
              element: {
                  required: false,
                  serializedName: 'StringElementType',
                  type: {
                    name: 'String'
                  }
              }
            }
          },
          locales: {
            required: false,
            serializedName: 'locales',
            type: {
              name: 'Sequence',
              element: {
                  required: false,
                  serializedName: 'StringElementType',
                  type: {
                    name: 'String'
                  }
              }
            }
          },
          callbacks: {
            required: false,
            serializedName: 'callbacks',
            type: {
              name: 'Sequence',
              element: {
                  required: false,
                  serializedName: 'CallbackApiModelElementType',
                  type: {
                    name: 'Composite',
                    className: 'CallbackApiModel'
                  }
              }
            }
          },
          activationFilter: {
            required: false,
            serializedName: 'activationFilter',
            type: {
              name: 'Composite',
              className: 'EndpointActivationFilterApiModel'
            }
          }
        }
      }
    };
  }
}

module.exports = DiscoveryConfigApiModel;