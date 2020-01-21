/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 *
 * Code generated by Microsoft (R) AutoRest Code Generator 1.0.0.0
 * Changes may cause incorrect behavior and will be lost if the code is
 * regenerated.
 */

package com.microsoft.azure.iiot.opc.publisher.implementation;

import com.microsoft.azure.iiot.opc.publisher.AzureOpcPublisherClient;
import com.microsoft.rest.ServiceClient;
import com.microsoft.rest.RestClient;
import okhttp3.OkHttpClient;
import retrofit2.Retrofit;
import com.google.common.reflect.TypeToken;
import com.microsoft.azure.iiot.opc.publisher.models.PublishedItemListRequestApiModel;
import com.microsoft.azure.iiot.opc.publisher.models.PublishedItemListResponseApiModel;
import com.microsoft.azure.iiot.opc.publisher.models.PublishStartRequestApiModel;
import com.microsoft.azure.iiot.opc.publisher.models.PublishStartResponseApiModel;
import com.microsoft.azure.iiot.opc.publisher.models.PublishStopRequestApiModel;
import com.microsoft.azure.iiot.opc.publisher.models.PublishStopResponseApiModel;
import com.microsoft.rest.RestException;
import com.microsoft.rest.ServiceCallback;
import com.microsoft.rest.ServiceFuture;
import com.microsoft.rest.ServiceResponse;
import com.microsoft.rest.Validator;
import java.io.IOException;
import okhttp3.ResponseBody;
import retrofit2.http.Body;
import retrofit2.http.GET;
import retrofit2.http.Headers;
import retrofit2.http.HTTP;
import retrofit2.http.Path;
import retrofit2.http.POST;
import retrofit2.http.PUT;
import retrofit2.http.Query;
import retrofit2.Response;
import rx.functions.Func1;
import rx.Observable;

/**
 * Initializes a new instance of the AzureOpcPublisherClient class.
 */
public class AzureOpcPublisherClientImpl extends ServiceClient implements AzureOpcPublisherClient {
    /**
     * The Retrofit service to perform REST calls.
     */
    private AzureOpcPublisherClientService service;

    /**
     * Initializes an instance of AzureOpcPublisherClient client.
     */
    public AzureOpcPublisherClientImpl() {
        this("http://localhost:9080");
    }

    /**
     * Initializes an instance of AzureOpcPublisherClient client.
     *
     * @param baseUrl the base URL of the host
     */
    public AzureOpcPublisherClientImpl(String baseUrl) {
        super(baseUrl);
        initialize();
    }

    /**
     * Initializes an instance of AzureOpcPublisherClient client.
     *
     * @param clientBuilder the builder for building an OkHttp client, bundled with user configurations
     * @param restBuilder the builder for building an Retrofit client, bundled with user configurations
     */
    public AzureOpcPublisherClientImpl(OkHttpClient.Builder clientBuilder, Retrofit.Builder restBuilder) {
        this("http://localhost:9080", clientBuilder, restBuilder);
        initialize();
    }

    /**
     * Initializes an instance of AzureOpcPublisherClient client.
     *
     * @param baseUrl the base URL of the host
     * @param clientBuilder the builder for building an OkHttp client, bundled with user configurations
     * @param restBuilder the builder for building an Retrofit client, bundled with user configurations
     */
    public AzureOpcPublisherClientImpl(String baseUrl, OkHttpClient.Builder clientBuilder, Retrofit.Builder restBuilder) {
        super(baseUrl, clientBuilder, restBuilder);
        initialize();
    }

    /**
     * Initializes an instance of AzureOpcPublisherClient client.
     *
     * @param restClient the REST client containing pre-configured settings
     */
    public AzureOpcPublisherClientImpl(RestClient restClient) {
        super(restClient);
        initialize();
    }

    private void initialize() {
        initializeService();
    }

    private void initializeService() {
        service = retrofit().create(AzureOpcPublisherClientService.class);
    }

