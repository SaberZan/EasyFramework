package com.unity3d.player;


import android.app.Activity;
import android.util.Log;

//import com.android.billing.util.HookUtil;
//import com.android.billing.util.Security;
//import com.android.billing.util.Utility;
import androidx.annotation.NonNull;

import com.android.billingclient.api.AcknowledgePurchaseParams;
import com.android.billingclient.api.AcknowledgePurchaseResponseListener;
import com.android.billingclient.api.BillingClient;
import com.android.billingclient.api.BillingClientStateListener;
import com.android.billingclient.api.BillingFlowParams;
import com.android.billingclient.api.BillingResult;
import com.android.billingclient.api.ConsumeParams;
import com.android.billingclient.api.ConsumeResponseListener;
import com.android.billingclient.api.Purchase;
import com.android.billingclient.api.PurchasesUpdatedListener;
import com.android.billingclient.api.SkuDetails;
import com.android.billingclient.api.SkuDetailsParams;
import com.android.billingclient.api.SkuDetailsResponseListener;

import java.io.IOException;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

/**
 * Date: 2019/10/15
 * Author: anmi
 * Desc:google 支付管理类
 * 处理与Google Play Store的所有交互(通过计费库)，维护与Google Play Store的连接
 * 通过BillingClient缓存临时状态/数据
 */
public class BillingManager implements PurchasesUpdatedListener {

    /**
     * 在BillingManager还没有初始化之前，mBillingClientResponseCode的默认值
     */
    public static final int BILLING_MANAGER_NOT_INITIALIZED = -1;

    private static final String TAG = "google iap";//BillingManager.class.getSimpleName();

    /**
     * BillingClient 实例对象
     **/
    private BillingClient mBillingClient;

    /**
     * 服务连接状态
     */
    private boolean mIsServiceConnected;

    private final BillingUpdatesListener mBillingUpdatesListener;

    private final Activity mActivity;

    private final List<Purchase> mPurchases = new ArrayList<>();

    private Set<String> mTokensToBeConsumed;

    private  int  mBillingClientResponseCode = BILLING_MANAGER_NOT_INITIALIZED;


    /* BASE_64_ENCODED_PUBLIC_KEY should be YOUR APPLICATION'S PUBLIC KEY
     * (that you got from the Google Play developer console). This is not your
     * developer public key, it's the *app-specific* public key.
     *
     * Instead of just storing the entire literal string here embedded in the
     * program,  construct the key at runtime from pieces or
     * use bit manipulation (for example, XOR with some other string) to hide
     * the actual key.  The key itself is not secret information, but we don't
     * want to make it easy for an attacker to replace the public key with one
     * of their own and then fake messages from the server.
     * CONSTRUCT_YOU=base 64公钥
     */
    private static final String BASE_64_ENCODED_PUBLIC_KEY = "CONSTRUCT_YOUR";


    /**
     * 用于监听购买列表完成消费的更新
     */
    public interface BillingUpdatesListener {
        void onBillingClientSetupFinished();

        void onBillingClientSetupFailed(int error);

        void onConsumeFinished(BillingResult billingResult, String purchaseToken);

        void onAcknowledgeFinished(BillingResult billingResult);

        void onPurchasesUpdated(List<Purchase> purchases);

        void onPurchasesFailed(int error);
    }

    /**
     * 用于监听 Google Play Store客户端的连接状态
     */
    public interface ServiceConnectedListener {
        void onServiceConnected(@BillingClient.BillingResponseCode int resultCode);
    }

    public BillingManager(Activity activity, final BillingUpdatesListener updatesListener) {

        Log.d(TAG, "Creating Billing client.");
        this.mActivity = activity;
        this.mBillingUpdatesListener = updatesListener;

        //构建BillingClient
        mBillingClient = BillingClient.newBuilder(mActivity).setListener(this).enablePendingPurchases().build();

        Log.d(TAG, "Starting setup.");

        /**
         *异步启动服务连接Google Play Store客户端
         Start setup. This is asynchronous and the specified listener will be called
         once setup completes.
         It also starts to report all the new purchases through onPurchasesUpdated() callback.
         */
        startServiceConnection(new Runnable() {
            @Override
            public void run() {
                //通知购买客户端监听器 通信已就绪
                mBillingUpdatesListener.onBillingClientSetupFinished();

                //IAB已经建立完成，进行查询我们的库存
                Log.d(TAG, "Setup successful. Querying inventory.");
            }
        },new Runnable() {
            @Override
            public void run() {
                //通知购买客户端监听器 通信已就绪
                mBillingUpdatesListener.onBillingClientSetupFailed(mBillingClientResponseCode);

                //IAB已经建立完成，进行查询我们的库存
                Log.d(TAG, "Setup fail.");
            }
        });

    }

