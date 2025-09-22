using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Easy.EasyAsset
{
    public class EasyAssetConfigInfo
    {
        public string key;
        public string asset;
        public Type type;
        public long changeTag;
        public string abName;
        public bool isBind;
        public bool isRaw;
        public bool isActive;
        public List<EasyAssetConfigInfo> childAssets = new List<EasyAssetConfigInfo>();
        public List<string> beABUsed = new List<string>();
        public List<string> beGroupUsed = new List<string>();
        public List<string> bePackageUsed = new List<string>();

        public void AddChildAsset(EasyAssetConfigInfo childAsset)
        {
            if(!childAssets.Contains(childAsset))
            {
                childAssets.Add(childAsset);
            }
        }

        public void AddBeUsedAB(string beUsedABName)
        {
            if(!beABUsed.Contains(beUsedABName))
            {
                beABUsed.Add(beUsedABName);
            }
        }

        public void AddBeUsedGroup(string beUsedGroup)
        {
            if(!beGroupUsed.Contains(beUsedGroup))
            {
                beGroupUsed.Add(beUsedGroup);
            }
        }

        public void AddBeUsedPackage(string beUsedPackageName)
        {
            if(!bePackageUsed.Contains(beUsedPackageName))
            {
                bePackageUsed.Add(beUsedPackageName);
            }
        }

        public void AddChildAssets(List<EasyAssetConfigInfo> childAssets)
        {
            foreach (EasyAssetConfigInfo childAsset in childAssets)
            {
                if(!this.childAssets.Contains(childAsset))
                {
                    this.childAssets.Add(childAsset);
                }
            }
        }
    
        public void AddBeUsedABs(List<string> beUsedABNames)
        {
            foreach (string beUsedABName in beUsedABNames)
            {
                if(!beABUsed.Contains(beUsedABName))
                {
                    beABUsed.Add(beUsedABName);
                }
            }
        }

        public void AddBeUsedGroups(List<string> beUsedGroups)
        {
            foreach (string beUsedGroup in beUsedGroups)
            {
                if(!beGroupUsed.Contains(beUsedGroup))
                {
                    beGroupUsed.Add(beUsedGroup);
                }
            }
        }

        public void AddBeUsedPackages(List<string> beUsedPackageNames)
        {
            foreach (string beUsedPackageName in beUsedPackageNames)
            {
                if(!bePackageUsed.Contains(beUsedPackageName))
                {
                    bePackageUsed.Add(beUsedPackageName);
                }
            }
        }

        public List<string> GetDependABs(List<EasyAssetConfigInfo> openList = null)
        {
            List<string> abNames = new List<string>();

            if(openList == null)
            {
                openList = new List<EasyAssetConfigInfo>();
            }

            for(int i = 0; i < childAssets.Count; ++i)
            {
                if(!openList.Contains(childAssets[i]))
                {
                    openList.Add(childAssets[i]);
                    List<string> tempList = childAssets[i].GetDependABs(openList);
                    foreach(var tempName in tempList)
                    {
                        if(!abNames.Contains(tempName))
                        {
                            abNames.Add(tempName);
                        }
                    }
                    
                }
            }
            if(!abNames.Contains(abName))
            {
                abNames.Add(abName);
            }
            return abNames;
        }
    }
}
