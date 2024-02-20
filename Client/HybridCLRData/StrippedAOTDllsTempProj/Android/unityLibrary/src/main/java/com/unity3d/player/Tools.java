package com.unity3d.player;

import android.content.Context;
import android.os.Environment;
import android.os.StatFs;

import java.util.Locale;
import java.io.File;

class Tools {
    private static Tools _instance;

    public static Tools getInstance(){
        if (_instance == null){
            _instance = new Tools();
        }
        return _instance;
    }

    /*
     * 获取国家码
     * */
    public String getCountryZipCode(Context context) {
        String CountryZipCode = "";
        Locale locale = context.getResources().getConfiguration().locale;
        CountryZipCode = locale.getISO3Country();

        return CountryZipCode;
    }

    // /*
    //  * 获取内存
    //  */
    // public float GetMemory(Activity currentActivity) {
    //     float memory = -1;
    //     try {

    //         int pid = android.os.Process.myPid();
    //         ActivityManager mActivityManager = (ActivityManager) currentActivity
    //                 .getSystemService(Context.ACTIVITY_SERVICE);
    //         Debug.MemoryInfo[] memoryInfoArray = mActivityManager.getProcessMemoryInfo(new int[] { pid });
    //         memory = (float) memoryInfoArray[0].getTotalPrivateDirty() / 1024;
    //     } catch (Exception e) {
    //         if (Utile.isDebug())
    //             Utile.LogError(e.toString());
    //     }
    //     return memory;
    // }

    public static long GetFreeDiskSpace() 
    {
        try 
        {
            File file = Environment.getDataDirectory();
            StatFs sf = new StatFs(file.getPath());
            return sf.getAvailableBytes();
        } 
        catch (Throwable e) 
        {
            return -1;
        }
    }

}
