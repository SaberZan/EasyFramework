
#import <Foundation/Foundation.h>

#ifndef Tool_h
#define Tool_h

@interface Tool : NSObject

+(instancetype) shareInstance;

-(long)GetTotalDiskSpace;

-(long)GetFreeDiskSpace;

@end

#endif //Tool_h