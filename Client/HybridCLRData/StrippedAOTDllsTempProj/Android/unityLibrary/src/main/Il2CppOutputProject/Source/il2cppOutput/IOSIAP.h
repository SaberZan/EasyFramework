
#import <Foundation/Foundation.h>

#ifndef IOSIAP_h
#define IOSIAP_h

@interface IOSIAP : NSObject

+(instancetype) shareInstance;

-(void) initIAP:(NSString*) cfg;

-(void) purchase:(NSString *) productIndentifierId;

-(void) restorePurchase;

-(void) finishPurchase: (NSString *)transId;

-(void) sendToUnity: (const char*) obj funcName:(const char*) funcName para:(const char*) para;
@end

#endif //IOSIAP_h