    /**
     * The interface defining all the services for AzureOpcPublisherClient to be
     * used by Retrofit to perform actually REST calls.
     */
    interface AzureOpcPublisherClientService {
        @Headers({ "Content-Type: application/json-patch+json; charset=utf-8", "x-ms-logging-context: com.microsoft.azure.iiot.opc.publisher.AzureOpcPublisherClient subscribe" })
        @PUT("v2/monitor/{endpointId}/samples")
        Observable<Response<ResponseBody>> subscribe(@Path("endpointId") String endpointId, @Body String body);

        @Headers({ "Content-Type: application/json; charset=utf-8", "x-ms-logging-context: com.microsoft.azure.iiot.opc.publisher.AzureOpcPublisherClient unsubscribe" })
        @HTTP(path = "v2/monitor/{endpointId}/samples/{userId}", method = "DELETE", hasBody = true)
        Observable<Response<ResponseBody>> unsubscribe(@Path("endpointId") String endpointId, @Path("userId") String userId);

        @Headers({ "Content-Type: application/json-patch+json; charset=utf-8", "x-ms-logging-context: com.microsoft.azure.iiot.opc.publisher.AzureOpcPublisherClient startPublishingValues" })
        @POST("v2/publish/{endpointId}/start")
        Observable<Response<ResponseBody>> startPublishingValues(@Path("endpointId") String endpointId, @Body PublishStartRequestApiModel body);

        @Headers({ "Content-Type: application/json-patch+json; charset=utf-8", "x-ms-logging-context: com.microsoft.azure.iiot.opc.publisher.AzureOpcPublisherClient stopPublishingValues" })
        @POST("v2/publish/{endpointId}/stop")
        Observable<Response<ResponseBody>> stopPublishingValues(@Path("endpointId") String endpointId, @Body PublishStopRequestApiModel body);

        @Headers({ "Content-Type: application/json-patch+json; charset=utf-8", "x-ms-logging-context: com.microsoft.azure.iiot.opc.publisher.AzureOpcPublisherClient getFirstListOfPublishedNodes" })
        @POST("v2/publish/{endpointId}")
        Observable<Response<ResponseBody>> getFirstListOfPublishedNodes(@Path("endpointId") String endpointId, @Body PublishedItemListRequestApiModel body);

        @Headers({ "Content-Type: application/json; charset=utf-8", "x-ms-logging-context: com.microsoft.azure.iiot.opc.publisher.AzureOpcPublisherClient getNextListOfPublishedNodes" })
        @GET("v2/publish/{endpointId}")
        Observable<Response<ResponseBody>> getNextListOfPublishedNodes(@Path("endpointId") String endpointId, @Query("continuationToken") String continuationToken);

    }

    /**
     * Subscribe to receive samples.
     * Register a client to receive publisher samples through SignalR.
     *
     * @param endpointId The endpoint to subscribe to
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @throws RestException thrown if the request is rejected by server
     * @throws RuntimeException all other wrapped checked exceptions if the request fails to be sent
     */
    public void subscribe(String endpointId) {
        subscribeWithServiceResponseAsync(endpointId).toBlocking().single().body();
    }

    /**
     * Subscribe to receive samples.
     * Register a client to receive publisher samples through SignalR.
     *
     * @param endpointId The endpoint to subscribe to
     * @param serviceCallback the async ServiceCallback to handle successful and failed responses.
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the {@link ServiceFuture} object
     */
    public ServiceFuture<Void> subscribeAsync(String endpointId, final ServiceCallback<Void> serviceCallback) {
        return ServiceFuture.fromResponse(subscribeWithServiceResponseAsync(endpointId), serviceCallback);
    }