    /**
     * 跨应用查询购买信息，并通过监听器返回结果
     */
    public void queryPurchases() {
        //创建查询任务
        Runnable queryToExecute = new Runnable() {
            @Override
            public void run() {
                long time = System.currentTimeMillis();

                Purchase.PurchasesResult purchasesResult = mBillingClient.queryPurchases(BillingClient.SkuType.INAPP);

                Log.d(TAG, "Querying purchases elapsed time:%s ms" + (System.currentTimeMillis() - time));

                //如果支持订阅，将添加订阅处理
                if (areSubscriptionsSupported()) {
                    //查询订阅
                    Purchase.PurchasesResult subscriptionResult
                            = mBillingClient.queryPurchases(BillingClient.SkuType.SUBS);

                    Log.d(TAG, "Querying purchases and subscriptions elapsed time: %s ms" + (System.currentTimeMillis() - time));

                    Log.d(TAG, "Querying subscriptions result code: %s Purchases size: %s" + subscriptionResult.getResponseCode() +" result:" + subscriptionResult.getPurchasesList().size());

                    if (subscriptionResult.getResponseCode() == BillingClient.BillingResponseCode.OK) {

                        purchasesResult.getPurchasesList().addAll(subscriptionResult.getPurchasesList());

                    } else {
                        Log.d(TAG, "Got an error response trying to query subscription purchases");
                    }
                } else if (purchasesResult.getResponseCode() == BillingClient.BillingResponseCode.OK) {
                    //跳过订阅购买查询，因为它不受支持
                    Log.d(TAG, "Skipped subscription purchases query since they are not supported");
                } else {

                    Log.d(TAG, "queryPurchases() got an error response code: " + purchasesResult.getResponseCode());

                }

                onQueryPurchasesFinished(purchasesResult);


            }
        };

        //执行查询任务的请求
        executeServiceRequest(queryToExecute);
    }


    /**
     * 处理查询购买的结果，并通过监听器通知更新后的列表
     */
    private void onQueryPurchasesFinished(Purchase.PurchasesResult result) {
        // Have we been disposed of in the meantime? If so, or bad result code, then quit
        if (mBillingClient == null || result.getResponseCode() != BillingClient.BillingResponseCode.OK) {

            Log.d(TAG, "Billing client was null or result code (%s)was bad - quitting" + result.getResponseCode());
            return;
        }

        Log.d(TAG, "Query inventory was successful.");

        // Update the UI and purchases inventory with new list of purchases
        mPurchases.clear();

        onPurchasesUpdated(result.getBillingResult(), result.getPurchasesList());
    }


    /**
     * 检查当前客户端是否支持订阅
     * 注意:此方法不会自动重试RESULT_SERVICE_DISCONNECTED。
     * 它只用于queryPurchase执行后， 实现了一个回退机制。
     */
    public boolean areSubscriptionsSupported() {
        BillingResult featureSupported = mBillingClient.isFeatureSupported(BillingClient.FeatureType.SUBSCRIPTIONS);

        if (featureSupported.getResponseCode() != BillingClient.BillingResponseCode.OK) {
            Log.d(TAG, "areSubscriptionsSupported() got an error response: " + featureSupported.getResponseCode());
        }

        return featureSupported.getResponseCode() == BillingClient.BillingResponseCode.OK;
    }

