package com.unity3d.player;

import android.app.Activity;
import android.util.Log;
import android.view.ViewDebug;
import android.widget.Toast;

import org.jetbrains.annotations.Nullable;

import com.android.billingclient.api.BillingClient;
import com.android.billingclient.api.BillingClientStateListener;
import com.android.billingclient.api.BillingResult;
import com.android.billingclient.api.Purchase;
import com.android.billingclient.api.PurchasesUpdatedListener;
import com.android.billingclient.api.SkuDetails;
import com.android.billingclient.api.SkuDetailsResponseListener;

import org.json.JSONException;
import org.json.JSONObject;

import java.net.InterfaceAddress;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;


class IAPMgr {

    private BillingClient mBillingClient;
    private boolean mIsServiceConnected;
    private @BillingClient.BillingResponseCode int  mBillingClientResponseCode;

    private static String TAG = "google iap";
    private static String UnityGameObject = "IAP";

    private BillingManager mBillingManager;
    private List<SkuDetails> mSkuDetails = new ArrayList<>();
    private List<SkuDetails> mSubscribeDetails = new ArrayList<>();

    private Boolean getSkuDetails = false;
    private Boolean getSubscribeDetails = false;

    List<String> skuList = new ArrayList<>(); //商品
    List<String> subList = new ArrayList<>(); //订阅

    Map<String, Purchase> _PurchaseMap = new HashMap<String, Purchase>();

    JSONObject purchaseCfg = null;

    private Activity context;

    private static IAPMgr instance;