    /**
     * Subscribe to receive samples.
     * Register a client to receive publisher samples through SignalR.
     *
     * @param endpointId The endpoint to subscribe to
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the {@link ServiceResponse} object if successful.
     */
    public Observable<Void> subscribeAsync(String endpointId) {
        return subscribeWithServiceResponseAsync(endpointId).map(new Func1<ServiceResponse<Void>, Void>() {
            @Override
            public Void call(ServiceResponse<Void> response) {
                return response.body();
            }
        });
    }

    /**
     * Subscribe to receive samples.
     * Register a client to receive publisher samples through SignalR.
     *
     * @param endpointId The endpoint to subscribe to
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the {@link ServiceResponse} object if successful.
     */
    public Observable<ServiceResponse<Void>> subscribeWithServiceResponseAsync(String endpointId) {
        if (endpointId == null) {
            throw new IllegalArgumentException("Parameter endpointId is required and cannot be null.");
        }
        final String body = null;
        return service.subscribe(endpointId, body)
            .flatMap(new Func1<Response<ResponseBody>, Observable<ServiceResponse<Void>>>() {
                @Override
                public Observable<ServiceResponse<Void>> call(Response<ResponseBody> response) {
                    try {
                        ServiceResponse<Void> clientResponse = subscribeDelegate(response);
                        return Observable.just(clientResponse);
                    } catch (Throwable t) {
                        return Observable.error(t);
                    }
                }
            });
    }

    /**
     * Subscribe to receive samples.
     * Register a client to receive publisher samples through SignalR.
     *
     * @param endpointId The endpoint to subscribe to
     * @param body The user id that will receive publisher samples.
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @throws RestException thrown if the request is rejected by server
     * @throws RuntimeException all other wrapped checked exceptions if the request fails to be sent
     */
    public void subscribe(String endpointId, String body) {
        subscribeWithServiceResponseAsync(endpointId, body).toBlocking().single().body();
    }

    /**
     * Subscribe to receive samples.
     * Register a client to receive publisher samples through SignalR.
     *
     * @param endpointId The endpoint to subscribe to
     * @param body The user id that will receive publisher samples.
     * @param serviceCallback the async ServiceCallback to handle successful and failed responses.
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the {@link ServiceFuture} object
     */
    public ServiceFuture<Void> subscribeAsync(String endpointId, String body, final ServiceCallback<Void> serviceCallback) {
        return ServiceFuture.fromResponse(subscribeWithServiceResponseAsync(endpointId, body), serviceCallback);
    }

    /**
     * Subscribe to receive samples.
     * Register a client to receive publisher samples through SignalR.
     *
     * @param endpointId The endpoint to subscribe to
     * @param body The user id that will receive publisher samples.
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the {@link ServiceResponse} object if successful.
     */
    public Observable<Void> subscribeAsync(String endpointId, String body) {
        return subscribeWithServiceResponseAsync(endpointId, body).map(new Func1<ServiceResponse<Void>, Void>() {
            @Override
            public Void call(ServiceResponse<Void> response) {
                return response.body();
            }
        });
    }

    /**
     * Subscribe to receive samples.
     * Register a client to receive publisher samples through SignalR.
     *
     * @param endpointId The endpoint to subscribe to
     * @param body The user id that will receive publisher samples.
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the {@link ServiceResponse} object if successful.
     */
    public Observable<ServiceResponse<Void>> subscribeWithServiceResponseAsync(String endpointId, String body) {
        if (endpointId == null) {
            throw new IllegalArgumentException("Parameter endpointId is required and cannot be null.");
        }
        return service.subscribe(endpointId, body)
            .flatMap(new Func1<Response<ResponseBody>, Observable<ServiceResponse<Void>>>() {
                @Override
                public Observable<ServiceResponse<Void>> call(Response<ResponseBody> response) {
                    try {
                        ServiceResponse<Void> clientResponse = subscribeDelegate(response);
                        return Observable.just(clientResponse);
                    } catch (Throwable t) {
                        return Observable.error(t);
                    }
                }
            });
    }

