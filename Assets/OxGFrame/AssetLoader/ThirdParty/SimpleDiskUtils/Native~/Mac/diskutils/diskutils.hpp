//
//  diskutils.hpp
//  diskutils
//
//  Created by dikra-prasetya on 3/28/16.
//  Copyright © 2016 dikra-prasetya. All rights reserved.
//

#include <stdio.h>
#include <stdlib.h>
#include <sys/param.h>
#include <sys/mount.h>

#ifndef diskutils_hpp
#define diskutils_hpp

extern "C" {
    int getTotalDiskSpace();
    int getAvailableDiskSpace();
    int getBusyDiskSpace();
}


#endif /* diskutils_hpp */