    //建立连接
    public void init(Activity ctx, String uGObj, String base64EncodedPublicKey,String json){
        instance = this;
        UnityGameObject = uGObj;
        context = ctx;

        try {
            purchaseCfg = new JSONObject(json);
            Iterator iterator = purchaseCfg.keys();
            while(iterator.hasNext()) {
                String key = (String) iterator.next();
                JSONObject purchaseJson = purchaseCfg.getJSONObject(key);
                String sku = purchaseJson.getString("googleplay_id");
                if (purchaseJson.getString("item_type").equals("subs")) {
                    subList.add(sku);
                } else {
                    skuList.add(sku);
                }
            }
        } catch (JSONException e) {
            e.printStackTrace();
        }

        mBillingManager = new BillingManager(ctx, new BillingManager.BillingUpdatesListener() {
            @Override
            public void onBillingClientSetupFinished() {
                //List<String> skuList = new ArrayList<>();
                //skuList.add(SKU_ID);
                handleManagerAndUiReady(BillingClient.SkuType.INAPP, skuList);
                handleManagerAndUiReady(BillingClient.SkuType.SUBS, subList);
                Log.d(TAG, "onBillingClientSetupFinished");
            }

            @Override
            public void onBillingClientSetupFailed(int error) {
                UnityPlayer.UnitySendMessage(UnityGameObject, "OnInitializeFailed", ""+error);
                Log.d(TAG, "onBillingClientSetupFail");
            }

            @Override
            public void onConsumeFinished(BillingResult billingResult, String purchaseToken) {

                Log.d(TAG, "Consumption finished. Purchase token: %s, result: %s" + purchaseToken + " code:" + billingResult.getResponseCode());

                if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK) {
                    // Successfully consumed, so we apply the effects of the item in our
                    // game world's logic, which in our case means filling the gas tank a bit
                    Log.d(TAG, "Consumption successful. Provisioning. purchasetoken:" + purchaseToken);
                    //if (instance.callBack != null){
                        //instance.callBack.consumeResult(true, purchase.getItemType(), purchase.getSku(),purchase.getToken(),purchase.getOrderId(),purchase.getPackageName(),purchase.getDeveloperPayload());
                    //}
                    //String sku = purchase.getSku();
                    //if (instance._PurchaseMap.containsKey(sku)) instance._PurchaseMap.remove(sku);

                } else {
                    Log.d(TAG, "Error while consuming: %s" + billingResult.getResponseCode());

                }

                Log.d(TAG, "End consumption flow.");
            }

            @Override
            public void onAcknowledgeFinished(BillingResult billingResult) {

            }

            @Override
            public void onPurchasesUpdated(List<Purchase> purchases) {
                Log.d(TAG, "onPurchasesUpdated: " + purchases.size());
                for (Purchase purchase : purchases) {

                    if (purchase.getPurchaseState() == Purchase.PurchaseState.PURCHASED) {
                        // Acknowledge purchase and grant the item to the user
                        instance._PurchaseMap.put(purchase.getSku(), purchase);
                        String itemtype = skuList.contains(purchase.getSku()) ? "inapp" : "subs";

                        UnityPlayer.UnitySendMessage(UnityGameObject,"ProcessPurchase", purchase.getOriginalJson());
//                        try {
//                            JSONObject purchaseJson = new JSONObject();
//                            purchaseJson.put("itemtype",itemtype);
//                            purchaseJson.put("token",purchase.getPurchaseToken());
//                            purchaseJson.put("appOrderId",purchase.getOrderId());
//                            purchaseJson.put("isRestore",purchase.isAcknowledged());
//                            purchaseJson.put("packageName",purchase.getPackageName());
//                            purchaseJson.put("productId",purchase.getSku());
//                            UnityPlayer.UnitySendMessage(UnityGameObject,"ProcessPurchase", purchaseJson.toString());
//                        } catch (JSONException e) {
//                            e.printStackTrace();
//                        }

                        //Sdk.getInstance().purchaseCallback(true,itemtype, purchase.getSku(), purchase.getPurchaseToken(), purchase.getOrderId(), purchase.getPackageName(), purchase.getAccountIdentifiers().getObfuscatedAccountId());
                    } else if (purchase.getPurchaseState() == Purchase.PurchaseState.PENDING) {
                        // Here you can confirm to the user that they've started the pending
                        // purchase, and to complete it, they should follow instructions that
                        // are given to them. You can also choose to remind the user in the
                        // future to complete the purchase if you detect that it is still
                        // pending.
                    }
                }
            }

            @Override
            public void onPurchasesFailed(int error) {
                UnityPlayer.UnitySendMessage(UnityGameObject,"OnPurchaseFailed", "error");
            }
        });
    }

    public void consumePurchase(String orderId){
        try {
            for (Purchase p : _PurchaseMap.values()) {
                if(p.getOrderId().equals(orderId) ) {
                    mBillingManager.consumeAsync(p);
                    break;
                }
            }

        } catch (Exception e) {
            //complain("Error consuming. Another async operation in progress.");
            //setWaitScreen(false);
            return;
        }
    }

    public void acknowledgePurchase(String orderId){
        try {
            for (Purchase p : _PurchaseMap.values()) {
                if(p.getOrderId().equals(orderId) ) {
                    mBillingManager.AcknowledgeAsync(p);
                    break;
                }
            }
        } catch (Exception e) {
            //complain("Error consuming. Another async operation in progress.");
            //setWaitScreen(false);
            return;
        }
    }

    ///解析价格传到客户端
    public void analysisProductDetails() {
        JSONObject productDetails = new JSONObject();
        try {
            for (SkuDetails sku : mSkuDetails) {
                productDetails.put(sku.getSku(),sku.getOriginalJson());
            }
        } catch (JSONException e) {
            e.printStackTrace();
        }
        UnityPlayer.UnitySendMessage("IAP", "OnInitialized", productDetails.toString());

        //分析完补单
        mBillingManager.queryPurchases();
    }

    public void onResume() {
        if (mBillingManager != null
                && mBillingManager.getBillingClientResponseCode() == BillingClient.BillingResponseCode.OK) {
            //mBillingManager.queryPurchases();
        }
    }

    public void onDestroy() {
        Log.d(TAG, "Destroying helper.");
        if (mBillingManager != null) {
            mBillingManager.destroy();
        }
    }

    public void repaireProduct(){
        if (mBillingManager != null
                && mBillingManager.getBillingClientResponseCode() == BillingClient.BillingResponseCode.OK) {
            mBillingManager.queryPurchases();
        }
    }

    public void doPay(final String sku, final String payload, final int type) {

        if (mBillingManager != null && mBillingManager.getBillingClientResponseCode() > BillingManager.BILLING_MANAGER_NOT_INITIALIZED) {

            if(type == 0){
                if (mSkuDetails != null && mSkuDetails.size() > 0) {
                    for (SkuDetails detail : mSkuDetails) {
                        if (detail.getSku().equals(sku)) {
                            mBillingManager.initiatePurchaseFlow(detail, payload);
                            return;
                        }
                    }
                }
            }else if( type == 1){
                if (mSubscribeDetails != null && mSubscribeDetails.size() > 0) {
                    for (SkuDetails detail : mSubscribeDetails) {
                        if (detail.getSku().equals(sku)) {
                            mBillingManager.initiatePurchaseFlow(detail, payload);
                            return;
                        }
                    }
                }
            }

            Toast.makeText(this.context, "Inapp not found :"+sku, Toast.LENGTH_LONG).show();
//                cb.purchaseResult(false, "", sku, "","","","");

        }else{
            Toast.makeText(this.context, "Inapp not ready, please try again later", Toast.LENGTH_LONG).show();
//            cb.purchaseResult(false, "", sku, "","","","");
            //TODO 重新初始化流程
        }
    }

    private void handleManagerAndUiReady(@BillingClient.SkuType final String billingType, final List<String> allSkuList) {

        mBillingManager.querySkuDetailsAsync(billingType, allSkuList, new SkuDetailsResponseListener() {
            @Override
            public void onSkuDetailsResponse(BillingResult billingResult, List<SkuDetails> skuDetailsList) {
                if (billingResult.getResponseCode() != BillingClient.BillingResponseCode.OK) {

                    Log.d(TAG, "Unsuccessful query for type: %s . Error code: %s"+ billingType + " " + billingResult.getResponseCode());
                    UnityPlayer.UnitySendMessage(UnityGameObject,"OnInitializeFailed","" + billingResult.getResponseCode());
                } else {
                    //成功地获得了sku详细
                    Log.d(TAG, "successful get sku");
                    if(billingType.equals(BillingClient.SkuType.INAPP)){
                        mSkuDetails.clear();
                        for (SkuDetails details : skuDetailsList) {
                            Log.d(TAG, "check sku:" + details);
                            if(skuList.contains(details.getSku())){
                                mSkuDetails.add(details);
                                Log.d(TAG, "add sku:" + details);
                            }else{
                                Log.d(TAG, "not add sku:" + details + " :skuuu:" + details.getSku());
                            }
                        }

                        getSkuDetails = true;
                    }

                    if(billingType.equals(BillingClient.SkuType.SUBS)){
                        mSubscribeDetails.clear();
                        for (SkuDetails details : skuDetailsList) {
                            Log.d(TAG, "check subscribe sku:" + details);
                            if(subList.contains(details.getSku())){
                                mSubscribeDetails.add(details);
                                Log.d(TAG, "add subscribe sku:" + details);
                            }else{
                                Log.d(TAG, "not subscribe add sku:" + details + " :skuuu:" + details.getSku());
                            }
                        }

                        getSubscribeDetails = true;
                    }

                    if(getSkuDetails && getSubscribeDetails){
                        analysisProductDetails();
                    }
                }

            }
        });

    }
}