    private ServiceResponse<Void> subscribeDelegate(Response<ResponseBody> response) throws RestException, IOException, IllegalArgumentException {
        return this.restClient().responseBuilderFactory().<Void, RestException>newInstance(this.serializerAdapter())
                .register(200, new TypeToken<Void>() { }.getType())
                .build(response);
    }

    /**
     * Unsubscribe from receiving samples.
     * Unregister a client and stop it from receiving samples.
     *
     * @param endpointId The endpoint to unsubscribe from
     * @param userId The user id that will not receive any more published samples
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @throws RestException thrown if the request is rejected by server
     * @throws RuntimeException all other wrapped checked exceptions if the request fails to be sent
     */
    public void unsubscribe(String endpointId, String userId) {
        unsubscribeWithServiceResponseAsync(endpointId, userId).toBlocking().single().body();
    }

    /**
     * Unsubscribe from receiving samples.
     * Unregister a client and stop it from receiving samples.
     *
     * @param endpointId The endpoint to unsubscribe from
     * @param userId The user id that will not receive any more published samples
     * @param serviceCallback the async ServiceCallback to handle successful and failed responses.
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the {@link ServiceFuture} object
     */
    public ServiceFuture<Void> unsubscribeAsync(String endpointId, String userId, final ServiceCallback<Void> serviceCallback) {
        return ServiceFuture.fromResponse(unsubscribeWithServiceResponseAsync(endpointId, userId), serviceCallback);
    }

    /**
     * Unsubscribe from receiving samples.
     * Unregister a client and stop it from receiving samples.
     *
     * @param endpointId The endpoint to unsubscribe from
     * @param userId The user id that will not receive any more published samples
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the {@link ServiceResponse} object if successful.
     */
    public Observable<Void> unsubscribeAsync(String endpointId, String userId) {
        return unsubscribeWithServiceResponseAsync(endpointId, userId).map(new Func1<ServiceResponse<Void>, Void>() {
            @Override
            public Void call(ServiceResponse<Void> response) {
                return response.body();
            }
        });
    }

    /**
     * Unsubscribe from receiving samples.
     * Unregister a client and stop it from receiving samples.
     *
     * @param endpointId The endpoint to unsubscribe from
     * @param userId The user id that will not receive any more published samples
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the {@link ServiceResponse} object if successful.
     */
    public Observable<ServiceResponse<Void>> unsubscribeWithServiceResponseAsync(String endpointId, String userId) {
        if (endpointId == null) {
            throw new IllegalArgumentException("Parameter endpointId is required and cannot be null.");
        }
        if (userId == null) {
            throw new IllegalArgumentException("Parameter userId is required and cannot be null.");
        }
        return service.unsubscribe(endpointId, userId)
            .flatMap(new Func1<Response<ResponseBody>, Observable<ServiceResponse<Void>>>() {
                @Override
                public Observable<ServiceResponse<Void>> call(Response<ResponseBody> response) {
                    try {
                        ServiceResponse<Void> clientResponse = unsubscribeDelegate(response);
                        return Observable.just(clientResponse);
                    } catch (Throwable t) {
                        return Observable.error(t);
                    }
                }
            });
    }

    private ServiceResponse<Void> unsubscribeDelegate(Response<ResponseBody> response) throws RestException, IOException, IllegalArgumentException {
        return this.restClient().responseBuilderFactory().<Void, RestException>newInstance(this.serializerAdapter())
                .register(200, new TypeToken<Void>() { }.getType())
                .build(response);
    }

    /**
     * Start publishing node values.
     * Start publishing variable node values to IoT Hub. The endpoint must be activated and connected and the module client and server must trust each other.
     *
     * @param endpointId The identifier of the activated endpoint.
     * @param body The publish request
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @throws RestException thrown if the request is rejected by server
     * @throws RuntimeException all other wrapped checked exceptions if the request fails to be sent
     * @return the PublishStartResponseApiModel object if successful.
     */
    public PublishStartResponseApiModel startPublishingValues(String endpointId, PublishStartRequestApiModel body) {
        return startPublishingValuesWithServiceResponseAsync(endpointId, body).toBlocking().single().body();
    }