    /**
     * 异步启动服务连接Google Play Store客户端（底层会走AIDL的方式
     * @param executeOnSuccess
     */
    public void startServiceConnection(final Runnable executeOnSuccess, final  Runnable executeOnFail) {
        mBillingClient.startConnection(new BillingClientStateListener() {
            @Override
            public void onBillingSetupFinished(BillingResult billingResult) {
                Log.d(TAG, "Setup finished. Response code: " + billingResult.getResponseCode());

                mBillingClientResponseCode = (int)billingResult.getResponseCode();
                if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK) {
                    // BillingClient已经准备好了。在这里可以查询购买情况。
                    mIsServiceConnected = true;
                    if (executeOnSuccess != null) {
                        executeOnSuccess.run();
                    }
                }else{
                    if (executeOnFail != null) {
                        executeOnFail.run();
                    }
                }

            }

            @Override
            public void onBillingServiceDisconnected() {
                mIsServiceConnected = false;
                if (executeOnFail != null) {
                    executeOnFail.run();
                }
            }
        });
    }

    /**
     * 异步启动服务连接Google Play Store客户端（底层会走AIDL的方式）
     * @param executeOnSuccess
     */
    public void startServiceConnection(final Runnable executeOnSuccess) {
        mBillingClient.startConnection(new BillingClientStateListener() {
            @Override
            public void onBillingSetupFinished(BillingResult billingResult) {
                Log.d(TAG, "Setup finished. Response code: " + billingResult.getResponseCode());

                mBillingClientResponseCode = (int)billingResult.getResponseCode();
                if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK) {
                    // BillingClient已经准备好了。在这里可以查询购买情况。
                    mIsServiceConnected = true;
                    if (executeOnSuccess != null) {
                        executeOnSuccess.run();
                    }
                }

            }

            @Override
            public void onBillingServiceDisconnected() {
                mIsServiceConnected = false;
            }
        });
    }

    /**
     * 处理从帐单库更新购买的回调
     */
    @Override
    public void onPurchasesUpdated(BillingResult billingResult,List<Purchase> purchases) {
        if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK) {
            //购买成功
            for (Purchase purchase : purchases) {
                handlePurchase(purchase);
            }
            mBillingUpdatesListener.onPurchasesUpdated(purchases);
        }  else {
            //出现其他错误，具体查询返回的状态码
            Log.d(TAG, "onPurchasesUpdated() got unknown resultCode: " + billingResult.getResponseCode());
            mBillingUpdatesListener.onPurchasesFailed((int)billingResult.getResponseCode());
        }
    }

    /**
     * 处理采购
     * <p>注意：对于每次购买，都要检查签名在客户端是否有效。
     * 建议将此检查移到后端。
     * 参见{@linkSecurity#verifyPurchase(String, String, String)}
     * </p>
     *
     * @param purchase 待处理的购买信息
     */
    private void handlePurchase(Purchase purchase) {
//        if (!verifyValidSignature(purchase.getOriginalJson(), purchase.getSignature())) {
//
//            Log.d(TAG, "Got a purchase: %s : but signature is bad. Skipping..." + purchase);
//
//            return;
//        }

        Log.d(TAG, "Got a verified purchase: " + purchase);

        mPurchases.add(purchase);
    }

    /**
     * 验证应用 的BASE_64_ENCODED_PUBLIC_KEY是否签属了购买
     * <p>
     * 强烈建议以下的判断放至业务后台进行处理判断，因为黑客可以进行反编译提取到BASE_64_ENCODED_PUBLIC_KEY，并重建应用，用“constant true”替换这个方法
     * 此处默认改为false
     */
    private boolean verifyValidSignature(String signedData, String signature) {
        // Some sanity checks to see if the developer (that's you!) really followed the
        // instructions to run this sample (don't put these checks on your app!)
//        if (BASE_64_ENCODED_PUBLIC_KEY.contains("CONSTRUCT_YOUR")) {
//            throw new RuntimeException("Please update your app's public key at: "
//                    + "BASE_64_ENCODED_PUBLIC_KEY");
//        }
//
//        try {
//            return Security.verifyPurchase(BASE_64_ENCODED_PUBLIC_KEY, signedData, signature);
//        } catch (IOException e) {
//
//            Log.d(TAG, "Got an exception trying to validate a purchase: " + e);
//
//            return false;
//        }
        return false;
    }


    /**
     * 启动购买或订阅流程
     */
    public void initiatePurchaseFlow(final SkuDetails skuDetails, final String developerPayload) {
        Runnable purchaseFlowRequest = new Runnable() {
            @Override
            public void run() {
                //HookUtil.doSetPayloadToBillingClient(mBillingClient,developerPayload);

                BillingFlowParams.Builder builder = BillingFlowParams.newBuilder();

                builder.setSkuDetails(skuDetails);
                builder.setObfuscatedAccountId(developerPayload);

                mBillingClient.launchBillingFlow(mActivity, builder.build());
            }
        };

        executeServiceRequest(purchaseFlowRequest);
    }


    /**
     * 异步查询商品信息
     *
     * @param itemType
     * @param skuList
     * @param listener
     */
    public void querySkuDetailsAsync(@BillingClient.SkuType final String itemType, final List<String> skuList,
                                     final SkuDetailsResponseListener listener) {
        // Creating a runnable from the request to use it inside our connection retry policy below
        Runnable queryRequest = new Runnable() {
            @Override
            public void run() {
                // Query the purchase async
                SkuDetailsParams.Builder params = SkuDetailsParams.newBuilder();
                params.setSkusList(skuList).setType(itemType);
                mBillingClient.querySkuDetailsAsync(params.build(), new SkuDetailsResponseListener() {
                    @Override
                    public void onSkuDetailsResponse(BillingResult billingResult, List<SkuDetails> list) {
                        listener.onSkuDetailsResponse(billingResult, list);
                    }
                });
            }
        };

        executeServiceRequest(queryRequest);
    }


    /**
     * 异步消耗商品
     * 只有消费成功之后，才能真正到账，否则3天之后，会执行退款处理 测试阶段只有5分钟
     *
     * @param purchase
     */
    public void consumeAsync(final Purchase purchase) {
        // If we've already scheduled to consume this token - no action is needed (this could happen
        // if you received the token when querying purchases inside onReceive() and later from
        // onActivityResult()
        if (mTokensToBeConsumed == null) {
            mTokensToBeConsumed = new HashSet<>();
        } else if (mTokensToBeConsumed.contains(purchase.getPurchaseToken())) {

            Log.d(TAG, "Token was already scheduled to be consumed - skipping...");

            return;
        }
        mTokensToBeConsumed.add(purchase.getPurchaseToken());


        // Generating Consume Response listener
        final ConsumeResponseListener onConsumeListener = new ConsumeResponseListener() {

            @Override
            public void onConsumeResponse(BillingResult billingResult, String purchaseToken) {
                Log.d(TAG, "onConsumeResponse code = %s  ,msg = %s ,purchaseToken = %s" + billingResult.getResponseCode() + " "+ billingResult.getDebugMessage()+ " " + purchaseToken);

                if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK) {
                    //消耗成功

                } else {
                    // 消费失败,后面查询消费记录后再次消费，否则，就只能等待退款
                }

                mBillingUpdatesListener.onConsumeFinished(billingResult, purchaseToken);
            }
        };

        // Creating a runnable from the request to use it inside our connection retry policy below
        Runnable consumeRequest = new Runnable() {
            @Override
            public void run() {
                // Consume the purchase async
                ConsumeParams consumeParams = ConsumeParams.newBuilder()
                        .setPurchaseToken(purchase.getPurchaseToken()).build();
                mBillingClient.consumeAsync(consumeParams, onConsumeListener);
            }
        };

        executeServiceRequest(consumeRequest);
    }

    public void AcknowledgeAsync(final Purchase purchase){
        if (purchase.getPurchaseState() == Purchase.PurchaseState.PURCHASED) {
            if (!purchase.isAcknowledged()) {
                // Generating Consume Response listener
                final AcknowledgePurchaseResponseListener acknowledgePurchaseResponseListener = new AcknowledgePurchaseResponseListener() {
                    @Override
                    public void onAcknowledgePurchaseResponse(@NonNull BillingResult billingResult) {
                        if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK) {
                            //消耗成功

                        } else {
                            // 消费失败,后面查询消费记录后再次消费，否则，就只能等待退款
                        }
                        mBillingUpdatesListener.onAcknowledgeFinished(billingResult);
                    }
                };

                // Creating a runnable from the request to use it inside our connection retry policy below
                Runnable acknowledgeRequest = new Runnable() {
                    @Override
                    public void run() {
                        // Consume the purchase async
                        AcknowledgePurchaseParams acknowledgePurchaseParams =
                                AcknowledgePurchaseParams.newBuilder()
                                        .setPurchaseToken(purchase.getPurchaseToken())
                                        .build();
                        mBillingClient.acknowledgePurchase(acknowledgePurchaseParams, acknowledgePurchaseResponseListener);
                    }
                };

                executeServiceRequest(acknowledgeRequest);
            }
        }
    }



    /**
     * Returns the value Billing client response code or BILLING_MANAGER_NOT_INITIALIZED if the
     * clien connection response was not received yet.
     */
    public int getBillingClientResponseCode() {
        return mBillingClientResponseCode;
    }



    /**
     * 执行任务并判断是否需要重连服务
     * @param runnable
     */
    private void executeServiceRequest(Runnable runnable) {
        if (mIsServiceConnected) {
            runnable.run();
        } else {
            // 如果账单服务被断开，我们尝试重新连接一次.
            // (feel free to introduce your retry policy here).
            startServiceConnection(runnable);
        }
    }

    /**
     * Clear the resources
     * activity -onDestroy调用
     */
    public void destroy() {
        Log.d(TAG,"Destroying the manager.");

        if (mBillingClient != null && mBillingClient.isReady()) {
            mBillingClient.endConnection();
            mBillingClient = null;
        }
    }

}
