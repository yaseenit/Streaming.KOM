using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

static class SimpleTimer 
{
    private static Dictionary<string, List<long>> passedTime = new Dictionary<string, List<long>>();
    
    private static Dictionary<string, long> lastBegin = new Dictionary<string, long>();


    public static bool Enabled = true;

    public static bool ShowDirectly = true;

    private static long getUnixMS()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public static void Begin(string key="")
    {
        if(!Enabled)
            return;

        lastBegin[key] = getUnixMS();
    }

    public static void PrintStats(string key)
    {
        if(!passedTime.ContainsKey(key))
        {
            return;
        }

        List<long> durations = passedTime[key];
        Debug.Log("Duration '" + key + "': " + durations[durations.Count - 1] + " ms, avg: " + durations.Average() + ", num calls: " + durations.Count);
    }

    public static void End(string key)
    {
        if(!Enabled)
            return;
        
        long now = getUnixMS();
        if(lastBegin.ContainsKey(key))
        {
            long begin = lastBegin[key];
            long duration = now - begin;

            if(!passedTime.ContainsKey(key))
            {
                passedTime[key] = new List<long>();
            }

            passedTime[key].Add(duration);

            if(ShowDirectly)
            {
                PrintStats(key);
            }
        }
        else
        {
            Debug.LogWarning("You have to call Begin() before End()!");
        }
    }
}