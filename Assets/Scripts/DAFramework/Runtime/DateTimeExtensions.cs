using System;

public static class DateTimeExtensions
{
    // 记录本次游戏开始时间
    public static long GameStarTime;
    
    public static long GeUtcToSeconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
    }
    
}