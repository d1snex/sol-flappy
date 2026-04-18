// Adds scene:openURLContexts: to Unity's UnityScene (UIWindowSceneDelegate).
// Unity doesn't implement this method, so iOS drops deep link URLs silently.
// Writes the URL to a file that WalletManager.cs reads on app resume.
//
// We intentionally do NOT call UnitySetAbsoluteURL here because the SDK's
// Web3Auth handler crashes on Phantom URLs and kills the event chain.

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@interface UnityScene : UIResponder <UIWindowSceneDelegate>
@end

@interface UnityScene (DeepLinkFix)
@end

@implementation UnityScene (DeepLinkFix)

- (void)scene:(UIScene *)scene openURLContexts:(NSSet<UIOpenURLContext *> *)URLContexts {
    for (UIOpenURLContext *ctx in URLContexts) {
        NSString *urlString = ctx.URL.absoluteString;
        NSString *dir = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES).firstObject;
        NSString *path = [dir stringByAppendingPathComponent:@"_deeplink.txt"];
        [urlString writeToFile:path atomically:YES encoding:NSUTF8StringEncoding error:nil];
    }
}

@end
