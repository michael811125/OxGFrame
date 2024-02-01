package com.activetheoryinc.diskutils;

/**
 * Created by dikra-prasetya on 4/1/16.
 */


import android.os.Build;
import android.os.Environment;
import android.os.StatFs;
import java.math.BigInteger;


public class DiskUtils {
    private static final long MEGA_BYTE = 1048576;

    /**
     * Calculates total space on disk.
     * @param external  Queries external disk if true, queries internal disk otherwise.
     * @return Total disk space in MB.
     */
    public static int totalSpace(boolean external)
    {
        long totalBlocks;
        long blockSize;

        StatFs statFs = getStats(external);
        if (Build.VERSION.SDK_INT < 18){
            totalBlocks = statFs.getBlockCount();
            blockSize = statFs.getBlockSize();
        }
        else
        {
            totalBlocks = statFs.getBlockCountLong();
            blockSize = statFs.getBlockSizeLong();
        }

        BigInteger total = BigInteger.valueOf(totalBlocks).multiply(BigInteger.valueOf(blockSize)).divide(BigInteger.valueOf(MEGA_BYTE));

        return total.intValue();
    }

    /**
     * Calculates available space on disk.
     * @param path  Gets the disk that contains the path, queries the internal disk if this is null or empty
     * @return Available disk space in MB.
     */
    public static int availableSpace(String path)
    {
        if (path == null || path.isEmpty()) {
            path = Environment.getRootDirectory().getAbsolutePath();
        }

        long totalBlocks;
        long blockSize;

        StatFs statFs = new StatFs(path);
        if (Build.VERSION.SDK_INT < 18) {
            totalBlocks = statFs.getAvailableBlocks();
            blockSize = statFs.getBlockSize();
        } else {
            totalBlocks = statFs.getAvailableBlocksLong();
            blockSize = statFs.getBlockSizeLong();
        }
        BigInteger total = BigInteger.valueOf(totalBlocks).multiply(BigInteger.valueOf(blockSize)).divide(BigInteger.valueOf(MEGA_BYTE));

        return total.intValue();
    }
    /**
     * Calculates available space on disk.
     * @param external  Queries external disk if true, queries internal disk otherwise.
     * @return Available disk space in MB.
     */
    public static int availableSpace(boolean external)
    {
        long availableBlocks;
        long blockSize;

        StatFs statFs = getStats(external);
        if (Build.VERSION.SDK_INT < 18){
            availableBlocks = statFs.getAvailableBlocks();
            blockSize = statFs.getBlockSize();
        }
        else
        {
            availableBlocks = statFs.getAvailableBlocksLong();
            blockSize = statFs.getBlockSizeLong();
        }

        BigInteger free = BigInteger.valueOf(availableBlocks).multiply(BigInteger.valueOf(blockSize)).divide(BigInteger.valueOf(MEGA_BYTE));

        return free.intValue();
    }

    /**
     * Calculates busy space on disk.
     * @param external  Queries external disk if true, queries internal disk otherwise.
     * @return Busy disk space in MB.
     */
    public static int busySpace(boolean external)
    {
        BigInteger total;
        BigInteger free;

        StatFs statFs = getStats(external);

        if (Build.VERSION.SDK_INT < 18){
            total = BigInteger.valueOf(statFs.getBlockCount()).multiply(BigInteger.valueOf(statFs.getBlockSize()));
            free  = BigInteger.valueOf(statFs.getFreeBlocks()).multiply(BigInteger.valueOf(statFs.getBlockSize()));
        }
        else
        {
            total = BigInteger.valueOf(statFs.getBlockCountLong()).multiply(BigInteger.valueOf(statFs.getBlockSizeLong()));
            free  = BigInteger.valueOf(statFs.getFreeBlocksLong()).multiply(BigInteger.valueOf(statFs.getBlockSizeLong()));
        }

        BigInteger ret = total.subtract(free).divide(BigInteger.valueOf(MEGA_BYTE));

        return ret.intValue();
    }

    private static StatFs getStats(boolean external){
        String path;

        if (external){
            path = Environment.getExternalStorageDirectory().getAbsolutePath();
        }
        else{
            path = Environment.getRootDirectory().getAbsolutePath();
        }

        return new StatFs(path);
    }

}

