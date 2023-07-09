
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>


extern "C" {
    
#pragma mark - Functions

    void _ConvertHEICToPNG(const char* heicFilePath, const char* pngFilePath);
}

void _ConvertHEICToPNG(const char* heicFilePath, const char* pngFilePath) {
    @autoreleasepool {
        NSString* heicPath = [NSString stringWithUTF8String:heicFilePath];
        NSString* pngPath = [NSString stringWithUTF8String:pngFilePath];
        
        NSURL* heicURL = [NSURL fileURLWithPath:heicPath];
        NSData* imageData = [NSData dataWithContentsOfURL:heicURL];
        
        UIImage* heicImage = [UIImage imageWithData:imageData];
        NSData* pngData = UIImagePNGRepresentation(heicImage);
        
        NSURL* pngURL = [NSURL fileURLWithPath:pngPath];
        [pngData writeToURL:pngURL atomically:YES];
    }
}