    /**
     * Start publishing node values.
     * Start publishing variable node values to IoT Hub. The endpoint must be activated and connected and the module client and server must trust each other.
     *
     * @param endpointId The identifier of the activated endpoint.
     * @param body The publish request
     * @param serviceCallback the async ServiceCallback to handle successful and failed responses.
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the {@link ServiceFuture} object
     */
    public ServiceFuture<PublishStartResponseApiModel> startPublishingValuesAsync(String endpointId, PublishStartRequestApiModel body, final ServiceCallback<PublishStartResponseApiModel> serviceCallback) {
        return ServiceFuture.fromResponse(startPublishingValuesWithServiceResponseAsync(endpointId, body), serviceCallback);
    }

    /**
     * Start publishing node values.
     * Start publishing variable node values to IoT Hub. The endpoint must be activated and connected and the module client and server must trust each other.
     *
     * @param endpointId The identifier of the activated endpoint.
     * @param body The publish request
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the observable to the PublishStartResponseApiModel object
     */
    public Observable<PublishStartResponseApiModel> startPublishingValuesAsync(String endpointId, PublishStartRequestApiModel body) {
        return startPublishingValuesWithServiceResponseAsync(endpointId, body).map(new Func1<ServiceResponse<PublishStartResponseApiModel>, PublishStartResponseApiModel>() {
            @Override
            public PublishStartResponseApiModel call(ServiceResponse<PublishStartResponseApiModel> response) {
                return response.body();
            }
        });
    }

    /**
     * Start publishing node values.
     * Start publishing variable node values to IoT Hub. The endpoint must be activated and connected and the module client and server must trust each other.
     *
     * @param endpointId The identifier of the activated endpoint.
     * @param body The publish request
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the observable to the PublishStartResponseApiModel object
     */
    public Observable<ServiceResponse<PublishStartResponseApiModel>> startPublishingValuesWithServiceResponseAsync(String endpointId, PublishStartRequestApiModel body) {
        if (endpointId == null) {
            throw new IllegalArgumentException("Parameter endpointId is required and cannot be null.");
        }
        if (body == null) {
            throw new IllegalArgumentException("Parameter body is required and cannot be null.");
        }
        Validator.validate(body);
        return service.startPublishingValues(endpointId, body)
            .flatMap(new Func1<Response<ResponseBody>, Observable<ServiceResponse<PublishStartResponseApiModel>>>() {
                @Override
                public Observable<ServiceResponse<PublishStartResponseApiModel>> call(Response<ResponseBody> response) {
                    try {
                        ServiceResponse<PublishStartResponseApiModel> clientResponse = startPublishingValuesDelegate(response);
                        return Observable.just(clientResponse);
                    } catch (Throwable t) {
                        return Observable.error(t);
                    }
                }
            });
    }

    private ServiceResponse<PublishStartResponseApiModel> startPublishingValuesDelegate(Response<ResponseBody> response) throws RestException, IOException, IllegalArgumentException {
        return this.restClient().responseBuilderFactory().<PublishStartResponseApiModel, RestException>newInstance(this.serializerAdapter())
                .register(200, new TypeToken<PublishStartResponseApiModel>() { }.getType())
                .build(response);
    }

    /**
     * Stop publishing node values.
     * Stop publishing variable node values to IoT Hub. The endpoint must be activated and connected and the module client and server must trust each other.
     *
     * @param endpointId The identifier of the activated endpoint.
     * @param body The unpublish request
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @throws RestException thrown if the request is rejected by server
     * @throws RuntimeException all other wrapped checked exceptions if the request fails to be sent
     * @return the PublishStopResponseApiModel object if successful.
     */
    public PublishStopResponseApiModel stopPublishingValues(String endpointId, PublishStopRequestApiModel body) {
        return stopPublishingValuesWithServiceResponseAsync(endpointId, body).toBlocking().single().body();
    }

