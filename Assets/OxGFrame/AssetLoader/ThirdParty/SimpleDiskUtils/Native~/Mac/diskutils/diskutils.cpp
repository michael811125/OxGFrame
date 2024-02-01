//
//  diskutils.cpp
//  diskutils
//
//  Created by dikra-prasetya on 3/28/16.
//  Copyright © 2016 dikra-prasetya. All rights reserved.
//


#include "diskutils.hpp"

int getTotalDiskSpace(){
    struct statfs statf;
    
    statfs(".", &statf);
    
    char buf[12];
    sprintf(buf, "%llu", statf.f_blocks * statf.f_bsize / 1048576ULL );
    int ret;
    sscanf(buf, "%d", &ret);
    
    return ret;
}

int getAvailableDiskSpace(){
    struct statfs statf;
    
    statfs(".", &statf);
    
    char buf[12];
    sprintf(buf, "%llu", statf.f_bavail * statf.f_bsize /1048576ULL);
    int ret;
    sscanf(buf, "%d", &ret);
    
    return ret;
}

int getBusyDiskSpace(){
    struct statfs statf;
    
    statfs(".", &statf);
    
    char buf[12];
    sprintf(buf, "%llu", (statf.f_blocks - statf.f_bfree) * statf.f_bsize /1048576ULL);
    int ret;
    sscanf(buf, "%d", &ret);
    
    return ret;
}