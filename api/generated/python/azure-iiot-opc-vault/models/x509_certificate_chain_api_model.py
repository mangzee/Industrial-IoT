# coding=utf-8
# --------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
#
# Code generated by Microsoft (R) AutoRest Code Generator 2.3.33.0
# Changes may cause incorrect behavior and will be lost if the code is
# regenerated.
# --------------------------------------------------------------------------

from msrest.serialization import Model


class X509CertificateChainApiModel(Model):
    """Certificate chain.

    :param chain: Chain
    :type chain: list[~azure-iiot-opc-vault.models.X509CertificateApiModel]
    """

    _attribute_map = {
        'chain': {'key': 'chain', 'type': '[X509CertificateApiModel]'},
    }

    def __init__(self, chain=None):
        super(X509CertificateChainApiModel, self).__init__()
        self.chain = chain