    /**
     * Stop publishing node values.
     * Stop publishing variable node values to IoT Hub. The endpoint must be activated and connected and the module client and server must trust each other.
     *
     * @param endpointId The identifier of the activated endpoint.
     * @param body The unpublish request
     * @param serviceCallback the async ServiceCallback to handle successful and failed responses.
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the {@link ServiceFuture} object
     */
    public ServiceFuture<PublishStopResponseApiModel> stopPublishingValuesAsync(String endpointId, PublishStopRequestApiModel body, final ServiceCallback<PublishStopResponseApiModel> serviceCallback) {
        return ServiceFuture.fromResponse(stopPublishingValuesWithServiceResponseAsync(endpointId, body), serviceCallback);
    }

    /**
     * Stop publishing node values.
     * Stop publishing variable node values to IoT Hub. The endpoint must be activated and connected and the module client and server must trust each other.
     *
     * @param endpointId The identifier of the activated endpoint.
     * @param body The unpublish request
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the observable to the PublishStopResponseApiModel object
     */
    public Observable<PublishStopResponseApiModel> stopPublishingValuesAsync(String endpointId, PublishStopRequestApiModel body) {
        return stopPublishingValuesWithServiceResponseAsync(endpointId, body).map(new Func1<ServiceResponse<PublishStopResponseApiModel>, PublishStopResponseApiModel>() {
            @Override
            public PublishStopResponseApiModel call(ServiceResponse<PublishStopResponseApiModel> response) {
                return response.body();
            }
        });
    }

    /**
     * Stop publishing node values.
     * Stop publishing variable node values to IoT Hub. The endpoint must be activated and connected and the module client and server must trust each other.
     *
     * @param endpointId The identifier of the activated endpoint.
     * @param body The unpublish request
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the observable to the PublishStopResponseApiModel object
     */
    public Observable<ServiceResponse<PublishStopResponseApiModel>> stopPublishingValuesWithServiceResponseAsync(String endpointId, PublishStopRequestApiModel body) {
        if (endpointId == null) {
            throw new IllegalArgumentException("Parameter endpointId is required and cannot be null.");
        }
        if (body == null) {
            throw new IllegalArgumentException("Parameter body is required and cannot be null.");
        }
        Validator.validate(body);
        return service.stopPublishingValues(endpointId, body)
            .flatMap(new Func1<Response<ResponseBody>, Observable<ServiceResponse<PublishStopResponseApiModel>>>() {
                @Override
                public Observable<ServiceResponse<PublishStopResponseApiModel>> call(Response<ResponseBody> response) {
                    try {
                        ServiceResponse<PublishStopResponseApiModel> clientResponse = stopPublishingValuesDelegate(response);
                        return Observable.just(clientResponse);
                    } catch (Throwable t) {
                        return Observable.error(t);
                    }
                }
            });
    }

    private ServiceResponse<PublishStopResponseApiModel> stopPublishingValuesDelegate(Response<ResponseBody> response) throws RestException, IOException, IllegalArgumentException {
        return this.restClient().responseBuilderFactory().<PublishStopResponseApiModel, RestException>newInstance(this.serializerAdapter())
                .register(200, new TypeToken<PublishStopResponseApiModel>() { }.getType())
                .build(response);
    }

    /**
     * Get currently published nodes.
     * Returns currently published node ids for an endpoint. The endpoint must be activated and connected and the module client and server must trust each other.
     *
     * @param endpointId The identifier of the activated endpoint.
     * @param body The list request
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @throws RestException thrown if the request is rejected by server
     * @throws RuntimeException all other wrapped checked exceptions if the request fails to be sent
     * @return the PublishedItemListResponseApiModel object if successful.
     */
    public PublishedItemListResponseApiModel getFirstListOfPublishedNodes(String endpointId, PublishedItemListRequestApiModel body) {
        return getFirstListOfPublishedNodesWithServiceResponseAsync(endpointId, body).toBlocking().single().body();
    }

