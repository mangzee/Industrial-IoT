// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Extensions.Configuration {
    using Microsoft.Extensions.Primitives;
    using Microsoft.Azure.IIoT.Auth.KeyVault;
    using Microsoft.Azure.IIoT;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Models;
    using Serilog;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.IIoT.Exceptions;

    /// <summary>
    /// Extension methods
    /// </summary>
    public static class ConfigurationEx {

        /// <summary>
        /// Add configuration from azure keyvault.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="singleton"></param>
        /// <param name="keyVaultUrlVarName"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddFromKeyVault(
            this IConfigurationBuilder builder, bool singleton = true,
            string keyVaultUrlVarName = null) {

            var provider = KeyVaultConfigurationProvider.CreateInstanceAsync(
                singleton, builder.Build(), keyVaultUrlVarName).Result;
            if (provider != null) {
                builder.Add(provider);
            }
            return builder;
        }

        /// <summary>
        /// Keyvault configuration provider.
        /// </summary>
        internal sealed class KeyVaultConfigurationProvider : IConfigurationSource,
            IConfigurationProvider {

            /// <summary>
            /// Create keyvault provider
            /// </summary>
            /// <param name="configuration"></param>
            /// <param name="keyVaultUri"></param>
            private KeyVaultConfigurationProvider(IConfigurationRoot configuration,
                string keyVaultUri) {
                _keyVault = new KeyVaultClientBootstrap(configuration);
                _keyVaultUri = keyVaultUri;
                _cache = new ConcurrentDictionary<string, Task<SecretBundle>>();
                _reloadToken = new ConfigurationReloadToken();
            }

            /// <inheritdoc/>
            public IConfigurationProvider Build(IConfigurationBuilder builder) {
                return this;
            }

            /// <inheritdoc/>
            public bool TryGet(string key, out string value) {
                value = null;
                if (!key.StartsWith("PCS_", StringComparison.InvariantCultureIgnoreCase)) {
                    return false;
                }
                try {
                    if (_allSecretsLoaded && !_cache.ContainsKey(key)) {
                        // Prevents non existant keys to be looked up
                        return false;
                    }
                    var resultTask = _cache.GetOrAdd(key, k => {
                        return _keyVault.Client.GetSecretAsync(_keyVaultUri,
                            GetSecretNameForKey(k));
                    });
                    if (resultTask.IsFaulted || resultTask.IsCanceled) {
                        return false;
                    }
                    value = resultTask.Result.Value;
                    return true;
                }
                catch {
                    return false;
                }
            }

            /// <inheritdoc/>
            public void Set(string key, string value) {
                // No op
            }

            /// <inheritdoc/>
            public void Load() {
                // No op
            }

            /// <inheritdoc/>
            public IChangeToken GetReloadToken() {
                return _reloadToken;
            }

            public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys,
                string parentPath) {
                // Not supported
                return Enumerable.Empty<string>();
            }

            /// <summary>
            /// Create configuration provider
            /// </summary>
            /// <param name="singleton"></param>
            /// <param name="configuration"></param>
            /// <param name="keyVaultUrlVarName"></param>
            /// <returns></returns>
            public static async Task<KeyVaultConfigurationProvider> CreateInstanceAsync(
                bool singleton, IConfigurationRoot configuration, string keyVaultUrlVarName) {
                if (string.IsNullOrEmpty(keyVaultUrlVarName)) {
                    keyVaultUrlVarName = PcsVariable.PCS_KEYVAULT_URL;
                }
                if (singleton) {
                    // Save singleton creation
                    if (_singleton == null) {
                        lock (kLock) {
                            if (_singleton == null) {
                                // Create instance
                                _singleton = CreateInstanceAsync(configuration,
                                    keyVaultUrlVarName, false);
                            }
                        }
                    }
                    return await _singleton;
                }
                // Create new instance
                return await CreateInstanceAsync(configuration, keyVaultUrlVarName, true);
            }

            /// <summary>
            /// Create new instance
            /// </summary>
            /// <param name="configuration"></param>
            /// <param name="keyVaultUrlVarName"></param>
            /// <param name="lazyLoad"></param>
            /// <returns></returns>
            private static async Task<KeyVaultConfigurationProvider> CreateInstanceAsync(
                IConfigurationRoot configuration, string keyVaultUrlVarName, bool lazyLoad) {

                var vaultUri = configuration.GetValue<string>(keyVaultUrlVarName, null);
                if (string.IsNullOrEmpty(vaultUri)) {
                    Log.Logger.Debug("No keyvault uri found in configuration under {key}. ",
                        keyVaultUrlVarName);
                    vaultUri = Environment.GetEnvironmentVariable(keyVaultUrlVarName);
                    if (string.IsNullOrEmpty(vaultUri)) {
                        Log.Logger.Debug("No keyvault uri found in environment under {key}. " +
                            "Not reading configuration from keyvault without keyvault uri.",
                            keyVaultUrlVarName);
                        return null;
                    }
                }

                var provider = new KeyVaultConfigurationProvider(configuration, vaultUri);
                if (!await provider.TryReadSecretAsync(keyVaultUrlVarName)) {
                    throw new InvalidConfigurationException(
                        "A keyvault uri was provided could not access keyvault at the address. " +
                        "If you want to read configuration from keyvault, make sure " +
                        "the keyvault is reachable, the required permissions are configured " +
                        "on keyvault and authentication provider information is available. " +
                        "Sign into Visual Studio or Azure CLI on this machine and try again.");
                }
                else if (!lazyLoad) {
                    await provider.LoadAllSecretsAsync();
                }
                return provider;
            }

            /// <summary>
            /// Read configuration secret
            /// </summary>
            /// <param name="secretName"></param>
            /// <returns></returns>
            private async Task<bool> TryReadSecretAsync(string secretName) {
                try {
                    var secret = await _keyVault.Client.GetSecretAsync(_keyVaultUri,
                        GetSecretNameForKey(secretName)).ConfigureAwait(false);

                    // Worked - we have a working keyvault client.
                    return true;
                }
                catch (Exception ex) {
                    Log.Logger.Error(ex, "Failed to access keyvault {url}.",
                        _keyVaultUri);
                    return false;
                }
            }

            /// <summary>
            /// Preload cache
            /// </summary>
            /// <returns></returns>
            private async Task LoadAllSecretsAsync() {
                // Read all secrets
                var secretPage = await _keyVault.Client.GetSecretsAsync(_keyVaultUri)
                    .ConfigureAwait(false);
                var allSecrets = new List<SecretItem>(secretPage.ToList());
                while (true) {
                    if (secretPage.NextPageLink == null) {
                        break;
                    }
                    secretPage =  await _keyVault.Client.GetSecretsNextAsync(
                        secretPage.NextPageLink).ConfigureAwait(false);
                    allSecrets.AddRange(secretPage.ToList());
                }
                foreach (var secret in allSecrets) {
                    if (secret.Attributes?.Enabled != true) {
                        continue;
                    }
                    var key = GetKeyForSecretName(secret.Identifier.Name);
                    if (key == null) {
                        continue;
                    }
                    _cache.TryAdd(key, _keyVault.Client.GetSecretAsync(
                        secret.Identifier.Identifier));
                }
                _allSecretsLoaded = true;
                await Task.WhenAll(_cache.Values).ConfigureAwait(false);
            }

            /// <summary>
            /// Get secret key for key value. Replace any upper case
            /// letters with lower case and _ with -.
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            private static string GetSecretNameForKey(string key) {
                return key.Replace("_", "-").ToLowerInvariant();
            }

            /// <summary>
            /// Get secret key for key value. Replace any upper case
            /// letters with lower case and _ with -.
            /// </summary>
            /// <param name="secretId"></param>
            /// <returns></returns>
            private static string GetKeyForSecretName(string secretId) {
                if (!secretId.StartsWith("pcs-")) {
                    return null;
                }
                return secretId.Replace("-", "_").ToUpperInvariant();
            }

            private static readonly object kLock = new object();
            private static Task<KeyVaultConfigurationProvider> _singleton;

            private readonly string _keyVaultUri;
            private readonly KeyVaultClientBootstrap _keyVault;
            private readonly ConcurrentDictionary<string, Task<SecretBundle>> _cache;
            private readonly ConfigurationReloadToken _reloadToken;
            private bool _allSecretsLoaded;
        }
    }
}