    /**
     * Get currently published nodes.
     * Returns currently published node ids for an endpoint. The endpoint must be activated and connected and the module client and server must trust each other.
     *
     * @param endpointId The identifier of the activated endpoint.
     * @param body The list request
     * @param serviceCallback the async ServiceCallback to handle successful and failed responses.
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the {@link ServiceFuture} object
     */
    public ServiceFuture<PublishedItemListResponseApiModel> getFirstListOfPublishedNodesAsync(String endpointId, PublishedItemListRequestApiModel body, final ServiceCallback<PublishedItemListResponseApiModel> serviceCallback) {
        return ServiceFuture.fromResponse(getFirstListOfPublishedNodesWithServiceResponseAsync(endpointId, body), serviceCallback);
    }

    /**
     * Get currently published nodes.
     * Returns currently published node ids for an endpoint. The endpoint must be activated and connected and the module client and server must trust each other.
     *
     * @param endpointId The identifier of the activated endpoint.
     * @param body The list request
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the observable to the PublishedItemListResponseApiModel object
     */
    public Observable<PublishedItemListResponseApiModel> getFirstListOfPublishedNodesAsync(String endpointId, PublishedItemListRequestApiModel body) {
        return getFirstListOfPublishedNodesWithServiceResponseAsync(endpointId, body).map(new Func1<ServiceResponse<PublishedItemListResponseApiModel>, PublishedItemListResponseApiModel>() {
            @Override
            public PublishedItemListResponseApiModel call(ServiceResponse<PublishedItemListResponseApiModel> response) {
                return response.body();
            }
        });
    }

    /**
     * Get currently published nodes.
     * Returns currently published node ids for an endpoint. The endpoint must be activated and connected and the module client and server must trust each other.
     *
     * @param endpointId The identifier of the activated endpoint.
     * @param body The list request
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the observable to the PublishedItemListResponseApiModel object
     */
    public Observable<ServiceResponse<PublishedItemListResponseApiModel>> getFirstListOfPublishedNodesWithServiceResponseAsync(String endpointId, PublishedItemListRequestApiModel body) {
        if (endpointId == null) {
            throw new IllegalArgumentException("Parameter endpointId is required and cannot be null.");
        }
        if (body == null) {
            throw new IllegalArgumentException("Parameter body is required and cannot be null.");
        }
        Validator.validate(body);
        return service.getFirstListOfPublishedNodes(endpointId, body)
            .flatMap(new Func1<Response<ResponseBody>, Observable<ServiceResponse<PublishedItemListResponseApiModel>>>() {
                @Override
                public Observable<ServiceResponse<PublishedItemListResponseApiModel>> call(Response<ResponseBody> response) {
                    try {
                        ServiceResponse<PublishedItemListResponseApiModel> clientResponse = getFirstListOfPublishedNodesDelegate(response);
                        return Observable.just(clientResponse);
                    } catch (Throwable t) {
                        return Observable.error(t);
                    }
                }
            });
    }

    private ServiceResponse<PublishedItemListResponseApiModel> getFirstListOfPublishedNodesDelegate(Response<ResponseBody> response) throws RestException, IOException, IllegalArgumentException {
        return this.restClient().responseBuilderFactory().<PublishedItemListResponseApiModel, RestException>newInstance(this.serializerAdapter())
                .register(200, new TypeToken<PublishedItemListResponseApiModel>() { }.getType())
                .build(response);
    }

    /**
     * Get next set of published nodes.
     * Returns next set of currently published node ids for an endpoint. The endpoint must be activated and connected and the module client and server must trust each other.
     *
     * @param endpointId The identifier of the activated endpoint.
     * @param continuationToken The continuation token to continue with
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @throws RestException thrown if the request is rejected by server
     * @throws RuntimeException all other wrapped checked exceptions if the request fails to be sent
     * @return the PublishedItemListResponseApiModel object if successful.
     */
    public PublishedItemListResponseApiModel getNextListOfPublishedNodes(String endpointId, String continuationToken) {
        return getNextListOfPublishedNodesWithServiceResponseAsync(endpointId, continuationToken).toBlocking().single().body();
    }

    /**
     * Get next set of published nodes.
     * Returns next set of currently published node ids for an endpoint. The endpoint must be activated and connected and the module client and server must trust each other.
     *
     * @param endpointId The identifier of the activated endpoint.
     * @param continuationToken The continuation token to continue with
     * @param serviceCallback the async ServiceCallback to handle successful and failed responses.
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the {@link ServiceFuture} object
     */
    public ServiceFuture<PublishedItemListResponseApiModel> getNextListOfPublishedNodesAsync(String endpointId, String continuationToken, final ServiceCallback<PublishedItemListResponseApiModel> serviceCallback) {
        return ServiceFuture.fromResponse(getNextListOfPublishedNodesWithServiceResponseAsync(endpointId, continuationToken), serviceCallback);
    }

    /**
     * Get next set of published nodes.
     * Returns next set of currently published node ids for an endpoint. The endpoint must be activated and connected and the module client and server must trust each other.
     *
     * @param endpointId The identifier of the activated endpoint.
     * @param continuationToken The continuation token to continue with
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the observable to the PublishedItemListResponseApiModel object
     */
    public Observable<PublishedItemListResponseApiModel> getNextListOfPublishedNodesAsync(String endpointId, String continuationToken) {
        return getNextListOfPublishedNodesWithServiceResponseAsync(endpointId, continuationToken).map(new Func1<ServiceResponse<PublishedItemListResponseApiModel>, PublishedItemListResponseApiModel>() {
            @Override
            public PublishedItemListResponseApiModel call(ServiceResponse<PublishedItemListResponseApiModel> response) {
                return response.body();
            }
        });
    }

    /**
     * Get next set of published nodes.
     * Returns next set of currently published node ids for an endpoint. The endpoint must be activated and connected and the module client and server must trust each other.
     *
     * @param endpointId The identifier of the activated endpoint.
     * @param continuationToken The continuation token to continue with
     * @throws IllegalArgumentException thrown if parameters fail the validation
     * @return the observable to the PublishedItemListResponseApiModel object
     */
    public Observable<ServiceResponse<PublishedItemListResponseApiModel>> getNextListOfPublishedNodesWithServiceResponseAsync(String endpointId, String continuationToken) {
        if (endpointId == null) {
            throw new IllegalArgumentException("Parameter endpointId is required and cannot be null.");
        }
        if (continuationToken == null) {
            throw new IllegalArgumentException("Parameter continuationToken is required and cannot be null.");
        }
        return service.getNextListOfPublishedNodes(endpointId, continuationToken)
            .flatMap(new Func1<Response<ResponseBody>, Observable<ServiceResponse<PublishedItemListResponseApiModel>>>() {
                @Override
                public Observable<ServiceResponse<PublishedItemListResponseApiModel>> call(Response<ResponseBody> response) {
                    try {
                        ServiceResponse<PublishedItemListResponseApiModel> clientResponse = getNextListOfPublishedNodesDelegate(response);
                        return Observable.just(clientResponse);
                    } catch (Throwable t) {
                        return Observable.error(t);
                    }
                }
            });
    }

    private ServiceResponse<PublishedItemListResponseApiModel> getNextListOfPublishedNodesDelegate(Response<ResponseBody> response) throws RestException, IOException, IllegalArgumentException {
        return this.restClient().responseBuilderFactory().<PublishedItemListResponseApiModel, RestException>newInstance(this.serializerAdapter())
                .register(200, new TypeToken<PublishedItemListResponseApiModel>() { }.getType())
                .build(response);
    }

}